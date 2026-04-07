using UnityEngine;
using UnityEngine.AI;

public class FootballPlayer : MonoBehaviour
{
    public Transform ball;
    public Collider pitchCollider;
    
    [HideInInspector] public bool isChasing = false;
    [HideInInspector] public Transform targetGoal;
    
    private EnemyMovement movementScript;
    private Vector3 initialHomePosition;
    private bool isFootballActive = false;
    
    public float kickPower = 8f;
    public float kickCooldown = 1.5f;
    private float lastKickTime;

    void Start()
    {
        movementScript = GetComponent<EnemyMovement>();
        initialHomePosition = transform.position;
    }

    void FixedUpdate()
    {
        if (!isFootballActive || ball == null) return;

        if (isChasing && Time.time > lastKickTime + kickCooldown)
        {
            if (pitchCollider.bounds.Contains(ball.position))
            {
                Vector3 directionToGoal = (targetGoal.position - ball.position).normalized;
                Vector3 approachPosition = ball.position - (directionToGoal * 0.5f);
                
                movementScript.SetAttackTarget(approachPosition);
            }
        }
        else
        {
            movementScript.SetAttackTarget(initialHomePosition);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Football") && isChasing)
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
        shootDirection.y = 0.15f; // Add "lift" so it doesn't just friction-stop on the grass

        ballRb.linearVelocity = Vector3.zero;
        ballRb.AddForce(shootDirection * kickPower, ForceMode.Impulse);

        movementScript.SetAttackTarget(initialHomePosition);
        
        Debug.Log(gameObject.name + " cleared out after kicking!");
    }

    public void SetPitchActivity(bool status)
    {
        isFootballActive = status;
    }
}