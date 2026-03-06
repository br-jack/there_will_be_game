using UnityEngine;
using System.Collections.Generic;
using TMPro;

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
    private Transform _playerTransformRef;

    [Header("Formation")]
    public FormationType formationType = FormationType.Grid; 
    public int formationColumns = 5;
    public Vector2 formationSpacing = new Vector2(3.0f, 3.0f); 
    public float attackRingRadius = 2.0f;
    public float joinFormationDistance = 32f;
    public float breakFormationDistance = 2f;
    public float anchorStandoffBuffer = 1.5f;

    [Header("Spawn")]
    public float spawnJitter = 0.75f;
    public float spawnRadius = 0.5f;

    private Vector3 _formationAnchor;
    private float _spawnTimer = 0.0f;
    private float _formationCheckTimer = 0.0f;
    private float _formationCheckInterval = 0.1f;
    public enum FormationType { Grid }

    private int _killCount = 0;
    [SerializeField] private TMP_Text killCounterText;
    public enum FormationType
    {
        Grid
    }
    private void Start()
    {
        _formationAnchor = transform.position;
        GameObject _playerRef = GameObject.FindWithTag("Player");
        if (_playerRef != null) _playerTransformRef = _playerRef.transform;
    }
    void Update()
    {
        // Update and react to timers.
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
        
        if (_playerTransformRef != null)
        {
            /* Anchor speed is set to the formation speed of the enemies in the formation.
            If there are no enemies in the formation, use default value. */
            float moveSpeed = aliveEnemies.Count > 0 ? aliveEnemies[0].formationSpeed : 2f;
            float formationSpeed = 2f;
            if (aliveEnemies.Count > 0)
            {
                formationSpeed = aliveEnemies[0].formationSpeed;
            }

            Vector3 anchorToPlayer = _playerTransformRef.position - _formationAnchor;

            float stopRadius = GetAnchorStopRadius();

            // Check whether the formation anchor is close enough to the player already before moving it.
            if (anchorToPlayer.magnitude < stopRadius)
            {
                // If the anchor position is less than a threshold distance to the player, don't move it
            }
            else
            {
                /* The anchor is further away from the player than the threshold.
                Move it by displacement = speed * time,
                BUT make sure the movement doesn't move the anchor any closer to the player than stopping distance */
                float displacement = Mathf.Min(formationSpeed * Time.deltaTime, anchorToPlayer.magnitude - stopRadius);
                _formationAnchor = _formationAnchor + anchorToPlayer.normalized * displacement;
            }

        }
    }

    void SpawnEnemy()
    {

        if (aliveEnemies.Count >= maxEnemies) return;

        Vector3 spawnPosition = GetSpawnPosition(aliveEnemies.Count);

        // Set up the enemy: link it to the spawner, add it to the list of alive enemies, re-calculate the formation.
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, transform.rotation);
        EnemyMovement enemyMovement = enemy.GetComponent<EnemyMovement>();
        if (enemyMovement == null) return;
        enemyMovement.spawner = this;
        aliveEnemies.Add(enemyMovement);
        UpdateFormationTargets();

    }

    public void RemoveEnemy(EnemyMovement enemy)
    {
        aliveEnemies.Remove(enemy);
        UpdateFormationTargets();
        _killCount++;

        killCounterText.text = $"Kill Count: {_killCount}";
    }

    public void UpdateFormationTargets()
    {
        void SpawnEnemy()
    {
        if (aliveEnemies.Count >= maxEnemies) return;

        Vector3 spawnPosition = GetSpawnPosition(aliveEnemies.Count);

        // Set up the enemy: link it to the spawner, add it to the list of alive enemies, re-calculate the formation
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, transform.rotation);
        EnemyMovement enemyMovement = enemy.GetComponent<EnemyMovement>();
        if (enemyMovement == null) return;
        enemyMovement.spawner = this;
        aliveEnemies.Add(enemyMovement);
        UpdateFormationTargets();
    }

    public void UpdateFormationTargets()
    {
        // Calculating directionToPlayer
        Vector3 directionToPlayer = Vector3.forward;

        if (_playerTransformRef != null)
        {
            Vector3 formationToPlayer = _playerTransformRef.position - _formationAnchor;

            if (formationToPlayer.sqrMagnitude < 0.01f) formationToPlayer = Vector3.forward;
            // Error mitigation: if distance of anchorToPlayer ~ 0 then use a fallback
            directionToPlayer = formationToPlayer.normalized;
        }

        if (aliveEnemies.Count == 0) return;

        var (cols, rows) = GetFormationDimensions(aliveEnemies.Count);

        // Give each enemy E in the formation their formation target
        int formationIndex = 0;
        for (int i = 0; i < aliveEnemies.Count; i++)
        {
            EnemyMovement E = aliveEnemies[i];
            if (E == null) continue;

            Vector3 offset = GetGridSpacing(formationIndex, aliveEnemies.Count, directionToPlayer, cols, rows);
            E.SetFormationTarget(_formationAnchor + offset, directionToPlayer);
            formationIndex++;
        }
    }


    /**
    Helper function for UpdateFormationTargets(): Returns whether enemy E can join a formation.
    Requirements for joining formation: E must be far enough from the player and close enough to another enemy. */
    private bool CanJoinFormation(EnemyMovement E)
    {
        if (E == null) return false;

        // E can't join formation if E is too close to the player.
        if (_playerTransformRef != null)
        {
            float distanceToPlayer = Vector3.Distance(E.transform.position, _playerTransformRef.position);
            if (distanceToPlayer < breakFormationDistance) return false;
        }

        // Can only join formation if E is within a threshold distance to other enemies.
        for (int i = 0; i < aliveEnemies.Count; i++)
        {
            EnemyMovement otherEnemy = aliveEnemies[i];
            if (otherEnemy == null || otherEnemy == E) continue;
            float distanceToEnemy = Vector3.Distance(E.transform.position, otherEnemy.transform.position);
            if (distanceToEnemy <= joinFormationDistance)
            {
                return true;
            }
        }

        return false;

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

    // A placeholder function for now for adding more advanced spawning functions.
    Vector3 GetSpawnPosition(int index)
    {
        return transform.position;
    }

    // Triangle version
    // private Vector3 GetTriangleSpacing(int index, int totalCount, Vector3 forwardDirection)
    // {
    //     // Get total number of rows
    //     // Get index's row number
    //     // Get amount of people in that index
    //     int[] rowCapacities = [1, 3, 5, 7, 9, 11]
    //     int i = totalCount;
    //     int totalRows = 0;
    //     int[] rows; 
    //     while (i > 0)
    //     {
    //         i -= rowCapacities[totalRows];
    //         totalRows++;
    //         if (i >= 0)
    //         {
    //             rows[i] = rowCapacities[i];
    //         }
    //         else
    //         {
    //             break;
    //         }
    //     }
    //     int rowIndex = index;
    //     while (i > 0)
    //     {
    //         rowIndex -= rows[i];
    //     }
    //     // rowIndex (index of row); rows (total structure); rows.Length is no. of rows
    //     int totalDepth = rows.Length * formationSpacing.y;
    //     int rowWidth = rows[rows.Length - 1] * formationSpacing.y;
    // }
}


