using System;
using System.Collections;
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
    [SerializeField] private float stopFromPlayerDistance = 1.5f; // Enemy stop distance must be less than player's attack range.
    [SerializeField] private float smoothVelocity = 0.35f;
    [SerializeField] private float rotationSpeed = 8f;

    private EnemyAttack attack = new EnemyAttack
    {
        damage = 10,
        range = 2.5f,
        cooldown = 2f,
        chargeTime = 0.25f
    };

    [Header("Knockback & Death")]
    [SerializeField] private float maxDeathTime = 4f;
    private const float KnockbackTime = 0.5f;
    private const float GroundCheckDistance = 0.4f;

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
    public void MarkShieldHit() => ShieldWasJustHit = true;
    public void Initialize(StandardEnemySpawner spawnerRef) => spawner = spawnerRef;

    // Timers
    private float knockbackTimer;
    private float timeOfNextAttack;
    private float deathTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        _shieldBreakAudioSource = GetComponent<AudioSource>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
        SetupNavMesh();
    }

    void Start() => ResolvePlayerRefs();

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
        if (agent == null)
        {
            Debug.Log("No NavMesh agent found for the StandardEnemyAI");
            return;
        }

        // Disable automatic movement - we drive the Rigidbody ourselves.
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.angularSpeed = 0f;
        agent.speed = speed;
        agent.stoppingDistance = attack.range * 0.7f;

        // Match the agent cylinder to the physical CapsuleCollider so path clearance matches body size.
        var capsule = GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            agent.radius = capsule.radius;
            agent.height = capsule.height;
            agent.baseOffset = capsule.center.y - capsule.height * 0.5f;
        }
    }

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
            return;
        }

        MeleeAttack();
        UpdateAnim();
    }

    void FixedUpdate()
    {
        ShieldWasJustHit = false;

        if (IsDying || IsKnockedBack) return;
        if (_playerTransformRef == null) return;

        // Flat direction to player
        Vector3 toPlayer = _playerTransformRef.position - transform.position;
        toPlayer.y = 0f;
        float distToPlayer = toPlayer.magnitude;
        Vector3 toPlayerDir = distToPlayer > 0.01f ? toPlayer / distToPlayer : Vector3.zero;

        // Use NavMesh for pathfinding if available, otherwise straight-line fallback.
        Vector3 moveDir = toPlayerDir;
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.SetDestination(_playerTransformRef.position);
            Vector3 desiredVel = agent.desiredVelocity;
            desiredVel.y = 0f;
            if (desiredVel.sqrMagnitude > 0.0001f) moveDir = desiredVel.normalized;
        }

        // Rotate towards movement direction
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            Quaternion finalRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, finalRotation, Time.fixedDeltaTime * rotationSpeed);
        }

        // Slow down as we enter attack range
        float currentSpeed = speed;
        float stopDist = attack.range * 0.7f;
        float arriveDist = attack.range + stopFromPlayerDistance;
        if (distToPlayer < stopDist)
        {
            currentSpeed = 0f;
        }
        else if (distToPlayer < arriveDist)
        {
            currentSpeed *= (distToPlayer - stopDist) / (arriveDist - stopDist);
        }

        // Apply movement
        Vector3 desired = moveDir * currentSpeed;
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

        // Keep NavMesh agent position in sync with Rigidbody
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
        if (_playerHealthRef.IsDead) return;
        if (Time.time < timeOfNextAttack) return;

        Vector3 toPlayer = _playerTransformRef.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude > attack.range * attack.range) return;

        timeOfNextAttack = Time.time + attack.cooldown;
        TryTrigger(attackTrigger);

        if (!useDamageAnimEvent) StartCoroutine(ChargeUpThenDamage());
    }

    private IEnumerator ChargeUpThenDamage()
    {
        if (attack.chargeTime > 0f) yield return new WaitForSeconds(attack.chargeTime);
        DoDamage();
    }

    private void DoDamage()
    {
        if (IsDying || _playerHealthRef == null) return;

        // Only damage if player is still in range at the moment of impact.
        Vector3 toPlayer = _playerTransformRef.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude <= attack.range * attack.range)
        {
            _playerHealthRef.TakeDamage(attack.damage);
        }
    }

    /// <summary>Called from an attack animation event at the hit frame.</summary>
    public void AnimDealDamage()
    {
        if (useDamageAnimEvent) DoDamage();
    }

    public void ApplyKnockback(Vector3 force)
    {
        IsKnockedBack = true;
        knockbackTimer = KnockbackTime;

        if (agent != null) agent.enabled = false;   // let physics own the body

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(force, ForceMode.Impulse);
        }

        TryTrigger(hitTrigger);
    }

    private void HandleKnockback()
    {
        knockbackTimer -= Time.deltaTime;
        if (knockbackTimer > 0f) return;
        if (!IsGrounded()) return;

        // Knockback finished - back on the ground.
        IsKnockedBack = false;
        knockbackTimer = KnockbackTime;

        if (agent != null)
        {
            agent.enabled = true;
            if (!agent.isOnNavMesh && NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
        }
    }

    public void KilledBy(Collider other, AttackHitbox hitBox)
    {
        if (IsDying) return;

        IsDying = true;
        IsKnockedBack = true;
        knockbackTimer = KnockbackTime;
        deathTimer = maxDeathTime;

        if (agent != null) agent.enabled = false;

        // Grey out the enemy.
        Renderer r = GetComponent<Renderer>() ?? GetComponentInChildren<Renderer>();
        if (r != null) r.material.color = Color.gray;

        // Knock away from whatever killed it.
        float force = hitBox != null ? hitBox.GetKnockbackForce() : 30f;
        Vector3 knockDir = transform.position - other.transform.position;
        knockDir.y = Mathf.Clamp(force / 75f, 0.2f, 1.5f);
        knockDir.Normalize();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(knockDir * force, ForceMode.Impulse);
        }

        TryTrigger(deadTrigger);

        spawner?.RemoveEnemy(this);
        OnDied?.Invoke();
    }

    private void KillEnemy()
    {
        deathTimer -= Time.deltaTime;

        if (IsKnockedBack)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f && IsGrounded()) IsKnockedBack = false;
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
            animSpeed = new Vector2(v.x, v.z).magnitude;
        }
        anim.SetFloat(speedParam, animSpeed);
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, GroundCheckDistance + 0.2f);
    }

    private void TryTrigger(string triggerName)
    {
        if (anim != null && !string.IsNullOrEmpty(triggerName)) anim.SetTrigger(triggerName);
    }

    private void OnDisable()
    {
        if (!IsDying) spawner?.RemoveEnemy(this);
    }
}
