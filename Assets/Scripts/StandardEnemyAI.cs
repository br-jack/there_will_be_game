using UnityEngine;
using UnityEngine.AI;

public struct EnemyAttack
{
    public float damage;
    public float range;
    public float cooldown;
    public float chargeTime;
}
public class StandardEnemyAI : MonoBehaviour
{
    // References
    public GameObject shield;
    private Rigidbody rb;
    private AudioSource _shieldBreakAudioSource;
    private StandardEnemySpawner spawner;
    [HideInInspector] public PlayerHealth _playerHealthRef;
    [HideInInspector] public Transform _playerTransformRef;
    private NavMeshAgent agent;

    [Header("Movement")]
    [SerializeField] private float speed = 5f;

    private EnemyAttack attack = new EnemyAttack
    {
        damage = 10f,
        range = 2.5f,
        cooldown = 2f,
        chargeTime = 0.25f
    };

    // The distance the Enemy stops at the player should be less than the player's attack range.
    [SerializeField] private float stopFromPlayerDistance = 1.5f;
    [SerializeField] private float smoothVelocity = 0.35f;
    [SerializeField] private float rotationSpeed = 8f;

    public bool IsKnockedBack { get; private set; }
    public bool IsDying { get; private set; }
    public bool ShieldWasJustHit { get; private set; }
    public event Action OnDied;

    // Timers
    private float knockbackTimer;
    private float knockbackTime;
    private float attackTimer;
    private float deathTime = 0.5f;
    private float deathTimer;

    // Thresholds
    private float groundDistanceThreshold;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        _shieldBreakAudioSource = GetComponent<AudioSource>();
    }
    void Start()
    {
        
    }

    private void NavMeshAgentSetup()
    {
        agent = GetComponent<NavMeshAgent>();
        var capsule = GetComponent<CapsuleCollider>();

        if (agent == null)
        {
            Debug.Log("No NavMesh agent found for the StandardEnemyAI");
        }

        // Disable the automatic movement
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.angularSpeed   = 0f;
        agent.speed = speed;
        agent.stoppingDistance = attackRange * 0.7f; // Enemy stops within attacking range of player.

        // Make the NavMesh agent the same size as the Enemy capsule collider
        agent.radius = capsule.radius;
        agent.height = capsule.height;
        agent.baseOffset = capsule.center.y - capsule.height * 0.5f;

    }

    // Update is called once per frame
    void Update()
    {
        if (IsDying)
        {
            KillEnemy();
            return;
        }

        if (IsKnockedBack)
        {
            HandleKnockback();
            return;
        }
    }

    void FixedUpdate()
    {
        ShieldWasJustHit = false;

        if (IsDying) return;
        if (IsKnockedBack) return;
        if (_playerTransformRef == null) return;

        // MOVEMENT
        Vector3 toPlayer = _playerTransformRef.position - transform.position;
        toPlayer.y = 0f;

        // toPlayerDir is nonsense if the vector is too small.
        Vector3 toPlayerDir;
        if (toPlayer.magnitude > 0.01f)
        {
            toPlayerDir = toPlayer / toPlayer.magnitude;
        }
        else
        {
            toPlayerDir = Vector3.zero;
        }

        bool IsNavMeshAvail = agent != null && agent.enabled && agent.isOnNavMesh;
        Vector3 moveDir;

        // Use NavMesh for pathfinding if available, otherise use the straight line fallback (terrible).
        if (IsNavMeshAvail)
        {
            agent.SetDestination(_playerTransformRef.position);
            moveDir = agent.desiredVelocity;
            moveDir.y = 0
            if (moveDir.magnitude > 0.01f)
            {
                moveDir = moveDir.normalized;
            }
            else // Fallback if the vector is too short, then it's nonsence.
            {
                moveDir = toPlayerDir;
            }
        }
        // Fallback if there's no NavMesh
        else
        {
            moveDir = toPlayerDir;
        }
        
        moveDir = new Vector3(moveDir.x, 0f, moveDir.z);

        if (moveDir.magnitude > 0.01f)
        {
            Quaternion finalRotation = Quaternion.LookRotation(moveDir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, finalRotation, Time.fixedDeltaTime * rotationSpeed);
        }

        Vector3 desired = moveDir * speed;

        if (rb != null)
        {
            Vector3 cur = rb.linearVelocity;

            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, new Vector3(desired.x, rb.linearVelocity.y, desired.z), smoothVelocity);
        }
        else
        {
            transform.position += desired * Time.fixedDeltaTime;
        }
    }

    public void BreakShield()
    {
        if (shield == null) return;
        Destroy(shield);
        shield = null;
        _shieldBreakAudioSource?.Play();
    }
    
    private void MeleeAttack()
    {
        if (_playerHealthRef == null) return;
        if (_playerTransformRef == null) return;
        if (_playerHealthRef.IsDead) return;
    }


    private void HandleKnockback()
    {
        knockbackTimer -= Time.deltaTime;

        if (knockbackTimer > 0f) return;

        bool grounded = IsGroundedHelper();
        
        if (!grounded) return;

        // The player is back on the ground (aka knockback has FINISHED).
        IsKnockedBack = false;
        knockbackTimer = knockbackTime;

        // The agent is disabled during knockback. Enable it back now that knockback is finished.
        if (agent != null)
        {
            agent.enabled = true;

            if (!agent.isOnNavMesh)
            {
                if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 3f, NavMesh.AllAreas)) agent.Warp(hit.position);
            }
        }
    }

    public void KilledBy(Collider other, AttackHitBox hitBox)
    {
        IsDying = true;
        IsKnockedBack = true;
        knockbackTimer = knockbackTime;
        deathTimer = deathTime;

        // Remove Navmesh control.
        if (agent != null) agent.enabled = false;

        // Grey out the enemy.
        Renderer r = GetComponent<Renderer>() ?? GetComponentInChildren<Renderer>();
        if (r != null) r.material.color = Color.gray;

        float force = 30f;
        if (attack) force = attack.GetKnockBackForce();
        Vector3 knockDirection = (transform.position - other.transform.position).normalized;
        float upward = Mathf.Clamp(force/75f, 0.2f, 1.5f);
        knockDirection.Normalize();

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(knockDirection * force, ForceMode.Impulse);

        OnDied?.Invoke();
    }

    private void KillEnemy()
    {
        deathTimer -= Time.deltaTime;

        if (IsKnockedBack)
        {
            knockbackTimer -= Time.deltaTime;

            if (knockbackTimer <= 0f)
            {
                bool grounded = IsGroundedHelper();
                if (grounded) IsKnockedBack = false;
            }
        }

        if (!IsKnockedBack || deathTimer <= 0) Destroy(gameObject);
    }

    private bool IsGroundedHelper()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.15f, Vector3.down, groundDistanceThreshold + 0.15f);
    }
}
