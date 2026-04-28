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
    private enum CombatState { Approaching, Holding, Striking, Attacking, Retreating, Wandering, Idling }

    // References
    [HideInInspector] public GameObject shield;
    private Rigidbody rb;
    private AudioSource _shieldBreakAudioSource;
    [HideInInspector] public PlayerHealth _playerHealthRef;
    [HideInInspector] public Transform _playerTransformRef;
    [HideInInspector] public Collider _playerBodyCollider;
    private NavMeshAgent agent;

    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField, Range(0f, 0.3f)] private float speedVariance = 0.15f;
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
    [SerializeField, Range(0f, 0.5f)] private float holdDistanceVariance = 0.3f;
    [SerializeField] private float strikeSpeedMultiplier = 2.5f;
    [SerializeField] private float retreatSpeedMultiplier = 1.5f;
    // Proportion of attack.range where strike enemies stop charging and commit the attack.
    // Clamped below 1 so stop distance is always strictly less than attack.range.
    [SerializeField, Range(0f, 1f)] private float strikeStopRatio = 0.7f;

    [Header("Ranged Behavior (used when !useStrike)")]
    [SerializeField] private float stopFromPlayerDistance = 1.5f;

    [Header("Wandering (when player is out of sight)")]
    [SerializeField] private float sightRange = 25f;
    [SerializeField] private RandomMovementSettings randomMovement = new RandomMovementSettings
    {
        radius = 12f,
        minIdleDuration = 1.5f,
        maxIdleDuration = 4f,
        speed = 2f
    };
    private float wanderIdleEndTime;
    private Vector3 _wanderProgressPos;
    private float _wanderProgressTime;
    private const float WanderStallTime = 1.5f;
    private const float WanderProgressDist = 0.5f;

    [Header("Knockback & Death")]
    [SerializeField] private float maxDeathTime = 4f;
    private const float KnockbackTime = 0.5f;
    private const float GroundCheckDistance = 0.4f;

    private CombatState combatState = CombatState.Approaching;
    private float actualHoldDistance;
    private float actualSpeed;
    private float StrikeStopDistance => attack.range * Mathf.Min(Mathf.Clamp01(strikeStopRatio), 0.999f);

    [Header("Animation (optional)")]
    [SerializeField] private Animator anim;
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private float idleSpeedThreshold = 0.1f;
    [SerializeField] private string attackTrigger = "Attack";
    [SerializeField] private string shieldBreakTrigger = "ShieldBreak";
    [SerializeField] private string hitTrigger = "Hit";
    [SerializeField] private string deadTrigger = "Die";
    [SerializeField] private bool useDamageAnimEvent = false;

    [SerializeField] private bool useTutorialKillLock = false;
    public bool CanBeKilled { get; private set; } = true;

    public bool IsKnockedBack { get; private set; }
    public bool IsDying { get; private set; }
    public event Action OnDied;

    public bool HasShield() => shield != null;
    public bool WasShielded { get; private set; }

    // Timers
    private float knockbackTimer;
    private float timeOfNextAttack;
    private float deathTimer;

    private const float AnimSampleInterval = 0.15f;
    private Vector3 _animPrevSamplePos;
    private float _animPrevSampleTime;
    private Vector3 _animCurSamplePos;
    private float _animCurSampleTime;

    // Stuck-escape: if we've been commanding movement for StuckTimeThreshold but
    // haven't actually translated, sidestep for UnstuckDuration to break free.
    private const float StuckTimeThreshold = 0.4f;
    private const float StuckMovedSqr = 0.04f;
    private const float StuckCommandedSqr = 1f;
    private const float UnstuckDuration = 0.3f;
    private Vector3 _stuckCheckPos;
    private float _stuckCheckTime;
    private float _unstuckUntil;
    private Vector3 _unstuckDir;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        _shieldBreakAudioSource = GetComponent<AudioSource>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
        ShieldHit shieldHit = GetComponentInChildren<ShieldHit>();
        if (shieldHit != null) shield = shieldHit.gameObject;
        WasShielded = shield != null;

        // Each enemy gets a slightly different hold distance (Stalk distance should ALWAYS be further than attack range).
        actualHoldDistance = holdDistance * (1f + UnityEngine.Random.Range(-holdDistanceVariance, holdDistanceVariance));
        actualHoldDistance = Mathf.Max(actualHoldDistance, attack.range + 0.5f);

        actualSpeed = speed * (1f + UnityEngine.Random.Range(-speedVariance, speedVariance));

        _animPrevSamplePos = _animCurSamplePos = transform.position;
        _animPrevSampleTime = _animCurSampleTime = Time.time;

        _stuckCheckPos = transform.position;
        _stuckCheckTime = Time.time;

        SetupNavMesh();
    }

    public void EnableTutorialKillLockMode()
    {
        useTutorialKillLock = true;
        CanBeKilled = true;
    }

    public void SetCanBeKilled(bool canBeKilled)
    {
        if (!useTutorialKillLock)
        {
            return;
        }

        CanBeKilled = canBeKilled;
    }

    void Start() => ResolvePlayerRefs();

    private void ResolvePlayerRefs()
    {
        if (_playerHealthRef != null && _playerTransformRef != null) return;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        if (_playerTransformRef == null) _playerTransformRef = player.transform;
        if (_playerHealthRef == null) _playerHealthRef = player.GetComponent<PlayerHealth>();
        // CharacterController is the player body collider (not the hammer).
        // Range checks use this so a blocked enemy — hemmed in by the horse's collider
        // or a slope edge — still registers as "in range" once it's touching the body.
        if (_playerBodyCollider == null) _playerBodyCollider = player.GetComponent<CharacterController>();
    }

    // Horizontal distance from our pivot to the nearest point on the player's body collider.
    // Falls back to pivot-to-pivot distance if the collider isn't resolved yet.
    private float HorizontalDistanceToPlayerBody()
    {
        Vector3 target = _playerBodyCollider != null
            ? _playerBodyCollider.ClosestPoint(transform.position)
            : _playerTransformRef.position;
        Vector3 toPlayer = target - transform.position;
        toPlayer.y = 0f;
        return toPlayer.magnitude;
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
            agent.radius = capsule.radius * 1.5f;
            agent.height = capsule.height;
            agent.baseOffset = capsule.center.y - capsule.height * 0.5f;
        }

        agent.avoidancePriority = UnityEngine.Random.Range(30, 70);
    }

    void Update()
    {
        if (IsDying) { KillEnemy(); return; }
        if (IsKnockedBack) { HandleKnockback(); return; }

        if (_playerHealthRef == null || _playerTransformRef == null)
        {
            ResolvePlayerRefs();
            return;
        }

        // Awareness gating: when the player is out of sight, fall back to NPC-style wandering.
        // Hysteresis (+2) prevents flapping if the player hovers right at the boundary.
        float distToPlayer = HorizontalDistanceToPlayerBody();
        bool isWandering = combatState == CombatState.Wandering || combatState == CombatState.Idling;

        if (isWandering)
        {
            if (distToPlayer <= sightRange)
            {
                combatState = CombatState.Approaching;
            }
            else
            {
                UpdateWander();
                UpdateAnim();
                return;
            }
        }
        else if ((combatState == CombatState.Approaching || combatState == CombatState.Holding)
                 && distToPlayer > sightRange + 2f)
        {
            EnterWandering();
            UpdateAnim();
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

    private void UpdateWander()
    {
        switch (combatState)
        {
            case CombatState.Wandering:
                // Arrived, or the pathfinder gave up — either way, pause and pick again later.
                if (agent != null && !agent.pathPending && (!agent.hasPath || agent.remainingDistance < 0.5f))
                {
                    EnterWanderIdle();
                    break;
                }
                // Stalled (e.g. walking into an unbaked obstacle): give up the current target,
                // briefly idle, then pick a new one in EnterWandering. Otherwise the agent thinks
                // its path is still valid and the script keeps pushing into the wall forever.
                if ((transform.position - _wanderProgressPos).sqrMagnitude > WanderProgressDist * WanderProgressDist)
                {
                    _wanderProgressPos = transform.position;
                    _wanderProgressTime = Time.time;
                }
                else if (Time.time - _wanderProgressTime > WanderStallTime)
                {
                    EnterWanderIdle();
                }
                break;
            case CombatState.Idling:
                if (Time.time >= wanderIdleEndTime) EnterWandering();
                break;
        }
    }

    private void EnterWandering()
    {
        combatState = CombatState.Wandering;
        _wanderProgressPos = transform.position;
        _wanderProgressTime = Time.time;
        PickNewWanderPoint();
    }

    private void EnterWanderIdle()
    {
        combatState = CombatState.Idling;
        if (agent != null && agent.isOnNavMesh) agent.ResetPath();
        wanderIdleEndTime = Time.time + UnityEngine.Random.Range(randomMovement.minIdleDuration, randomMovement.maxIdleDuration);
    }

    private void PickNewWanderPoint()
    {
        if (agent == null || !agent.isOnNavMesh) return;
        Vector3 candidate = transform.position + UnityEngine.Random.insideUnitSphere * randomMovement.radius;
        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, randomMovement.radius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
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
                // Commit the attack once we're inside the stop ratio — gives a buffer so a slow-drifting
                // player doesn't slip outside attack.range during chargeTime.
                if (HorizontalDistanceToPlayerBody() <= StrikeStopDistance)
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

        if (HorizontalDistanceToPlayerBody() > attack.range) return;

        PerformAttack();
    }

    private IEnumerator StrikeDamageThenRetreat()
    {
        if (attack.chargeTime > 0f) yield return new WaitForSeconds(attack.chargeTime);
        if (!useDamageAnimEvent) DoDamage();
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
        if (IsDying || IsKnockedBack) return;
        if (_playerTransformRef == null) return;

        bool isWandering = combatState == CombatState.Wandering || combatState == CombatState.Idling;

        // Direction is taken to the pivot (stable) — distance is taken to the body collider
        // (so movement/attack thresholds aren't fooled by the horse's collider extent).
        Vector3 toPivot = _playerTransformRef.position - transform.position;
        toPivot.y = 0f;
        float pivotDist = toPivot.magnitude;
        Vector3 toPlayerDir = pivotDist > 0.01f ? toPivot / pivotDist : Vector3.zero;
        float distToPlayer = HorizontalDistanceToPlayerBody();

        // NavMesh pathfinding direction. While wandering, the destination was already set when the
        // state was entered — don't overwrite it with the player's position.
        Vector3 moveDir = isWandering ? Vector3.zero : toPlayerDir;
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            if (!isWandering) agent.SetDestination(_playerTransformRef.position);
            Vector3 desiredVel = agent.desiredVelocity;
            desiredVel.y = 0f;
            if (desiredVel.sqrMagnitude > 0.0001f) moveDir = desiredVel.normalized;
        }

        // Face the player when engaged; face the move direction while wandering.
        Vector3 faceDir = isWandering ? moveDir : toPlayerDir;
        if (faceDir.sqrMagnitude > 0.0001f)
        {
            Quaternion finalRotation = Quaternion.LookRotation(faceDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, finalRotation, Time.fixedDeltaTime * rotationSpeed);
        }

        if (isWandering)
        {
            Vector3 wanderVel = combatState == CombatState.Idling ? Vector3.zero : moveDir * randomMovement.speed;
            ApplyVelocity(wanderVel);
        }
        else if (useStrike)
        {
            StrikeMovement(distToPlayer, moveDir, toPlayerDir);
        }
        else
        {
            ClassicMovement(distToPlayer, moveDir);
        }

        if (agent != null && agent.enabled && agent.isOnNavMesh) agent.nextPosition = transform.position;

        UpdateStuckEscape(toPlayerDir);
    }

    private void UpdateStuckEscape(Vector3 toPlayerDir)
    {
        if (rb == null) return;
        if (Time.time < _unstuckUntil) return; // already escaping; don't update detection

        Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        bool commandingMovement = horizontalVel.sqrMagnitude > StuckCommandedSqr;
        bool moved = (transform.position - _stuckCheckPos).sqrMagnitude > StuckMovedSqr;

        if (!commandingMovement || moved)
        {
            _stuckCheckPos = transform.position;
            _stuckCheckTime = Time.time;
            return;
        }

        if (Time.time - _stuckCheckTime <= StuckTimeThreshold) return;

        // Stuck — pick a perpendicular direction and sidestep briefly. Random side
        // so two enemies wedged against each other don't both pick the same way.
        Vector3 right = Vector3.Cross(Vector3.up, toPlayerDir);
        if (right.sqrMagnitude < 0.0001f) right = Vector3.right;
        right.Normalize();
        _unstuckDir = UnityEngine.Random.value < 0.5f ? right : -right;
        _unstuckUntil = Time.time + UnstuckDuration;
        _stuckCheckPos = transform.position;
        _stuckCheckTime = Time.time;
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
                // Charge toward player, but stop at the commit distance.
                if (distToPlayer > StrikeStopDistance)
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
        // While escaping a stuck position, override whatever the AI wanted with a
        // sideways push at normal speed.
        if (Time.time < _unstuckUntil)
        {
            desired = _unstuckDir * actualSpeed;
        }

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

    public void BreakShieldFromAttack(Collider attacker, AttackHitbox attack)
    {
        float force = attack.GetKnockbackForce();
        Vector3 dir = transform.position - attacker.transform.position;
        dir.y = Mathf.Clamp(force / 75f, 0.2f, 1.5f);
        dir.Normalize();
        ApplyKnockback(dir * force, playHitAnim: false);
        BreakShield();
    }

    private IEnumerator ChargeUpThenDamage()
    {
        if (attack.chargeTime > 0f) yield return new WaitForSeconds(attack.chargeTime);
        DoDamage();
    }

    protected virtual void DoDamage()
    {
        if (IsDying || _playerHealthRef == null) return;

        if (HorizontalDistanceToPlayerBody() <= attack.range)
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

    public void getKilledBasic(Vector3 knockback)
    {
        IsDying = true;
        IsKnockedBack = true;
        knockbackTimer = KnockbackTime;
        deathTimer = maxDeathTime;

        if (agent != null) agent.enabled = false;


        rb.AddForce(knockback);

        Debug.Log("i died :(");

        TryTrigger(deadTrigger);
        OnDied?.Invoke();
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

    public void KilledByFire(Vector3 sourcePosition, float force = 18f)
    {
        if (IsDying) return;
        if (!CanBeKilled) return;

        IsDying = true;
        IsKnockedBack = true;
        knockbackTimer = KnockbackTime;
        deathTimer = maxDeathTime;

        if (agent != null) agent.enabled = false;

        Renderer r = GetComponent<Renderer>() ?? GetComponentInChildren<Renderer>();
        if (r != null) r.material.color = new Color(0.2f, 0.2f, 0.2f);

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

        // Drive the animator from net translation over a ~0.15-0.3s window, not
        // per-frame delta. When the rigidbody is jammed against geometry, physics
        // depenetration jitters it each frame — per-frame |delta| is non-zero but
        // net displacement over the window cancels out, so the legs correctly idle.
        Vector3 currentPos = transform.position;

        if (Time.time - _animCurSampleTime >= AnimSampleInterval)
        {
            _animPrevSamplePos = _animCurSamplePos;
            _animPrevSampleTime = _animCurSampleTime;
            _animCurSamplePos = currentPos;
            _animCurSampleTime = Time.time;
        }

        float dt = Time.time - _animPrevSampleTime;
        float animSpeed = 0f;
        if (dt > 0.05f)
        {
            Vector3 delta = currentPos - _animPrevSamplePos;
            animSpeed = new Vector2(delta.x, delta.z).magnitude / dt;
        }

        if (animSpeed < idleSpeedThreshold) animSpeed = 0f;
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
}
