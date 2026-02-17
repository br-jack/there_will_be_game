using System;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float speed;
    public GameObject shield;

    private Health _playerHealthRef;
    private Transform _playerTransformRef;

    private Rigidbody _rb;

    [SerializeField] private float onDeathTimer;
    [SerializeField] private float onKnockbackTimer;
    private float _knockbackTimer;
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
        
        _knockbackTimer = onKnockbackTimer;
    }

    // Update is called once per frame
    private void Update()
    {
        if (IsKnockedBack)
        {
            _knockbackTimer -= Time.deltaTime;
            if (_knockbackTimer <= 0)
            {
                IsKnockedBack = false;
                _knockbackTimer = onKnockbackTimer;
            }
        }
        if (IsDying)
        {
            onDeathTimer -= Time.deltaTime;
            Vector3 rayOrigin = transform.position;
            bool isGrounded = Physics.Raycast(rayOrigin, Vector3.down, deathGroundCheckDistance, groundMask);
            Debug.DrawRay(rayOrigin, Vector3.down * deathGroundCheckDistance, Color.yellow);
            
            if (onDeathTimer <= 0 || (isGrounded == true && IsKnockedBack == false))
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

        _rb.linearVelocity = new Vector3(direction.x * speed, _rb.linearVelocity.y, direction.z * speed);
    }

    public void Die(Collider other)
    {
        float knockbackForce = 20f;
        
        //Stagger enemy
        Vector3 knockbackDirection = transform.position - other.transform.position;
        knockbackDirection.y = 0.5f;
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
}
