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
    [HideInInspector] public Transform _playerTransformRef;

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

    private MovementState movementState = MovementState.RandomMovement;
    private NavMeshAgent agent;
    private Rigidbody rb;

    private float currentSpeed;

    private float idleEndTime;
    private float nextRunAwayRefreshTime;

    void Awake()
    {
        if (anim == null) anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        SetupNavMesh();
        EnsureOnNavMesh();
    }

    void Start()
    {
        ResolvePlayerRefs();
        EnterRandomMovement();
    }

    void Update()
    {
        if (_playerTransformRef == null)
        {
            ResolvePlayerRefs();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransformRef.position);

        // Run-away takes priority over wandering / idling.
        if (movementState != MovementState.RunAway && distanceToPlayer < runAway.startRunningRadius)
        {
            EnterRunAway();
        }
        else if (movementState == MovementState.RunAway && distanceToPlayer > runAway.stopRunningRadius)
        {
            EnterRandomMovement();
        }

        // Per-state tick.
        switch (movementState)
        {
            case MovementState.RandomMovement:
                bool arrived = agent != null && !agent.pathPending && agent.remainingDistance < 0.5f;
                if (arrived) EnterIdling();
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
        if (_playerTransformRef == null) return;

        // Ask the agent which way it wants to go — but DON'T let it move us.
        Vector3 moveDir = Vector3.zero;
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            Vector3 desiredVel = agent.desiredVelocity;
            desiredVel.y = 0f;
            if (desiredVel.sqrMagnitude > 0.0001f) moveDir = desiredVel.normalized;
        }

        // Face the direction we're travelling.
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            Quaternion finalRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, finalRotation, Time.fixedDeltaTime * rotationSpeed);
        }

        // Move via the Rigidbody.
        ApplyVelocity(moveDir * currentSpeed);

        // Keep the agent's internal position in sync with where the body actually is.
        if (agent != null && agent.enabled && agent.isOnNavMesh) agent.nextPosition = transform.position;
    }

    private void ApplyVelocity(Vector3 desired)
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.Lerp(
                rb.linearVelocity,
                new Vector3(desired.x, rb.linearVelocity.y, desired.z),
                smoothVelocity);
        }
        else
        {
            transform.position += desired * Time.fixedDeltaTime;
        }
    }

    private void UpdateAnim()
    {
        if (anim == null || string.IsNullOrEmpty(speedParam))
        {
            return;
        }

        float animSpeed = 0f;
        if (rb != null)
        {
            Vector3 v = rb.linearVelocity;
            animSpeed = new Vector2(v.x, v.z).magnitude;
        }
        anim.SetFloat(speedParam, animSpeed);
    }

    private void EnterRandomMovement()
    {
        movementState = MovementState.RandomMovement;
        currentSpeed = randomMovement.speed;
        if (agent != null) agent.speed = currentSpeed;
        PickNewRandomMovementPoint();
    }

    private void EnterIdling()
    {
        movementState = MovementState.Idling;
        currentSpeed = 0f;
        if (agent != null && agent.isOnNavMesh) agent.ResetPath();
        idleEndTime = Time.time + Random.Range(randomMovement.minIdleDuration, randomMovement.maxIdleDuration);
    }

    private void EnterRunAway()
    {
        movementState = MovementState.RunAway;
        currentSpeed = runAway.speed;
        if (agent != null) agent.speed = currentSpeed;
        PickNewRunAwayPoint();
    }

    private void PickNewRandomMovementPoint()
    {
        if (agent == null || !agent.isOnNavMesh) return;

        Vector3 potentialPick = transform.position + Random.insideUnitSphere * randomMovement.radius;
        if (NavMesh.SamplePosition(potentialPick, out NavMeshHit hit, randomMovement.radius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private void PickNewRunAwayPoint()
    {
        if (agent == null || !agent.isOnNavMesh) return;

        Vector3 awayDirection = (transform.position - _playerTransformRef.position).normalized;
        Vector3 potentialPick = transform.position + awayDirection * runAway.stopRunningRadius;
        if (NavMesh.SamplePosition(potentialPick, out NavMeshHit hit, runAway.stopRunningRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        nextRunAwayRefreshTime = Time.time + runAway.refreshInterval;
    }

    private void SetupNavMesh()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.Log("No NavMesh agent found for the CivilianAI");
            return;
        }

        // Critical: NavMeshAgent guides, Rigidbody moves.
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.angularSpeed = 0f;

        currentSpeed = randomMovement.speed;
        agent.speed = currentSpeed;

        var capsule = GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            agent.radius = capsule.radius * 1.5f;
            agent.height = capsule.height;
            agent.baseOffset = capsule.center.y - capsule.height * 0.5f;
        }
    }

    // Safety net: if the civilian spawned slightly off the NavMesh, snap onto it.
    private void EnsureOnNavMesh()
    {
        if (agent == null || agent.isOnNavMesh) return;
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
    }

    private void ResolvePlayerRefs()
    {
        if (_playerTransformRef != null) return;
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        _playerTransformRef = player.transform;
    }
}
