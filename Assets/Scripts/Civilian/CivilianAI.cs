using UnityEngine;
using UnityEngine.AI;

[System.Serializable] public struct RandomMovementSettings
{
    public float radius;
    public float minInterval;
    public float maxInterval;
    public float speed;
}

[System.Serializable] public struct RunAwaySettings
{
    public float startRunningRadius;
    public float stopRunningRadius;
    public float speed;
    public float targetDistance;
    public float refreshInterval;
}

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class CivilianAI : MonoBehaviour
{
    private enum MovementState { RandomMovement, RunAway }
    [HideInInspector] public Transform _playerTransformRef;
    [SerializeField] private RandomMovementSettings randomMovement = new RandomMovementSettings
    {
        radius = 8f,
        minInterval = 2f,
        maxInterval = 5f,
        speed = 2f
    };

    [SerializeField] private RunAwaySettings runAway = new RunAwaySettings
    {
        startRunningRadius = 10f,
        stopRunningRadius = 18f,
        speed = 6f,
        targetDistance = 15f,
        refreshInterval = 0.2f
    };

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string speedParam = "Speed";

    private MovementState movementState = MovementState.RandomMovement;
    private NavMeshAgent agent;

    // Timers
    private float nextMovementTime;
    private float nextRunAwayRefreshTime;

    void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        SetupNavMesh();
    }

    void Start()
    {
        ResolvePlayerRefs();
    }
    void Update()
    {
        if (_playerTransformRef == null)
        {
            ResolvePlayerRefs();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransformRef.position);

        // Switching between states
        if (movementState == MovementState.RandomMovement && distanceToPlayer < runAway.startRunningRadius)
        {
            movementState = MovementState.RunAway;
            agent.speed = runAway.speed;
            PickNewRunAwayPoint();
        }
        else if (movementState == MovementState.RunAway && distanceToPlayer > runAway.stopRunningRadius)
        {
            movementState = MovementState.RandomMovement;
            agent.speed = randomMovement.speed;
            PickNewRandomMovementPoint();
        }

        if (movementState == MovementState.RandomMovement)
        {
            
        }
        else
        {
            
        }
    }

    private void PickNewRandomMovementPoint()
    {
        Vector3 potentialPick = transform.position + Random.insideUnitSphere * randomMovement.radius;
        if (NavMesh.SamplePosition(potentialPick, out NavMeshHit hit, randomMovement.radius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        nextMovementTime = Time.time + Random.Range(randomMovement.minInterval, randomMovement.maxInterval);
    }

    private void PickNewRunAwayPoint()
    {
        Vector3 awayDirection = (transform.position - _playerTransformRef.position).normalized;
        Vector3 potentialPick = transform.position + awayDirection * runAway.targetDistance;
        if (NavMesh.SamplePosition(potentialPick, out NavMeshHit hit, runAway.targetDistance, NavMesh.AllAreas))
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
        agent.speed = randomMovement.speed;
    }

    private void ResolvePlayerRefs()
    {
        if (_playerTransformRef != null) return;
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        _playerTransformRef = player.transform;
    }
}