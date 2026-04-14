using UnityEngine;
using UnityEngine.AI;

public class FootballPlayer : MonoBehaviour
{
    public Transform ball;
    public Collider pitchCollider;
    public Transform targetGoal;
    
    private StandardEnemyAI enemyAI;
    private NavMeshAgent agent;
    private Vector3 homeAnchor;
    private bool isFootballActive = false;

    [Header("Tether Settings")]
    public float detectionRadius = 3f;
    public float maxLeashDistance = 4f;
    public float kickPower = 6f;
    public float kickCooldown = 2f;
    private float lastKickTime;
    [HideInInspector] public Transform _playerTransformRef;

    void Start()
    {
        movementScript = GetComponent<EnemyMovement>();
        homeAnchor = transform.position;
    }

    public void SwitchToNormalAI()
    {
        Debug.Log("footballPlayer switched to enemy");
        isFootballActive = false;
        
        if (movementScript != null)
        {
            movementScript.ClearFormationTarget();
            movementScript.SetAttackTarget(_playerTransformRef != null ? _playerTransformRef.position : transform.position);
        }

        // Disable this specific football script so it stops overriding targets
        this.enabled = false; 
    }

    void FixedUpdate()
    {
        if (!isFootballActive || ball == null) return;

        float distFromHomeToBall = Vector3.Distance(homeAnchor, ball.position);
        float distFromSelfToHome = Vector3.Distance(transform.position, homeAnchor);

        if (distFromHomeToBall <= detectionRadius && 
            distFromSelfToHome < maxLeashDistance && 
            Time.time > lastKickTime + kickCooldown)
        {
            Vector3 directionToGoal = (targetGoal.position - ball.position).normalized;
            Vector3 approachPosition = ball.position - (directionToGoal * 0.4f);
            
            movementScript.SetAttackTarget(approachPosition);
        }
        else
        {
            movementScript.SetAttackTarget(homeAnchor);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Football") && isFootballActive)
        {
            if (Time.time > lastKickTime + kickCooldown)
            {
                ExecuteKick(collision.gameObject.GetComponent<Rigidbody>());
            }
        }
    }

    void ExecuteKick(Rigidbody ballRb)
    {
        if (ballRb == null || targetGoal == null) return;

        lastKickTime = Time.time;

        Vector3 shootDirection = (targetGoal.position - ball.position).normalized;
        shootDirection.y = 0.1f; 

        ballRb.linearVelocity = Vector3.zero; 
        ballRb.AddForce(shootDirection * kickPower, ForceMode.Impulse);

        movementScript.SetAttackTarget(homeAnchor);
    }

    public void SetPitchActivity(bool status)
    {
        isFootballActive = status;
    }
}