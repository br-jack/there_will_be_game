using System;
using UnityEngine;
using UnityEngine.Serialization;

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
    public float speed;
    public GameObject shield;

    private Health _playerHealthRef;
    private Transform _playerTransformRef;
    private Rigidbody _rb;

    public bool isKnockedback;
    public bool shieldWasJustHit = false;
    private Vector3 _formationTarget;
    private Vector3 _attackTarget;
    public bool hasFormationTarget;
    private bool _hasAttackTarget;
    [SerializeField] private float onDeathTimer;
    [SerializeField] private float knockbackTime;
    [SerializeField] private float currentKnockbackTimer;
    [SerializeField] private float deathGroundCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundMask;

    public bool IsDying { get; private set; }

    public bool IsKnockedBack { get; private set; }
    public bool ShieldWasJustHit { get; private set; }

    private void Awake()
    {

    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        GameObject playerRef = GameObject.FindWithTag("Player");

        _playerTransformRef = playerRef.transform;
        _playerHealthRef = playerRef.GetComponent<Health>();

        // Default values (if none set)
        if (defaultSpeed <= 0f) defaultSpeed = 3f;
        if (formationSpeed <= 0f) formationSpeed = 2/3 * defaultSpeed;
        
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

        
            if (isGrounded && currentKnockbackTimer <= 0)
            {
                IsKnockedBack = false;
                currentKnockbackTimer = knockbackTime;
            }
        }
        if (IsDying)
        {
            onDeathTimer -= Time.deltaTime;
            
            if (onDeathTimer <= 0 || IsKnockedBack == false)
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

    public void Die(Collider other)
    {
        //Grey out enemy to signify that its dead
        gameObject.GetComponent<Renderer>().material.color = Color.gray;

        float knockbackForce = 40f;
        
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
        if (spawner != null)
        {
            spawner.aliveEnemies.Remove(this);
            
            spawner.UpdateFormationTargets();
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
