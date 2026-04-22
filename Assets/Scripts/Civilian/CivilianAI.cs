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
    public float refreshInterval;
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
        speed = 6f,
        refreshInterval = 0.2f
    };

    [Header("Movement")]
    [SerializeField, Range(0f, 1f)] private float smoothVelocity = 0.35f;
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Animation (optional)")]
    [SerializeField] private Animator anim;
    [SerializeField] private string speedParam = "Speed";

    private MovementState state = MovementState.RandomMovement;
    private NavMeshAgent agent;
    private Rigidbody rb;
    private Transform playerRef;
    private float currentSpeed;
    private float idleEndTime;
    private float nextRunAwayRefreshTime;

    void Awake()
    {
        if (anim == null) anim = GetComponentInChildren<Animator>();

        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.angularSpeed = 0f;

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
                    EnterIdling();
                break;

            case MovementState.Idling:
                if (Time.time >= idleEndTime) EnterRandomMovement();
                break;

            case MovementState.RunAway:
                if (Time.time >= nextRunAwayRefreshTime) PickNewRunAwayPoint();
                break;
        }

        UpdateAnim();
    }

    void FixedUpdate()
    {
        Vector3 moveDir = Vector3.zero;
        if (agent.isOnNavMesh)
        {
            Vector3 desiredVel = agent.desiredVelocity;
            desiredVel.y = 0f;
            if (desiredVel.sqrMagnitude > 0.0001f) moveDir = desiredVel.normalized;
        }

        if (moveDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * rotationSpeed);
        }

        ApplyVelocity(moveDir * currentSpeed);

        if (agent.isOnNavMesh) agent.nextPosition = transform.position;
    }

    private void ApplyVelocity(Vector3 desired)
    {
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
        if (anim == null || string.IsNullOrEmpty(speedParam)) return;
        Vector3 v = rb.linearVelocity;
        anim.SetFloat(speedParam, new Vector2(v.x, v.z).magnitude);
    }

    private void EnterRandomMovement()
    {
        state = MovementState.RandomMovement;
        currentSpeed = randomMovement.speed;
        agent.speed = currentSpeed;
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
        PickNewRunAwayPoint();
    }

    private void PickNewRandomPoint()
    {
        if (!agent.isOnNavMesh) return;

        Vector3 candidate = transform.position + Random.insideUnitSphere * randomMovement.radius;
        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, randomMovement.radius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private void PickNewRunAwayPoint()
    {
        nextRunAwayRefreshTime = Time.time + runAway.refreshInterval;
        if (!agent.isOnNavMesh || playerRef == null) return;

        Vector3 awayDir = transform.position - playerRef.position;
        awayDir.y = 0f;
        if (awayDir.sqrMagnitude < 0.0001f) return;
        awayDir.Normalize();

        // Try straight-away first, then widen the angle. Without this, a civilian picks a point behind a
        // wall, snaps onto the navmesh right up against the wall, and presses into it instead of routing
        // through a nearby door.
        float[] angleOffsets = { 0f, 45f, -45f, 90f, -90f, 135f, -135f };
        NavMeshPath path = new NavMeshPath();
        float currentDist = HorizontalDistance(transform.position, playerRef.position);

        foreach (float angle in angleOffsets)
        {
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * awayDir;
            Vector3 candidate = transform.position + dir * runAway.stopRunningRadius;

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 3f, NavMesh.AllAreas)) continue;
            if (!agent.CalculatePath(hit.position, path) || path.status != NavMeshPathStatus.PathComplete) continue;
            if (HorizontalDistance(hit.position, playerRef.position) <= currentDist) continue;

            agent.SetDestination(hit.position);
            return;
        }
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
