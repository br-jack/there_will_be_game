/**
Soldier.
- Responsible for moving towards the target (set by SquadSpawner).
- Responding to squad state (e.g. in formation vs out of formation).
- Telling the SquadSpawner when it has died. */

using System;
using UnityEngine;

public class Soldier : MonoBehaviour
{
    /**
    Variables explained:
    defaultSpeed: speed E travels at when OUT of formation
    formationSpeed: speed E travels at when IN formation
    spawner: every E is connected to a spawner
    _formationTarget: when IN formation, the position that E should move towards (in formation grid)
    _attackTarget: when NOT in formation, the position that E should move towards
    */
    public float baseSpeed = 3f; // default speed 
    private float _formationSpeedMultiplier = 0.6f;
    public SquadController squad;
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

    private Vector3 ComputeTargetHelper()
    {
        /**
        Goal: decide which target to move towards.
        Logic:
        - IF in formation, go to formation & decide to alter base speed if in formation.
        - ELSE IF has attack target, go to attack target.
        - ELSE go to player.*/
        if (hasFormationTarget)
        {
            return _formationTarget;
        }
        else if (_hasAttackTarget)
        {
            return _attackTarget
        }
        else
        {
            return _playerTransformRef.position;
        }
    }

    private void MoveSoldierHelper(Vector3 targetPosition, float speed)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        _rb.linearVelocity = new Vector3(direction.x * speed, _rb.linearVelocity.y, direction.z * speed);
    }

    private void FixedUpdate()
    {
        float speed = baseSpeed;

        Vector3 targetPosition = ComputeTargetHelper();

        MoveSoldierHelper(targetPosition, speed);

        // If the enemy is in a formation, break it when it's sufficiently close to the player
        if (hasFormationTarget && spawner != null && distanceToPlayer < spawner.breakFormationDistance)
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