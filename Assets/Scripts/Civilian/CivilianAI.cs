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
    private const float StuckTimeThreshold = 0.9f;
    private const float StuckMovedSqr = 0.04f;
    private const float StuckCommandedSqr = 1f;
    private const float UnstuckDuration = 0.3f;
    private Vector3 _stuckCheckPos;
    private float _stuckCheckTime;
    private float _unstuckUntil;
    private Vector3 _unstuckDir;
    private const float FacingRetargetAngle = 10f;
    private Vector3 _stableFacingDir;
    private const float StallTime = 1.5f;
    private const float StallProgressDist = 0.5f;
    private Vector3 _progressPos;
    private float _progressTime;
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
        if (DeathHandler.IsDying) return;
        
        if (playerRef == null) { ResolvePlayerRef(); return; }

        float distToPlayer = HorizontalDistance(transform.position, playerRef.position);

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
        if (DeathHandler.IsDying) return;

        Vector3 moveDir = Vector3.zero;
        Vector3 facingDir = Vector3.zero;
        if (agent.isOnNavMesh)
        {

            Vector3 desiredVel = agent.desiredVelocity;
            desiredVel.y = 0f;
            if (desiredVel.sqrMagnitude > 0.0001f) moveDir = desiredVel.normalized;

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
        if (Time.time < _unstuckUntil) return;

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

        if (Time.time < _unstuckUntil)
        {
            desired = _unstuckDir * currentSpeed;
        }

        if (desired.sqrMagnitude < 0.0001f)
        {
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

            Vector3 horizSnap = candidate - hit.position;
            horizSnap.y = 0f;
            bool snappedClose = horizSnap.sqrMagnitude < maxSnapSqr;
            bool clearLineOfSight = !NavMesh.Raycast(transform.position, hit.position, out _, NavMesh.AllAreas);

            if (snappedClose && clearLineOfSight)
            {
                agent.SetDestination(hit.position);
                return;
            }

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
