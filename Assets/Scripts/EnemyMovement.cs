using System;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float speed;
    
    private Health _playerHealthRef;
    private Transform _playerTransformRef;

    private Rigidbody _rb;
    
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
        //TODO use a* pathfinding instead
        Vector3 playerPosition = _playerTransformRef.position;
        
        Vector3 direction = (playerPosition - transform.position).normalized;
        
        _rb.linearVelocity = new Vector3(direction.x * speed, _rb.linearVelocity.y, direction.z * speed);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _playerHealthRef.LoseLife();
        }
    }
}
