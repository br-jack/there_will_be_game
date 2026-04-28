using Enemy;
using UnityEngine;

public class FootballPlayer : MonoBehaviour
{
    public Transform ball;
    public Collider pitchCollider;
    public Transform targetGoal;

    private StandardEnemyAI enemyAI;
    private Rigidbody rb;
    private Vector3 homeAnchor;
    private bool isFootballActive = false;

    [Header("Tether Settings")]
    public float detectionRadius = 3f;
    public float maxLeashDistance = 4f;
    public float kickPower = 6f;
    public float kickCooldown = 2f;
    public float moveSpeed = 11f;
    private float lastKickTime;
    [HideInInspector] public Transform _playerTransformRef;

    void Start()
    {
        enemyAI = GetComponent<StandardEnemyAI>();
        rb = GetComponent<Rigidbody>();
        homeAnchor = transform.position;
    }

    public void SwitchToNormalAI()
    {
        Debug.Log("footballPlayer switched to enemy");
        isFootballActive = false;

        if (enemyAI != null) enemyAI.enabled = true;

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

            MoveToward(approachPosition);
        }
        else
        {
            MoveToward(homeAnchor);
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

        MoveToward(homeAnchor);
    }

    public void SetPitchActivity(bool status)
    {
        isFootballActive = status;
        if (status && enemyAI != null) enemyAI.enabled = false;
    }

    private void MoveToward(Vector3 target)
    {
        Vector3 dir = target - transform.position;
        dir.y = 0f;
        float dist = dir.magnitude;

        float speed = moveSpeed;
        if (dist < 0.18f) speed = 0f;
        else if (dist < 0.6f) speed *= dist / 0.6f;

        Vector3 desired = dir.normalized * speed;
        rb.linearVelocity = Vector3.Lerp(
            rb.linearVelocity,
            new Vector3(desired.x, rb.linearVelocity.y, desired.z),
            0.25f
        );

        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion rot = Quaternion.LookRotation(new Vector3(dir.x, 0f, dir.z).normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.fixedDeltaTime * 5f);
        }
    }
}
