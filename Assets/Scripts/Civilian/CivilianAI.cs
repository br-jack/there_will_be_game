using Enemy;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

[System.Serializable] public struct RandomMovementSettings
{
    public float radius;
    [FormerlySerializedAs("minInterval")] public float minIdleDuration;
    [FormerlySerializedAs("maxInterval")] public float maxIdleDuration;
    public float speed;
}

[System.Serializable] public struct RunAwaySettings
{
    public float startRunningRadius;
    public float stopRunningRadius;
    public float speed;
}

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class CivilianAI : MonoBehaviour
{
    private enum MovementState { RandomMovement, Idling, RunAway }

    [SerializeField] private RandomMovementSettings randomMovement = new RandomMovementSettings
    {
        radius = 8f,
        minIdleDuration = 1.5f,
        maxIdleDuration = 4f,
        speed = 2f
    };

    [SerializeField] private RunAwaySettings runAway = new RunAwaySettings
    {
        startRunningRadius = 10f,
        stopRunningRadius = 18f,
        speed = 6f
    };

    [Header("Movement")]
    [SerializeField, Range(0f, 1f)] private float smoothVelocity = 0.7f;
    [SerializeField] private float rotationSpeed = 16f;

    [Header("Animation (optional)")]
    [SerializeField] private Animator anim;
    [SerializeField] private string speedParam = "Speed";

    private MovementState state = MovementState.RandomMovement;
    private NavMeshAgent agent;
    private Rigidbody rb;
    private Transform playerRef;
    private float currentSpeed;
    private float idleEndTime;

    // Stuck-escape: if commanded velocity is non-trivial but the civilian hasn't
    // actually translated for StuckTimeThreshold, sidestep for UnstuckDuration
    // perpendicular to the intended move direction to break free.
    private const float StuckTimeThreshold = 0.9f;
    private const float StuckMovedSqr = 0.04f;
    private const float StuckCommandedSqr = 1f;
    private const float UnstuckDuration = 0.3f;
    private Vector3 _stuckCheckPos;
    private float _stuckCheckTime;
    private float _unstuckUntil;
    private Vector3 _unstuckDir;

    // Deadzone for facing direction — only retarget rotation when desiredVelocity differs
    // from the last committed direction by more than this many degrees. Stops the civilian
    // chasing micro-swings caused by NavMesh obstacle-avoidance churn.
    private const float FacingRetargetAngle = 10f;
    private Vector3 _stableFacingDir;

    // Stall detection: if the civilian thinks it has a valid path but hasn't actually
    // translated in StallTime, abandon the destination so they don't lean into a fence
    // forever. The brief stuck-escape sidestep isn't enough on its own — it nudges, but
    // the path is still pointing into the wall, so they immediately resume pressing.
    private const float StallTime = 1.5f;
    private const float StallProgressDist = 0.5f;
    private Vector3 _progressPos;
    private float _progressTime;

    // When picking a flee destination, a candidate point inside a building gets snapped to
    // the nearest navmesh — usually right against the building's wall. Reject candidates
    // whose horizontal snap distance exceeds this, since they end up wall-adjacent.
    private const float RunAwayMaxHorizontalSnap = 1.5f;

    public IDeathState DeathHandler { get; private set; }
    public IKnockbackState KnockbackHandler { get; private set; }
    
    [Header("Ragdoll")]
    [SerializeField] private RagdollToggler ragdollToggler;

    void Awake()
    {
        if (anim == null) anim = GetComponentInChildren<Animator>();

        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.angularSpeed = 0f;

        _stuckCheckPos = transform.position;
        _stuckCheckTime = Time.time;

        KnockbackHandler = GetComponent<KnockbackHandler>();
        Debug.Assert(KnockbackHandler != null);
        RagdollDeathHandler deathHandler = GetComponent<RagdollDeathHandler>();
        if (deathHandler != null) 
        {
            deathHandler.Init(ragdollToggler, KnockbackHandler);
            DeathHandler = deathHandler;
        }
        Debug.Assert(DeathHandler != null);

        EnsureOnNavMesh();
    }

    void Start()
    {
        ResolvePlayerRef();
        EnterRandomMovement();
    }

    void Update()
    {
        if (playerRef == null) { ResolvePlayerRef(); return; }

        float distToPlayer = HorizontalDistance(transform.position, playerRef.position);

        // Run-away preempts everything.
        if (state != MovementState.RunAway && distToPlayer < runAway.startRunningRadius)
        {
            EnterRunAway();
        }
        else if (state == MovementState.RunAway && distToPlayer > runAway.stopRunningRadius)
        {
            EnterRandomMovement();
        }

        switch (state)
        {
            case MovementState.RandomMovement:
                // Arrived, or the pathfinder gave up — either way, pause and pick again later.
                if (!agent.pathPending && (!agent.hasPath || agent.remainingDistance < 0.5f))
                {
                    EnterIdling();
                    break;
                }
                if (IsStalled()) EnterIdling();
                break;

            case MovementState.Idling:
                if (Time.time >= idleEndTime) EnterRandomMovement();
                break;

            case MovementState.RunAway:
                // Re-pick a flee target only when we arrive (or the path failed). Re-picking
                // on a timer caused visible direction churn — the civilian commits to a
                // direction now and only re-evaluates once they've used the current one.
                if (!agent.pathPending && (!agent.hasPath || agent.remainingDistance < 0.5f))
                {
                    PickNewRunAwayPoint();
                    ResetProgressTracking();
                    break;
                }
                if (IsStalled())
                {
                    PickNewRunAwayPoint();
                    ResetProgressTracking();
                }
                break;
        }

        UpdateAnim();
    }

    void FixedUpdate()
    {
        Vector3 moveDir = Vector3.zero;
        Vector3 facingDir = Vector3.zero;
        if (agent.isOnNavMesh)
        {
            // Movement reads desiredVelocity (includes avoidance, so we physically swerve
            // around other civilians).
            Vector3 desiredVel = agent.desiredVelocity;
            desiredVel.y = 0f;
            if (desiredVel.sqrMagnitude > 0.0001f) moveDir = desiredVel.normalized;

            // Rotation reads the steering target (next path corner) instead — desiredVelocity
            // flips left/right as the avoidance solver picks vectors when civilians cluster,
            // and that flipping shows up as visible rotation spasm. Path corners are stable
            // until reached, the same way the enemy's toPlayerDir is stable.
            if (agent.hasPath)
            {
                Vector3 toCorner = agent.steeringTarget - transform.position;
                toCorner.y = 0f;
                if (toCorner.sqrMagnitude > 0.04f) facingDir = toCorner.normalized;
            }
        }

        if (facingDir.sqrMagnitude > 0.0001f)
        {
            if (_stableFacingDir.sqrMagnitude < 0.0001f
                || Vector3.Angle(_stableFacingDir, facingDir) > FacingRetargetAngle)
            {
                _stableFacingDir = facingDir;
            }
            Quaternion targetRot = Quaternion.LookRotation(_stableFacingDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * rotationSpeed);
        }

        ApplyVelocity(moveDir * currentSpeed);

        if (agent.isOnNavMesh) agent.nextPosition = transform.position;

        UpdateStuckEscape(moveDir);
    }

    private void UpdateStuckEscape(Vector3 moveDir)
    {
        if (Time.time < _unstuckUntil) return; // already escaping; don't update detection

        Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        bool commandingMovement = horizontalVel.sqrMagnitude > StuckCommandedSqr;
        bool moved = (transform.position - _stuckCheckPos).sqrMagnitude > StuckMovedSqr;

        if (!commandingMovement || moved)
        {
            _stuckCheckPos = transform.position;
            _stuckCheckTime = Time.time;
            return;
        }

        if (Time.time - _stuckCheckTime <= StuckTimeThreshold) return;

        // Stuck — sidestep perpendicular to the intended move direction. Random
        // side so two civilians wedged against each other don't pick the same way.
        Vector3 right = Vector3.Cross(Vector3.up, moveDir);
        if (right.sqrMagnitude < 0.0001f) right = Vector3.right;
        right.Normalize();
        _unstuckDir = Random.value < 0.5f ? right : -right;
        _unstuckUntil = Time.time + UnstuckDuration;
        _stuckCheckPos = transform.position;
        _stuckCheckTime = Time.time;
    }

    private void ApplyVelocity(Vector3 desired)
    {
        // While escaping a stuck position, override whatever the AI wanted with a
        // sideways push at the current state's speed.
        if (Time.time < _unstuckUntil)
        {
            desired = _unstuckDir * currentSpeed;
        }

        if (desired.sqrMagnitude < 0.0001f)
        {
            // Not trying to move: hard-stop horizontal drift so physics can't push us around.
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        rb.linearVelocity = Vector3.Lerp(
            rb.linearVelocity,
            new Vector3(desired.x, rb.linearVelocity.y, desired.z),
            smoothVelocity);
    }

    private void UpdateAnim()
    {
        if (anim == null || anim.runtimeAnimatorController == null || string.IsNullOrEmpty(speedParam)) return;
        Vector3 v = rb.linearVelocity;
        anim.SetFloat(speedParam, new Vector2(v.x, v.z).magnitude);
    }

    private void EnterRandomMovement()
    {
        state = MovementState.RandomMovement;
        currentSpeed = randomMovement.speed;
        agent.speed = currentSpeed;
        ResetProgressTracking();
        PickNewRandomPoint();
    }

    private void EnterIdling()
    {
        state = MovementState.Idling;
        currentSpeed = 0f;
        if (agent.isOnNavMesh) agent.ResetPath();
        idleEndTime = Time.time + Random.Range(randomMovement.minIdleDuration, randomMovement.maxIdleDuration);
    }

    private void EnterRunAway()
    {
        state = MovementState.RunAway;
        currentSpeed = runAway.speed;
        agent.speed = currentSpeed;
        ResetProgressTracking();
        PickNewRunAwayPoint();
    }

    private void ResetProgressTracking()
    {
        _progressPos = transform.position;
        _progressTime = Time.time;
    }

    private bool IsStalled()
    {
        if ((transform.position - _progressPos).sqrMagnitude > StallProgressDist * StallProgressDist)
        {
            _progressPos = transform.position;
            _progressTime = Time.time;
            return false;
        }
        return Time.time - _progressTime > StallTime;
    }

    private void PickNewRandomPoint()
    {
        if (!agent.isOnNavMesh) return;

        // Small sample radius + path validation prevents picking a candidate inside a building
        // that snaps onto the navmesh right up against an outside wall — the civilian would
        // then walk straight at the wall instead of routing around the building.
        NavMeshPath path = new NavMeshPath();
        for (int attempt = 0; attempt < 8; attempt++)
        {
            Vector3 candidate = transform.position + Random.insideUnitSphere * randomMovement.radius;
            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas)) continue;
            if (!agent.CalculatePath(hit.position, path) || path.status != NavMeshPathStatus.PathComplete) continue;

            agent.SetDestination(hit.position);
            return;
        }
    }

    private void PickNewRunAwayPoint()
    {
        if (!agent.isOnNavMesh || playerRef == null) return;

        Vector3 awayDir = transform.position - playerRef.position;
        awayDir.y = 0f;
        if (awayDir.sqrMagnitude < 0.0001f) return;
        awayDir.Normalize();

        // 11 angles spanning ±150° around the away vector — smaller offsets first so the
        // civilian prefers the most direct flee direction when one is available.
        float[] angleOffsets = { 0f, 30f, -30f, 60f, -60f, 90f, -90f, 120f, -120f, 150f, -150f };
        NavMeshPath path = new NavMeshPath();
        float currentDist = HorizontalDistance(transform.position, playerRef.position);
        float maxSnapSqr = RunAwayMaxHorizontalSnap * RunAwayMaxHorizontalSnap;

        Vector3 fallbackDest = Vector3.zero;
        bool hasFallback = false;

        foreach (float angle in angleOffsets)
        {
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * awayDir;
            Vector3 candidate = transform.position + dir * runAway.stopRunningRadius;

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 3f, NavMesh.AllAreas)) continue;
            if (!agent.CalculatePath(hit.position, path) || path.status != NavMeshPathStatus.PathComplete) continue;
            if (HorizontalDistance(hit.position, playerRef.position) <= currentDist) continue;

            // Strict criteria: SamplePosition didn't have to snap far horizontally (i.e., the
            // candidate wasn't inside a building), AND a navmesh-raycast from civilian to the
            // destination is unobstructed (so the first leg of the path won't visibly head at
            // a wall before routing around it).
            Vector3 horizSnap = candidate - hit.position;
            horizSnap.y = 0f;
            bool snappedClose = horizSnap.sqrMagnitude < maxSnapSqr;
            bool clearLineOfSight = !NavMesh.Raycast(transform.position, hit.position, out _, NavMesh.AllAreas);

            if (snappedClose && clearLineOfSight)
            {
                agent.SetDestination(hit.position);
                return;
            }

            // Otherwise stash as fallback so a cornered civilian still commits to a routed flee
            // rather than standing still next to the player.
            if (!hasFallback)
            {
                fallbackDest = hit.position;
                hasFallback = true;
            }
        }

        if (hasFallback) agent.SetDestination(fallbackDest);
    }

    private void EnsureOnNavMesh()
    {
        if (agent.isOnNavMesh) return;
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
    }

    private void ResolvePlayerRef()
    {
        if (playerRef != null) return;
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerRef = player.transform;
    }

    private static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        Vector3 d = a - b;
        d.y = 0f;
        return d.magnitude;
    }
}
