using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy
{
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
        [SerializeField, UnityEngine.Range(0f, 0.3f)] private float speedVariance = 0.15f;
        [SerializeField] private float smoothVelocity = 0.35f;
        [SerializeField] private float rotationSpeed = 8f;

        [SerializeField] protected EnemyAttack attack = new EnemyAttack
        {
            damage = 10,
            range = 2.5f,
            cooldown = 2f,
            chargeTime = 0.25f
        };

        /* variables related to striking are here (used for melee and shielded but not ranged or rapid)
        if you're making a new enemy, you don't need to tune these unless you tick the use strike behaviour checkbox
        and you want it to do the retreating back to the set distance from the player */
        [Header("Strike Behavior")]
        [SerializeField] protected bool useStrike = true;
        [SerializeField] private float holdDistance = 6f;
        [SerializeField, UnityEngine.Range(0f, 0.5f)] private float holdDistanceVariance = 0.3f;
        [SerializeField] private float strikeSpeedMultiplier = 2.5f;
        [SerializeField] private float retreatSpeedMultiplier = 1.5f;
        // proportion of attack.range where strike enemies stop charging and commit the attack
        // it's clamped below 1 so stop distance is always strictly less than attack.range because this was leading to bugs earlier
        [SerializeField, UnityEngine.Range(0f, 1f)] private float strikeStopRatio = 0.7f;

        [Header("Ranged Behavior (used INSTEAD of striking)")]
        [SerializeField] private float stopFromPlayerDistance = 1.5f;

        [Header("ambient behaviour)")]
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
        [SerializeField] private RagdollToggler ragdollToggler;
        public IKnockbackState KnockbackHandler { get; private set; }
        public IDeathState DeathHandler { get; private set; }

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

        public bool HasShield() => shield != null;
        public bool WasShielded { get; private set; }

        private float timeOfNextAttack;

        private const float AnimSampleInterval = 0.15f;
        private Vector3 _animPrevSamplePos;
        private float _animPrevSampleTime;
        private Vector3 _animCurSamplePos;
        private float _animCurSampleTime;

        // do a step to the side if enemy is supposed to move but has stalled (doesn't move)
        private const float StuckTimeThreshold = 0.9f;
        private const float StuckMovedSqr = 0.04f;
        private const float StuckCommandedSqr = 1f;
        private const float UnstuckDuration = 0.3f;
        private Vector3 _stuckCheckPos;
        private float _stuckCheckTime;
        private float _unstuckUntil;
        private Vector3 _unstuckDir;

        // interval because it doesn't need to calculate each path every frame (initial implementation - too slow)
        private const float DestinationUpdateInterval = 0.2f;
        private float _nextDestinationTime;
        private const float CloseFacingFreezeDistance = 1.0f;
        private AudioSource audioSource; // audios start here

        public AudioClip swordClip;
        public AudioClip swordClip1;
        public AudioClip swordClip2;
        private AudioClip[] swordSounds;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            _shieldBreakAudioSource = GetComponent<AudioSource>();
            if (anim == null) anim = GetComponentInChildren<Animator>();
            ShieldHit shieldHit = GetComponentInChildren<ShieldHit>();
            if (shieldHit != null) shield = shieldHit.gameObject;
            WasShielded = shield != null;

            // each enemy gets a slightly different hold distance, added a little randomness (Stalk distance should ALWAYS be further than attack range).
            // the randomness was a good idea but doesn't seem to make a playable difference, may remove later
            actualHoldDistance = holdDistance * (1f + UnityEngine.Random.Range(-holdDistanceVariance, holdDistanceVariance));
            actualHoldDistance = Mathf.Max(actualHoldDistance, attack.range + 0.5f);

            actualSpeed = speed * (1f + UnityEngine.Random.Range(-speedVariance, speedVariance));

            _animPrevSamplePos = _animCurSamplePos = transform.position;
            _animPrevSampleTime = _animCurSampleTime = Time.time;

            _stuckCheckPos = transform.position;
            _stuckCheckTime = Time.time;
            
            SetupNavMesh();
            
            KnockbackHandler kbHandler = GetComponent<KnockbackHandler>();
            if (kbHandler != null)
            {
                kbHandler.KnockbackEnded += SetApproachState;
                KnockbackHandler = kbHandler;
            }
            
            RagdollDeathHandler deathHandler = GetComponent<RagdollDeathHandler>();
            if (deathHandler != null)
            {
                deathHandler.Init(ragdollToggler, KnockbackHandler);
                DeathHandler = deathHandler;
            }
            else
            {
                OldDeathHandler legacyDeathHandler = GetComponent<OldDeathHandler>();
                if (legacyDeathHandler != null)
                {
                    legacyDeathHandler.Init(KnockbackHandler);
                    DeathHandler = legacyDeathHandler;
                }
            }

            Debug.Assert(DeathHandler != null);

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f;
            audioSource.volume = 1f;
            audioSource.playOnAwake = false;

            swordSounds = new AudioClip[]
            {
                swordClip,
                swordClip1,
                swordClip2
            };
        }

        void Start()
        {
            ResolvePlayerRefs();
        }

        private void ResolvePlayerRefs()
        {
            // basic reference checks, add more here if you see a player ref bug
            if (_playerHealthRef != null && _playerTransformRef != null) return;

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            if (_playerTransformRef == null) _playerTransformRef = player.transform;
            if (_playerHealthRef == null) _playerHealthRef = player.GetComponent<PlayerHealth>();
            if (_playerBodyCollider == null) _playerBodyCollider = player.GetComponent<CharacterController>();
        }

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
            if (DeathHandler.IsDying) return;
            if (KnockbackHandler.IsKnockedBack)
            {
                return;
            }

            if (_playerHealthRef == null || _playerTransformRef == null)
            {
                ResolvePlayerRefs();
                return;
            }

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
                    if (agent != null && !agent.pathPending && (!agent.hasPath || agent.remainingDistance < 0.5f))
                    {
                        EnterWanderIdle();
                        break;
                    }

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

            NavMeshPath path = new NavMeshPath();
            for (int attempt = 0; attempt < 8; attempt++)
            {
                Vector3 candidate = transform.position + UnityEngine.Random.insideUnitSphere * randomMovement.radius;
                if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas)) continue;
                if (!agent.CalculatePath(hit.position, path) || path.status != NavMeshPathStatus.PathComplete) continue;

                agent.SetDestination(hit.position);
                return;
            }
        }

        // attack cycle state system
        private void StrikeUpdate()
        {
            if (_playerHealthRef.IsDead) return;

            switch (combatState)
            {
                case CombatState.Holding:
                    // wait for cooldown, then start striking
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
                // note to understand logic for the different prefabs:
                // these are the changes now the prefabs have diff behaviour:
                // StrikeMovement() in FixedUpdate() does Approaching and Retreating
                // If there's no approach and retreat (e.g. for ranged enemies) then ClassicMovement().
            }
        }

        // The classic system, currently used by ranged and rapid enemies
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
            if (DeathHandler.IsDying) return;
            if (KnockbackHandler.IsKnockedBack) return;
            if (_playerTransformRef == null) return;

            bool isWandering = combatState == CombatState.Wandering || combatState == CombatState.Idling;

            Vector3 toPivot = _playerTransformRef.position - transform.position;
            toPivot.y = 0f;
            float pivotDist = toPivot.magnitude;
            Vector3 toPlayerDir = pivotDist > 0.01f ? toPivot / pivotDist : Vector3.zero;
            float distToPlayer = HorizontalDistanceToPlayerBody();

            Vector3 moveDir = Vector3.zero;
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                if (!isWandering && Time.time >= _nextDestinationTime)
                {
                    agent.SetDestination(_playerTransformRef.position);
                    _nextDestinationTime = Time.time + DestinationUpdateInterval;
                }
                Vector3 desiredVel = agent.desiredVelocity;
                desiredVel.y = 0f;
                if (desiredVel.sqrMagnitude > 0.0001f) moveDir = desiredVel.normalized;
            }
            else if (!isWandering)
            {
                // No NavMesh available — fall back to direct line so combat still works.
                moveDir = toPlayerDir;
            }

            // makes it look to the player when engaged but look at the direction it's moving at otherwise
            Vector3 faceDir = isWandering ? moveDir : toPlayerDir;
            bool freezeFacing = !isWandering && pivotDist < CloseFacingFreezeDistance;
            if (!freezeFacing && faceDir.sqrMagnitude > 0.0001f)
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
            if (Time.time < _unstuckUntil) return; 

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
                // for the approaching state, the enemy move toward its position.
                case CombatState.Approaching:
                    if (distToPlayer > actualHoldDistance)
                    {
                        velocity = moveDir * actualSpeed;
                    }
                    else
                    {
                        combatState = CombatState.Holding;
                    }
                    break;

                // enemy should be standing still at a set distance from the player here
                case CombatState.Holding:
                    if (distToPlayer > actualHoldDistance + 1f)
                    {
                        velocity = moveDir * actualSpeed;
                    }
                    else
                    {
                        velocity = Vector3.zero;
                    }
                    break;
                // charge at the player from the set position but stop when its weapon feels like it could touch the player
                // adjust this depending on the weapon model if it changes later
                case CombatState.Striking:
                    if (distToPlayer > StrikeStopDistance)
                    {
                        velocity = moveDir * actualSpeed * strikeSpeedMultiplier;
                    }
                    else
                    {
                        velocity = Vector3.zero;
                    }
                    break;
                // stand stationary when the attacking animation plays - could change depending on attack length
                case CombatState.Attacking:
                    velocity = Vector3.zero;
                    break;
                // go back to the target set position so the hammer can attack more easily  
                case CombatState.Retreating:
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
            // To escape STUCK, overwrite whatever the NavMesh agent wants with just a sideways push.
            // Ignore NavMesh agent completely
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
            KnockbackHandler.ApplyKnockback(dir * force);
            
            //TryTrigger(hitTrigger);
            
            BreakShield();
            
            attack.BrokeShield();
        }

        private IEnumerator ChargeUpThenDamage()
        {
            if (attack.chargeTime > 0f) yield return new WaitForSeconds(attack.chargeTime);
            DoDamage();
        }

        protected virtual void DoDamage()
        {
            AudioClip clip = swordSounds[UnityEngine.Random.Range(0, swordSounds.Length)];
            if (clip != null)
            {
                audioSource.PlayOneShot(clip, 1f);
            }

            if (DeathHandler.IsDying || _playerHealthRef == null) return;

            if (HorizontalDistanceToPlayerBody() <= attack.range)
            {
                _playerHealthRef.TakeDamage(attack.damage);
            }
        }

        public void AnimDealDamage()
        {
            if (useDamageAnimEvent) DoDamage();
        }

        private void UpdateAnim()
        {
            if (anim == null || string.IsNullOrEmpty(speedParam)) return;

            Vector3 currentPos = transform.position;

            if (Time.time - _animCurSampleTime >= AnimSampleInterval)
            {
                _animPrevSamplePos = _animCurSamplePos;
                _animPrevSampleTime = _animCurSampleTime;
                _animCurSamplePos = currentPos;
                _animCurSampleTime = Time.time;
            }

            float dt = Time.time - _animPrevSampleTime;float animSpeed = 0f;
                if (dt > 0.05f)
                {
                    Vector3 delta = currentPos - _animPrevSamplePos;
                    animSpeed = new Vector2(delta.x, delta.z).magnitude / dt;
            }
            
            if (animSpeed < idleSpeedThreshold) animSpeed = 0f;
            anim.SetFloat(speedParam, animSpeed);
        }

        private void SetApproachState()
        {
            // After knockback, re-approach from wherever we ended up.
            if (useStrike)
            {
                combatState = CombatState.Approaching;
            }
        }

        private void TryTrigger(string triggerName)
        {
            if (anim != null && !string.IsNullOrEmpty(triggerName)) anim.SetTrigger(triggerName);
        }

        private void OnDisable()
        {
            if (KnockbackHandler is KnockbackHandler handler)
            {
                handler.KnockbackEnded -= SetApproachState;
            }
        }
    }
}