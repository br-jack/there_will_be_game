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

    [SerializeField] private RunAwaySettings runAwaySettings = new RunAwaySettings
    {
        startRunningRadius = 10f,
        stopRunningRadius = 18f,
        speed = 6f,
        targetDistance = 15f,
        refreshInterval = 0.2f
    };

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private movementState = MovementState.RandomMovement;

    // Timers
    private float nextMovementTimes;
    private float nextRunAwayRefreshTime;

    private enum State
    {
        RandomMovement,
        RunAway
    }

    private State state = State.RandomMovement;
    private NavMeshAgent agent;
    private Transform playerTransform;
    private float nextMovementTime;
    private float nextRunAwayRefreshTime;

    void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        SetupNavMesh();
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = randomMovement.speed;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }
    void Update()
    {
        if (_playerTransformRef == null)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransformRef.position)

        // Switching between states
        if (movementState == MovementState.RandomMovement && distanceToPlayer < runAway.startRunningRadius)
        {
            
        }
        else if (movementState == MovementState.RunAway && distanceToPlayer > runAway.stopRunningRadius)
        {
            
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
        Vector3 potential = transform.position + Random.insideUnitSphere * randomMovement.radius;
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
}