using UnityEngine;
using UnityEngine.AI;

public class FootballPlayer : MonoBehaviour
{
    public Transform ball;
    public Collider pitchCollider;
    private NavMeshAgent agent;
    private bool isActive = false;
    private Vector3 startPosition;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        startPosition = transform.position;
        agent.enabled = false; // Stay put initially
    }

    void Update()
    {
        if (!isActive || ball == null) return;

        // Only chase if the ball is inside the pitch
        if (pitchCollider.bounds.Contains(ball.position))
        {
            agent.SetDestination(ball.position);
        }
        else
        {
            agent.SetDestination(startPosition); // Go back to "position" if ball is out
        }
    }

    public void SetPitchActivity(bool status)
    {
        isActive = status;
        agent.enabled = status;
        
        if(!status) agent.ResetPath();
    }
}