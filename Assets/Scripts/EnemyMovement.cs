using System;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float speed;
    public EnemySpawner spawner;
    private Health _playerHealthRef;
    private Transform _playerTransformRef;
    private Rigidbody _rb;
    private Vector3 _formationTarget;
    private Vector3 _attackTarget;
    public bool hasFormationTarget;
    private bool _hasAttackTarget;
    
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
        Vector3 targetPosition = hasFormationTarget
            ? _formationTarget
            : (_hasAttackTarget ? _attackTarget : _playerTransformRef.position);
        Vector3 direction = (targetPosition - transform.position).normalized;

        _rb.linearVelocity = new Vector3(direction.x * speed * 0.5f, _rb.linearVelocity.y, direction.z * speed * 0.5f);

        // If the enemy is in a formation, break it when it's sufficiently close to the player
        if (hasFormationTarget && spawner != null && Vector3.Distance(_playerTransformRef.position, transform.position) < spawner.breakFormationDistance)
        {
            ClearFormationTarget();
            spawner.UpdateFormationTargets();
        }
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
