using System;
using UnityEngine;

public struct ZoneSettings
{
    public float radius;
    public float weight;
}

public struct AdaptiveRepelSettings
{
    public float speedThreshold;
    public ZoneSettings normal;
    public ZoneSettings tooSlow;
    public ZoneSettings GetSettings(float planarSpeed)
    {
        if (planarSpeed < speedThreshold) return tooSlow;
        return normal;
    }
}

public struct StillStuckSettings
{
    public float minSpeedThreshold;
    public float lateralNudgeDisplacement;

}

public class EnemyMovement : MonoBehaviour
{
    /**
    Variables explained:
    defaultSpeed: speed E travels at when OUT of formation
    formationSpeed: speed E travels at when IN formation
    spawner: every E is connected to a spawner
    _formationTarget: when IN formation, the position that E should move towards (in formation grid)
    _attackTarget: when NOT in formation, the position that E should move towards
    */
    public float defaultSpeed;
    public float formationSpeed;
    public EnemySpawner spawner;
    public GameObject shield;

    [Header("Smooth Movement Variables")]
    public float arriveRadius = 0.6f; // Distance from target before enemy slows down.
    public float stopRadius = 0.18f; // Distance from target when enemy halts.
    public float velocityLerp = 0.25f; // How quickly desired velocity becomes current velocity.
    /* 
    There are 2 zones implemented around each player that prevent clumping.
    
    REPELS in a smaller zone
    A force of repulsion of strength {repelStrength} repels 2 enemies inversely proportional to distance between.
    It only has effect when distance is less than the {repelRadius} threshold. (It's analagous to the force-distance graph of the strong nuclear force but for repulsion rather than attraction.)

    SLOWS down in a slightly bigger zone: {slowZoneRadius, slowFactor}
    */
    [Header("Prevent Clumping")]
    public float slotJitterRadius = 0.08f;
    public float minForwardClearLen = 0.45f;

    [HideInInspector] public PlayerHealth _playerHealthRef;
    [HideInInspector] public Transform _playerTransformRef;
    private Rigidbody _rb;

    public bool isKnockedBack;
    public bool shieldWasJustHit = false;
    private Vector3 _formationTarget;
    private Vector3 _attackTarget;
    public bool hasFormationTarget;
    private bool _hasAttackTarget; 
    private float _sideBias = 1f;

    public AdaptiveRepelSettings repel = new AdaptiveRepelSettings
    {
        speedThreshold = 0.12f,
        normal = new ZoneSettings
        {
            radius = 1.15f, // Distance where enemies repel each other (analagous to protons in a nucleus, prevents clumping together)
            weight = 1.15f // Decides how strong the repel force is 
        },
        tooSlow = new ZoneSettings
        {
            radius = 1.6f,
            weight = 2.2f
        }
    };

    public ZoneSettings slowZone = new ZoneSettings
    {
        radius = 0.9f, // radius of zone (max distance of effect)
        weight = 0.35f // min speed multiplier when heavily crowded
    }; 
    public StillStuckSettings stillStuckSettings = new StillStuckSettings 
    { 
        minSpeedThreshold = 0.18f, // min speed to count as 'stuck'
        lateralNudgeDisplacement = 0.6f // distance that the enemy is moved if it's not moving (stuck)
    };

    private void Awake()
    {
        _sideBias = UnityEngine.Random.value > 0.5f ? 1f : -1f;
        Vector2 jitter2D = UnityEngine.Random.insideUnitCircle * slotJitterRadius;
        _slotJitter = new Vector3(jitter2D.x, 0f, jitter2D.y);
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        GameObject playerRef = GameObject.FindWithTag("Player");
        _shieldBreakAudioSource = GetComponent<AudioSource>();

        if (playerRef != null)
        {
            _playerTransformRef = playerRef.transform;
            _playerHealthRef = playerRef.GetComponent<Health>();
        }
        
        if (_shieldBreakAudioSource == null)
        {
            Debug.LogError("man there's no audio source");
        }

        // Default values (if none set)
        if (defaultSpeed <= 0f) defaultSpeed = 3f;
        if (formationSpeed <= 0f) formationSpeed = (2f/3f) * defaultSpeed;
        
        currentKnockbackTimer = knockbackTime;
    }

    // Update is called once per frame
    private void Update()
    {
        if (IsKnockedBack)
        {
            currentKnockbackTimer -= Time.deltaTime;
            Vector3 rayOrigin = transform.position;
            bool isGrounded = Physics.Raycast(rayOrigin, Vector3.down, deathGroundCheckDistance, groundMask);
            // Debug.DrawRay(rayOrigin, Vector3.down * deathGroundCheckDistance, Color.yellow);
            
            //End knockback state if on ground AND only if a certain amount of time has passed
            //(as enemy doesn't get launched vertically off ground when their shield breaks)
            if (isGrounded && currentKnockbackTimer <= 0)
            {
                IsKnockedBack = false;
                currentKnockbackTimer = knockbackTime;
            }
        }
        if (IsDying)
        {
            onDeathTimer -= Time.deltaTime;
            
            //If the enemy gets launched entirely off the map,
            //onDeathTimer ensures it will still eventually be destroyed.
            //But otherwise, will destroy when enemy reaches the ground
            if (IsKnockedBack == false || onDeathTimer <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    private void FixedUpdate()
    {
        ShieldWasJustHit = false;
        if (IsKnockedBack)
        {
            return;
        }

        if (_playerTransformRef == null)
        {
            GameObject playerRef = GameObject.FindWithTag("Player");
            if (playerRef == null) return;
            _playerTransformRef = playerRef.transform;
            _playerHealthRef = playerRef.GetComponent<PlayerHealth>();
        }

        //TODO use a* pathfinding instead

        float speed = defaultSpeed;

        /**
        IF in formation, go to formation.
        ELSE IF has attack target, go to attack target.
        ELSE go to player.*/
        Vector3 targetPosition = hasFormationTarget
            ? _formationTarget
            : (_hasAttackTarget ? _attackTarget : _playerTransformRef.position);
        Vector3 targetDirection = (targetPosition - transform.position).normalized;
        
        // If it meets the criteria of being in a formation, keep in formation.
        float distanceToFormation = Vector3.Distance(transform.position, _formationTarget);
        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransformRef.position);
        if (hasFormationTarget && (distanceToPlayer > spawner.breakFormationDistance) && (distanceToFormation < spawner.joinFormationDistance))
        {
            speed = formationSpeed;
        }

        /**
        If E is trying to get in formation but not currently in the correct position, it continues to move at defaultSpeed.
        Once E is in the actual formation position, it moves at formationSpeed.
        */
        if (hasFormationTarget) {
            if (distanceToFormation < 0.5f) { speed = formationSpeed; }
        }

        Vector3 playerPosition = _playerTransformRef.position;

        Vector3 direction = (playerPosition - transform.position).normalized;

        // Enemy actually needs to face the player
        if (direction != Vector3.zero)
        {
            // Need to stop the enemy from tilting forward when they are close
            Vector3 flatDirection = new Vector3(direction.x, 0, direction.z).normalized;
            if (flatDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(flatDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);
            }
        }

        _rb.linearVelocity = new Vector3(targetDirection.x * speed, _rb.linearVelocity.y, targetDirection.z * speed);

        // If the enemy is in a formation, break it when it's sufficiently close to the player
        if (hasFormationTarget && spawner != null && distanceToPlayer < spawner.breakFormationDistance)
        {
            ClearFormationTarget();
            spawner.UpdateFormationTargets();
        }
    }

    public void KilledBy(Collider other)
    {
        //Grey out enemy to signify that it's dead
        gameObject.GetComponent<Renderer>().material.color = Color.gray;
        
        if (spawner != null)
        {
            spawner.RemoveEnemy(this);
        }

        float knockbackForce = 40f;
        
        //Knock away from what killed it
        Vector3 knockbackDirection = transform.position - other.transform.position;
        knockbackDirection.y = 1.0f;
        knockbackDirection.Normalize();

        ApplyKnockback(knockbackDirection * knockbackForce);

        IsKnockedBack = true;
        IsDying = true;
    }

    public void ApplyKnockback(Vector3 force)
    {
        IsKnockedBack = true;

        _rb.linearVelocity = Vector3.zero;
        _rb.AddForce(force, ForceMode.Impulse);
    }

    public void BreakShield()
    {
        if (shield != null)
        {
            Destroy(shield);
            shield = null;
            _shieldBreakAudioSource.Play();
        }
    }

    public bool HasShield()
    {
        return shield != null;
    }

    public void MarkShieldHit()
    {
        ShieldWasJustHit = true;
    }

    private void OnDisable()
    {
        //If the death trigger code hasn't already run,
        //make sure enemy is removed from spawner list
        if (!IsDying && spawner != null)
        {
            spawner.RemoveEnemy(this);
        }
    }

    public void SetFormationTarget(Vector3 target) 
    {
        _formationTarget = target;
        hasFormationTarget = true;
    }

    public void ClearFormationTarget()
    {
        _formationTarget = Vector3.zero;
        hasFormationTarget = false;
    }

    public void SetAttackTarget(Vector3 target)
    {
        _attackTarget = target;
        _hasAttackTarget = true;
    }
}
