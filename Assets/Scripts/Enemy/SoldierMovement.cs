using System;
using UnityEngine;

public class SoldierMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [HideInInspector] public PlayerHealth _playerHealthRef;
    [HideInInspector] public Transform _playerTransformRef;
    private Rigidbody _rb;
    [SerializeField] private int collisionDamage = 10;
    
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();

        GameObject playerRef = GameObject.FindWithTag("Player");
        if (playerRef != null)
        {
            _playerTransformRef = playerRef.transform;
            _playerHealthRef = playerRef.GetComponent<PlayerHealth>();
        }
    }

    private void FixedUpdate()
    {
        if (_rb == null) return;
        if (_playerTransformRef == null) return;

        // TODO use A* pathfinding instead
        Vector3 playerPosition = _playerTransformRef.position;
        Vector3 direction = (playerPosition - transform.position).normalized;
        _rb.linearVelocity = new Vector3(direction.x * speed, _rb.linearVelocity.y, direction.z * speed);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (_playerHealthRef != null)
            {
                _playerHealthRef.TakeDamage(collisionDamage);
            }
        }
    }
}

