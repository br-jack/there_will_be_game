using System;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float speed;
    public GameObject shield;

    private Health _playerHealthRef;
    private Transform _playerTransformRef;

    private Rigidbody _rb;

    private bool isKnockedback;

    private void Awake()
    {
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();

        GameObject playerRef = GameObject.FindWithTag("Player");
        
        _playerTransformRef = playerRef.transform;
        _playerHealthRef = playerRef.GetComponent<Health>();
    }

    // Update is called once per frame
    private void Update()
    {

    }

    private void FixedUpdate()
    {
        if (isKnockedback)
        {
            return;
        }
        //TODO use a* pathfinding instead
        Vector3 playerPosition = _playerTransformRef.position;
        
        Vector3 direction = (playerPosition - transform.position).normalized;

        // Enemy actually needs to face the player
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);
        }

        _rb.linearVelocity = new Vector3(direction.x * speed, _rb.linearVelocity.y, direction.z * speed);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (HasShield())
            {
                _playerHealthRef.LoseLife();
            }
            else
            {
                Die();
            }
        }

        if (other.gameObject.CompareTag("Arena") && isKnockedback)
        {
            isKnockedback = false;
        }
    }

    public void Die()
    {
        Debug.Log("Enemy died!");
        Destroy(gameObject);
    }

    public void ApplyKnockback(Vector3 force)
    {
        isKnockedback = true;

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

    public void HitBody()
    {
        if (shield != null) return;

        Die();
    }
}
