using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public struct EnemyAttack
{
    public int damage;
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
        damage = 10,
        range = 2.5f,
        cooldown = 2f,
        chargeTime = 0.25f
    };

    [SerializeField] private float stopFromPlayerDistance = 1.5f; // The distance the Enemy stops at the player should be less than the player's attack range.
    [SerializeField] private float smoothVelocity = 0.35f;
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Knockback & Death")]
    [SerializeField] private float maxDeathTime = 4f;
    private float groundCheckDistance = 0.4f;

    [Header("Animation (optional)")]
    [SerializeField] private Animator anim;
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string attackTrigger = "Attack";
    [SerializeField] private string hitTrigger = "Hit";
    [SerializeField] private string deadTrigger = "Die";
    [SerializeField] private bool useDamageAnimEvent = false;

    public bool IsKnockedBack { get; private set; }
    public bool IsDying { get; private set; }
    public bool ShieldWasJustHit { get; private set; }
    public event Action OnDied;

    public bool HasShield() => shield != null;

    public void MarkShieldHit()
    {
        ShieldWasJustHit = true;
    }

    // Timers
    private float knockbackTimer;
    private float knockbackTime = 0.5f;
    private float timeOfNextAttack;
    private float deathTimer;

    // State
    private bool hasWarnedMissingPlayer;

    
    public void Initialize(StandardEnemySpawner spawnerRef)
    {
        spawner = spawnerRef;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        _shieldBreakAudioSource = GetComponent<AudioSource>();

        SetupNavMesh();

        if (anim == null) anim = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        ResolvePlayerRefs();
    }

    private void ResolvePlayerRefs()
    {
        if (_playerHealthRef != null && _playerTransformRef != null) return;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        if (_playerTransformRef == null) _playerTransformRef = player.transform;
        if (_playerHealthRef == null) _playerHealthRef = player.GetComponent<PlayerHealth>();
    }

    private void SetupNavMesh()
    {
        agent = GetComponent<NavMeshAgent>();
        var capsule = GetComponent<CapsuleCollider>();

        if (agent == null)
        {
            Debug.Log("No NavMesh agent found for the StandardEnemyAI");
            return;
        }

        // Disable the automatic movement
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.angularSpeed = 0f;
        agent.speed = speed;
        agent.stoppingDistance = attack.range * 0.7f; // Enemy stops within attacking range of player.

        // Auto-size the agent cylinder from the physical CapsuleCollider so that
        // the path clearance matches the enemy's actual body size.
        if (capsule != null)
        {
            // Make the NavMesh agent the same size as the Enemy capsule collider
            agent.radius = capsule.radius;
            agent.height = capsule.height;
            agent.baseOffset = capsule.center.y - capsule.height * 0.5f;
        }
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

        if (_playerHealthRef == null || _playerTransformRef == null)
        {
            ResolvePlayerRefs();
            if (!hasWarnedMissingPlayer && Time.time > 2f)
            {
                hasWarnedMissingPlayer = true;
            }
            return;
        }

        MeleeAttack();
        UpdateAnim();
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
        float distToPlayer = toPlayer.magnitude;
        Vector3 toPlayerDir;
        if (distToPlayer > 0.01f)
        {
            toPlayerDir = toPlayer / distToPlayer;
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
            moveDir.y = 0f;
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

        // Arrive / stop speed ramp — slow down as we enter attack range
        float currentSpeed   = speed;
        float stopDist       = attack.range * 0.7f;
        float arriveDist     = attack.range + stopFromPlayerDistance;

        if (distToPlayer < stopDist)
        {
            currentSpeed = 0f;
        }
        else if (distToPlayer < arriveDist)
        {
            float t = (distToPlayer - stopDist) / (arriveDist - stopDist);
            currentSpeed *= t;
        }

        Vector3 desired = moveDir * currentSpeed;

        if (rb != null)
        {
            Vector3 cur = rb.linearVelocity;

            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, new Vector3(desired.x, rb.linearVelocity.y, desired.z), smoothVelocity);
        }
        else
        {
            transform.position += desired * Time.fixedDeltaTime;
        }

        if (agent != null && agent.enabled && agent.isOnNavMesh) agent.nextPosition = transform.position;
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
        if (Time.time < timeOfNextAttack) return;

        Vector3 toPlayer = _playerTransformRef.position - transform.position;
        toPlayer.y = 0f;

        if (toPlayer.sqrMagnitude > attack.range * attack.range) return;

        timeOfNextAttack = Time.time + attack.cooldown;

        if (anim != null && !string.IsNullOrEmpty(attackTrigger)) anim.SetTrigger(attackTrigger);

        if (!useDamageAnimEvent) StartCoroutine(ChargeUpThenDamage());
    }

    private IEnumerator ChargeUpThenDamage()
    {
        if (attack.chargeTime > 0f) yield return new WaitForSeconds(attack.chargeTime);
        DoDamage();
    }

    private void DoDamage()
    {
        if (_playerHealthRef == null) return;
        if (IsDying) return;

        Vector3 toPlayer = _playerTransformRef.position - transform.position;
        toPlayer.y = 0;

        // Damage is only done if the player is within range after the attack animation finishes.
        if (toPlayer.sqrMagnitude <= attack.range * attack.range)
        {
            _playerHealthRef.TakeDamage(attack.damage);
        }
    }

    /// <summary>Called from an attack animation event at the hit frame.</summary>
    public void AnimDealDamage()
    {
        if (!useDamageAnimEvent) return;
        DoDamage();
    }

    public void ApplyKnockback(Vector3 force)
    {
        IsKnockedBack  = true;
        knockbackTimer = knockbackTime;

        if (agent != null) agent.enabled = false;   // let physics own the body

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(force, ForceMode.Impulse);
        }

        if (anim != null && !string.IsNullOrEmpty(hitTrigger))
        {
            anim.SetTrigger(hitTrigger);
        }
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

    public void KilledBy(Collider other, AttackHitbox hitBox)
    {
        if (IsDying) return;

        IsDying = true;
        IsKnockedBack = true;
        knockbackTimer = knockbackTime;
        deathTimer = maxDeathTime;

        // Remove Navmesh control.
        if (agent != null) agent.enabled = false;

        // Grey out the enemy.
        Renderer r = GetComponent<Renderer>() ?? GetComponentInChildren<Renderer>();
        if (r != null) r.material.color = Color.gray;

        float force = hitBox != null ? hitBox.GetKnockbackForce() : 30f;
        Vector3 knockDirection = transform.position - other.transform.position;
        knockDirection.y = Mathf.Clamp(force / 75f, 0.2f, 1.5f);
        knockDirection.Normalize();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(knockDirection * force, ForceMode.Impulse);
        }

        if (anim != null && !string.IsNullOrEmpty(deadTrigger))
        {
            anim.SetTrigger(deadTrigger);
        }

        spawner?.RemoveEnemy(this);
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

        if (!IsKnockedBack || deathTimer <= 0f) Destroy(gameObject);
    }

    private void UpdateAnim()
    {
        if (anim == null || string.IsNullOrEmpty(speedParam)) return;

        float animSpeed = 0f;
        if (rb != null)
        {
            Vector3 v = rb.linearVelocity;
            animSpeed = new Vector3(v.x, 0f, v.z).magnitude;
        }
        anim.SetFloat(speedParam, animSpeed);
    }

    private bool IsGroundedHelper()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, groundCheckDistance + 0.2f);
    }

    private void OnDisable()
    {
        if (!IsDying) spawner?.RemoveEnemy(this);
    }
}
