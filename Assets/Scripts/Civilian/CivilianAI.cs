using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]

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

public class CivilianAI : MonoBehaviour
{
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

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = randomMovement.speed;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;

    }
    void Update()
    {
        
    }
}