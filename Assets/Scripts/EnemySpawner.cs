using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    /**
    Variables explained:
    aliveEnemies: stores enemies currently in play.
        - use aliveEnemies.Length for number of enemies
    spawnInterval: time interval between enemies spawning.
    formationColumns: number of cols in formation (rows decided automatically).
    formationSpacing: spacing between enemies in formation.
    attackRingRadius: how far from players do the enemies stop to attack?
    joinFormationDistance: max distance from formation that an enemy tries to join.
    breakFormationDistance: max distance from player that an enemy leaves the formation.
    formationCheckInterval: checks every player and determines whether it should be solo or in a formation based on distance to a player being greater than breakFormationDistance and distance to a formation greater than joinFormationDistance: this is checked every `formationCheckInterval` seconds.
    */
    public List<EnemyMovement> aliveEnemies = new List<EnemyMovement>();
    public GameObject enemyPrefab; 
    public float spawnInterval = 0.25f;
    public int maxEnemies = 50;
    public enum FormationType
    {
        Grid
    }

    [Header("Formation")]
    public FormationType formationType = FormationType.Grid; 
    public int formationColumns = 5;
    public Vector2 formationSpacing = new Vector2(3.0f, 3.0f); 
    public float attackRingRadius = 2.0f;
    public float joinFormationDistance = 32f;
    public float breakFormationDistance = 2f;
    private Vector3 _formationAnchor;
    private float _spawnTimer = 0.0f;
    private float _formationCheckTimer = 0.0f;
    private float _formationCheckInterval = 0.1f;

    private void Start()
    {
        _formationAnchor = transform.position;
        GameObject _playerRef = GameObject.FindWithTag("Player");
        if (_playerRef != null) _playerTransform = _playerRef.transform;
    }
    void Update()
    {
        // Update timers
        _spawnTimer += Time.deltaTime;
        _formationCheckTimer += Time.deltaTime;

        if (_spawnTimer >= spawnInterval)
        {
            SpawnEnemy();
            _spawnTimer = 0.0f;
        }
        if (_formationCheckTimer >= _formationCheckInterval)
        {
            UpdateFormationTargets();
            _formationCheckTimer = 0.0f;
        }
        
        if (_playerRef != null)
        {
            float moveSpeed = aliveEnemies.Count > 0 ? aliveEnemies[0].formationSpeed : 2f;
            Vector3 playerPos = _playerRef.transform.position;
            _formationAnchor = Vector3.MoveTowards(
                _formationAnchor,
                playerPos,
                moveSpeed * Time.deltaTime
            );
        }
    }

    void SpawnEnemy()
    {
        if (aliveEnemies.Count < maxEnemies)
        {
            Vector3 spawnPosition = GetFormationPosition(aliveEnemies.Count);
            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, transform.rotation);
            EnemyMovement enemyMovement = enemy.GetComponent<EnemyMovement>();
            enemyMovement.spawner = this;
            aliveEnemies.Add(enemyMovement);
            UpdateFormationTargets();
        }
    }

    public void UpdateFormationTargets()
    {
        // Calculate direction to player for formation orientation
        Vector3 directionToPlayer = Vector3.forward;
        if (_playerTransform != null)
        {
            directionToPlayer = (_playerTransform.position - _formationAnchor).normalized;
            if (directionToPlayer.sqrMagnitude < 0.01f)
            {
                directionToPlayer = Vector3.forward;
            }
        }
        
        // Update formation eligibility for each enemy
        for (int i = 0; i < aliveEnemies.Count; i++)
        {
            EnemyMovement enemy = aliveEnemies[i];
            if (enemy == null) continue;
            
            bool shouldBeInFormation = CanJoinFormation(enemy, _playerTransform);
            
            if (shouldBeInFormation && !enemy.hasFormationTarget)
            {
                enemy.hasFormationTarget = true;
            }
            else if (!shouldBeInFormation && enemy.hasFormationTarget)
            {
                enemy.ClearFormationTarget();
            }
        }
        
        // Count formation members
        int formationCount = 0;
        for (int i = 0; i < aliveEnemies.Count; i++)
        {
            EnemyMovement enemy = aliveEnemies[i];
            if (enemy == null || !enemy.hasFormationTarget) continue;
            formationCount++;
        }

        // Assign formation positions
        int formationIndex = 0;
        for (int i=0; i < aliveEnemies.Count; i++)
        {
            EnemyMovement enemy = aliveEnemies[i];
            if (enemy == null || !enemy.hasFormationTarget) continue;

            Vector3 offset = GetGridSpacing(formationIndex, formationCount, directionToPlayer);
            enemy.SetFormationTarget(_formationAnchor + offset);
            formationIndex++;
        }

        if (_playerTransform == null)
        {
            return;
        }

        // Update attack targets for broken formation enemies
        int brokenCount = 0;
        for (int i = 0; i < aliveEnemies.Count; i++)
        {
            EnemyMovement enemy = aliveEnemies[i];
            if (enemy == null || enemy.hasFormationTarget)
            {
                continue;
            }

            brokenCount++;
        }

        if (brokenCount == 0)
        {
            return;
        }

        Vector3 playerPosition = _playerTransform.position;
        float angleStep = 360f / brokenCount;
        int brokenIndex = 0;
        for (int i = 0; i < aliveEnemies.Count; i++)
        {
            EnemyMovement enemy = aliveEnemies[i];
            if (enemy == null || enemy.hasFormationTarget)
            {
                continue;
            }

            float angle = (angleStep * brokenIndex) * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * attackRingRadius;
            enemy.SetAttackTarget(playerPosition + offset);
            brokenIndex++;
        }
    }

    // Returns whether enemy E can join a formation.
    private bool CanJoinFormation(EnemyMovement enemy, Transform playerTransform)
    {
        if (enemy == null) return false;
        
        // E can't join formation if E is too close to the player.
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(enemy.transform.position, playerTransform.position);
            if (distanceToPlayer < breakFormationDistance)
            {
                return false;
            }
        }
        
        // Can only join formation if E is within a threshold distance to other enemies.
        bool nearOtherEnemy = false;
        for (int i = 0; i < aliveEnemies.Count; i++)
        {
            EnemyMovement other = aliveEnemies[i];
            if (other == null || other == enemy) continue;
            
            float distance = Vector3.Distance(enemy.transform.position, other.transform.position);
            if (distance <= joinFormationDistance)
            {
                nearOtherEnemy = true;
                break;
            }
        }
        
        return nearOtherEnemy;
    }

    // Returns an offset that tells the enemy where to position itself in relation to _formationAnchor.
    private Vector3 GetGridSpacing(int index, int totalCount, Vector3 forwardDirection)
    {
        int columns = Mathf.Max(1, formationColumns);
        int rows = Mathf.Max(1, Mathf.CeilToInt(totalCount / (float)columns));

        int col = index % columns;
        int row = Mathf.FloorToInt(index / columns);

        float totalWidth = (columns - 1) * formationSpacing.x;
        float totalDepth = (rows - 1) * formationSpacing.y;

        float x = col * formationSpacing.x - totalWidth * 0.5f;
        float z = row * formationSpacing.y - totalDepth * 0.5f;
        
        // Orient the offset toward the player
        Vector3 right = Vector3.Cross(Vector3.up, forwardDirection).normalized;
        Vector3 forward = forwardDirection;
        
        return right * x + forward * z;
    }

    Vector3 GetFormationPosition(int index)
    {
        return transform.position;
    }

}
