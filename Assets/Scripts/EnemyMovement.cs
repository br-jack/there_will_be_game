using System;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float speed;
    public EnemySpawner spawner;
    private Health _playerHealthRef;
    private Transform _playerTransformRef;
    private Rigidbody _rb;
    private Vector3 formationTarget;
    private bool hasFormationTarget;
    
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
        Vector3 targetPosition = hasFormationTarget ? formationTarget : _playerTransformRef.position;
        Vector3 direction = (targetPosition - transform.position).normalized;

        _rb.linearVelocity = new Vector3(direction.x * speed, _rb.linearVelocity.y, direction.z * speed);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _playerHealthRef.LoseLife();
        }
    }

    private void OnDisable()
    {
        if (spawner != null)
        {
            spawner.aliveEnemies.Remove(this);
            spawner.currentEnemies = Mathf.Max(0, spawner.currentEnemies - 1);
            spawner.UpdateFormationTargets();
        }
    }

    public void SetFormationTarget(Vector3 target) 
    {
        formationTarget = target;
        hasFormationTarget = true;
    }
}
