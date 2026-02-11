using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public List<EnemyMovement> aliveEnemies = new List<EnemyMovement>();
    public GameObject enemyPrefab; 
    public float spawnInterval = 0.5f;
    public int maxEnemies = 5;
    public int currentEnemies = 0;
    public enum FormationType
    {
        Grid
    }

    [Header("Formation")]
    public FormationType formationType = FormationType.Grid; 
    // formationRows are based on how many enemies you have and how many columns you allow
    public int formationColumns = 5;
    public Vector2 formationSpacing = new Vector2(3f, 3f);
    public bool centerFormationOnSpawner = true;
    public float attackRingRadius = 2.0f;
    public float attackRingRotationSpeed = 0f;
    public float formationJoinDistance = 8f;
    public float breakFormationDistance = 4f;
    private Vector3 _formationAnchor;
    private float _spawnTimer = 0.0f;
    private float _formationCheckTimer = 0.0f;
    private float _formationCheckInterval = 0.5f;

    private void Start()
    {
        _formationAnchor = transform.position;
    }
    void Update()
    {
        _spawnTimer += Time.deltaTime;
        if (_spawnTimer >= spawnInterval)
        {
            SpawnEnemy();
            _spawnTimer = 0.0f;
        }
        
        _formationCheckTimer += Time.deltaTime;
        if (_formationCheckTimer >= _formationCheckInterval)
        {
            UpdateFormationTargets();
            _formationCheckTimer = 0.0f;
        }
        
        GameObject playerRef = GameObject.FindWithTag("Player");
        if (playerRef != null)
        {
            float moveSpeed = aliveEnemies.Count > 0 ? aliveEnemies[0].speed * 0.5f : 2f;
            Vector3 playerPos = playerRef.transform.position;
            _formationAnchor = Vector3.MoveTowards(
                _formationAnchor,
                playerPos,
                moveSpeed * Time.deltaTime
            );
        }
        UpdateFormationTargets();
    }

    void SpawnEnemy()
    {
        if (currentEnemies < maxEnemies)
        {
            Vector3 spawnPosition = GetFormationPosition(currentEnemies);
            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, transform.rotation);
            EnemyMovement enemyMovement = enemy.GetComponent<EnemyMovement>();
            enemyMovement.spawner = this;
            aliveEnemies.Add(enemyMovement);
            currentEnemies++;
            UpdateFormationTargets();
        }
    }

    public void UpdateFormationTargets()
    {
        GameObject playerRef = GameObject.FindWithTag("Player");
        Vector3 anchorPosition = _formationAnchor;
        
        // Calculate direction to player for formation orientation
        Vector3 directionToPlayer = Vector3.forward;
        if (playerRef != null)
        {
            directionToPlayer = (playerRef.transform.position - anchorPosition).normalized;
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
            
            bool shouldBeInFormation = CanJoinFormation(enemy, playerRef);
            
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
            enemy.SetFormationTarget(anchorPosition + offset);
            formationIndex++;
        }

        if (playerRef == null)
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

        Vector3 playerPosition = playerRef.transform.position;
        float angleStep = 360f / brokenCount;
        float rotationOffset = attackRingRotationSpeed * Time.time;
        int brokenIndex = 0;
        for (int i = 0; i < aliveEnemies.Count; i++)
        {
            EnemyMovement enemy = aliveEnemies[i];
            if (enemy == null || enemy.hasFormationTarget)
            {
                continue;
            }

            float angle = (angleStep * brokenIndex + rotationOffset) * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * attackRingRadius;
            enemy.SetAttackTarget(playerPosition + offset);
            brokenIndex++;
        }
    }

    private bool CanJoinFormation(EnemyMovement enemy, GameObject playerRef)
    {
        if (enemy == null) return false;
        
        // Check if too close to player
        if (playerRef != null)
        {
            float distanceToPlayer = Vector3.Distance(enemy.transform.position, playerRef.transform.position);
            if (distanceToPlayer < breakFormationDistance)
            {
                return false;
            }
        }
        
        // Check if near any other enemy
        bool nearOtherEnemy = false;
        for (int i = 0; i < aliveEnemies.Count; i++)
        {
            EnemyMovement other = aliveEnemies[i];
            if (other == null || other == enemy) continue;
            
            float distance = Vector3.Distance(enemy.transform.position, other.transform.position);
            if (distance <= formationJoinDistance)
            {
                nearOtherEnemy = true;
                break;
            }
        }
        
        return nearOtherEnemy;
    }

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
