using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable] public struct EnemyAttack
{
    public int damage;
    public float range;
    public float cooldown;
    public float chargeTime;
}

public class StandardEnemyAI : MonoBehaviour
{
    private enum CombatState { Approaching, Holding, Striking, Attacking, Retreating }

    // References
    [HideInInspector] public GameObject shield;
    private Rigidbody rb;
    private AudioSource _shieldBreakAudioSource;
    [HideInInspector] public PlayerHealth _playerHealthRef;
    [HideInInspector] public Transform _playerTransformRef;
    private NavMeshAgent agent;

    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField, Range(0f, 0.3f)] private float speedVariance = 0.1f;
    [SerializeField] private float smoothVelocity = 0.35f;
    [SerializeField] private float rotationSpeed = 8f;

    [SerializeField] protected EnemyAttack attack = new EnemyAttack
    {
        damage = 10,
        range = 2.5f,
        cooldown = 2f,
        chargeTime = 0.25f
    };

    [Header("Strike Behavior")]
    [SerializeField] protected bool useStrike = true;
    [SerializeField] private float holdDistance = 6f;
    [SerializeField] private float holdDistanceVariance = 1.5f;
    [SerializeField] private float strikeSpeedMultiplier = 2.5f;
    [SerializeField] private float retreatSpeedMultiplier = 1.5f;

    [Header("Ranged Behavior (used when !useStrike)")]
    [SerializeField] private float stopFromPlayerDistance = 1.5f;

    [Header("Knockback & Death")]
    [SerializeField] private float maxDeathTime = 4f;
    private const float KnockbackTime = 0.5f;
    private const float GroundCheckDistance = 0.4f;

    private CombatState combatState = CombatState.Approaching;
    private float actualHoldDistance;
    private float actualSpeed;

    [Header("Animation (optional)")]
    [SerializeField] private Animator anim;
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string idleBoolParam = "IsIdle";
    [SerializeField] private float idleSpeedThreshold = 0.1f;
    [SerializeField] private string attackTrigger = "Attack";
    [SerializeField] private string shieldBreakTrigger = "ShieldBreak";
    [SerializeField] private string hitTrigger = "Hit";
    [SerializeField] private string deadTrigger = "Die";
    [SerializeField] private bool useDamageAnimEvent = false;

    public bool IsKnockedBack { get; private set; }
    public bool IsDying { get; private set; }
    public bool ShieldWasJustHit { get; private set; }
    public event Action OnDied;

    public bool HasShield() => shield != null;
    public void MarkShieldHit() => ShieldWasJustHit = true;

    // Timers
    private float knockbackTimer;
    private float timeOfNextAttack;
    private float deathTimer;

    // Debug throttling
    private float _dbgTimer;
    private bool _doDebug;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        _shieldBreakAudioSource = GetComponent<AudioSource>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
        ShieldHit shieldHit = GetComponentInChildren<ShieldHit>();
        if (shieldHit != null) shield = shieldHit.gameObject;

        // Each enemy gets a slightly different hold distance (Stalk distance should ALWAYS be further than attack range).
        actualHoldDistance = holdDistance + UnityEngine.Random.Range(-holdDistanceVariance, holdDistanceVariance);
        actualHoldDistance = Mathf.Max(actualHoldDistance, attack.range + 0.5f);

        actualSpeed = speed * (1f + UnityEngine.Random.Range(-speedVariance, speedVariance));

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

        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.angularSpeed = 0f;
        agent.speed = actualSpeed;
        agent.stoppingDistance = attack.range * 0.7f;

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
        _dbgTimer -= Time.deltaTime;
        _doDebug = _dbgTimer <= 0f;
        if (_doDebug) _dbgTimer = 1f;

        if (IsDying) { KillEnemy(); return; }
        if (IsKnockedBack) { HandleKnockback(); return; }

        if (_playerHealthRef == null || _playerTransformRef == null)
        {
            ResolvePlayerRefs();
            return;
        }

        if (useStrike)
        {
            StrikeUpdate();
        }
        else
        {
            ClassicAttackUpdate();
        }

        UpdateAnim();
    }

    // Stalk, Strike and Retreat state system
    private void StrikeUpdate()
    {
        if (_playerHealthRef.IsDead) return;

        switch (combatState)
        {
            case CombatState.Holding:
                // Wait for cooldown, then start striking.
                if (Time.time >= timeOfNextAttack)
                {
                    combatState = CombatState.Striking;
                }
                break;

            case CombatState.Striking:
                // Check if in attack range.
                Vector3 toPlayer = _playerTransformRef.position - transform.position;
                toPlayer.y = 0f;
                if (toPlayer.sqrMagnitude <= attack.range * attack.range)
                {
                    combatState = CombatState.Attacking;
                    timeOfNextAttack = Time.time + attack.cooldown;
                    TryTrigger(attackTrigger);
                    StartCoroutine(StrikeDamageThenRetreat());
                }
                break;

            // StrikeMovement() in FixedUpdate() does Approaching and Retreating
            // If there's no approach and retreat (e.g. for ranged enemies) then ClassicMovement().
        }
    }

    // The classic system (system without the lunging forwards and retreating). Ranged enemies use this.
    private void ClassicAttackUpdate()
    {
        if (_playerHealthRef.IsDead) return;
        if (Time.time < timeOfNextAttack) return;

        Vector3 toPlayer = _playerTransformRef.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude > attack.range * attack.range) return;

        PerformAttack();
    }

    private IEnumerator StrikeDamageThenRetreat()
    {
        if (attack.chargeTime > 0f) yield return new WaitForSeconds(attack.chargeTime);
        DoDamage();
        combatState = CombatState.Retreating;
    }

    private void PerformAttack()
    {
        timeOfNextAttack = Time.time + attack.cooldown;
        TryTrigger(attackTrigger);
        if (!useDamageAnimEvent) StartCoroutine(ChargeUpThenDamage());
    }

    void FixedUpdate()
    {
        ShieldWasJustHit = false;

        if (IsDying || IsKnockedBack) return;
        if (_playerTransformRef == null) return;

        Vector3 toPlayer = _playerTransformRef.position - transform.position;
        toPlayer.y = 0f;
        float distToPlayer = toPlayer.magnitude;
        Vector3 toPlayerDir = distToPlayer > 0.01f ? toPlayer / distToPlayer : Vector3.zero;

        // NavMesh pathfinding direction.
        Vector3 moveDir = toPlayerDir;
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.SetDestination(_playerTransformRef.position);
            Vector3 desiredVel = agent.desiredVelocity;
            desiredVel.y = 0f;
            if (desiredVel.sqrMagnitude > 0.0001f) moveDir = desiredVel.normalized;
        }

        // Always face the player.
        if (toPlayerDir.sqrMagnitude > 0.0001f)
        {
            Quaternion finalRotation = Quaternion.LookRotation(toPlayerDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, finalRotation, Time.fixedDeltaTime * rotationSpeed);
        }

        if (useStrike)
        {
            StrikeMovement(distToPlayer, moveDir, toPlayerDir);
        }
        else
        {
            ClassicMovement(distToPlayer, moveDir);
        }

        if (agent != null && agent.enabled && agent.isOnNavMesh) agent.nextPosition = transform.position;
    }

    private void StrikeMovement(float distToPlayer, Vector3 moveDir, Vector3 toPlayerDir)
    {
        Vector3 velocity = Vector3.zero;

        switch (combatState)
        {
            case CombatState.Approaching:
                // Move toward hold distance.
                if (distToPlayer > actualHoldDistance)
                {
                    velocity = moveDir * actualSpeed;
                }
                else
                {
                    combatState = CombatState.Holding;
                }
                break;

            case CombatState.Holding:
                // Stand still at hold distance, but re-approach if player moves away.
                if (distToPlayer > actualHoldDistance + 1f)
                {
                    velocity = moveDir * actualSpeed;
                }
                else
                {
                    velocity = Vector3.zero;
                }
                break;

            case CombatState.Striking:
                // Charge toward player, but stop at attack range.
                if (distToPlayer > attack.range * 0.95)
                {
                    velocity = moveDir * actualSpeed * strikeSpeedMultiplier;
                }
                else
                {
                    velocity = Vector3.zero;
                }
                break;

            case CombatState.Attacking:
                // Stand still while the attack animation plays.
                velocity = Vector3.zero;
                break;

            case CombatState.Retreating:
                // Move away from player.
                if (distToPlayer < actualHoldDistance)
                {
                    velocity = -toPlayerDir * actualSpeed * retreatSpeedMultiplier;
                }
                else
                {
                    combatState = CombatState.Holding;
                }
                break;
        }
        ApplyVelocity(velocity);
    }

    private void ClassicMovement(float distToPlayer, Vector3 moveDir)
    {
        // Slow down as we approach stop distance.
        float currentSpeed = actualSpeed;
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

        ApplyVelocity(moveDir * currentSpeed);
    }

    private void ApplyVelocity(Vector3 desired)
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.Lerp(
                rb.linearVelocity,
                new Vector3(desired.x, rb.linearVelocity.y, desired.z), smoothVelocity);
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
        TryTrigger(shieldBreakTrigger);
    }

    private IEnumerator ChargeUpThenDamage()
    {
        if (attack.chargeTime > 0f) yield return new WaitForSeconds(attack.chargeTime);
        DoDamage();
    }

    protected virtual void DoDamage()
    {
        if (IsDying || _playerHealthRef == null) return;

        Vector3 toPlayer = _playerTransformRef.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude <= attack.range * attack.range)
        {
            _playerHealthRef.TakeDamage(attack.damage);
        }
    }

    public void AnimDealDamage()
    {
        if (useDamageAnimEvent) DoDamage();
    }

    public void ApplyKnockback(Vector3 force, bool playHitAnim = true)
    {
        IsKnockedBack = true;
        knockbackTimer = KnockbackTime;

        if (agent != null) agent.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(force, ForceMode.Impulse);
        }

        if (playHitAnim) TryTrigger(hitTrigger);
    }

    private void HandleKnockback()
    {
        knockbackTimer -= Time.deltaTime;
        if (knockbackTimer > 0f) return;
        if (!IsGrounded()) return;

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

        // After knockback, re-approach from wherever we ended up.
        if (useStrike) combatState = CombatState.Approaching;
    }

    public void KilledBy(Collider other, AttackHitbox hitBox)
    {
        if (IsDying) return;

        IsDying = true;
        IsKnockedBack = true;
        knockbackTimer = KnockbackTime;
        deathTimer = maxDeathTime;

        if (agent != null) agent.enabled = false;

        Renderer r = GetComponent<Renderer>() ?? GetComponentInChildren<Renderer>();
        if (r != null) r.material.color = Color.gray;

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

        if (!string.IsNullOrEmpty(idleBoolParam))
            anim.SetBool(idleBoolParam, animSpeed < idleSpeedThreshold);
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, GroundCheckDistance + 0.2f);
    }

    private void TryTrigger(string triggerName)
    {
        if (anim != null && !string.IsNullOrEmpty(triggerName)) anim.SetTrigger(triggerName);
    }
}
