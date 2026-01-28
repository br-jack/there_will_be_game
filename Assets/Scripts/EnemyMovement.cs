using System;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float speed;
    
    public Transform playerTransformRef;

    private Rigidbody _rb;
    
    private void Awake()
    {
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        //TODO use a* pathfinding instead
        Vector3 playerPosition = playerTransformRef.position;
        
        Vector3 distanceFromPlayer = playerPosition - transform.position;
        
        _rb.AddForce(distanceFromPlayer.normalized * speed, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
        }
    }
}
