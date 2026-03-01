using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    /**
    aliveEnemies: stores enemies currently in play.
    spawnInterval: time interval between enemies spawning.
    formationColumns: max cols in formation (rows decided automatically).
    formationSpacing: spacing between enemies in formation.
    attackRingRadius: how far from players do the enemies stop to attack?
    joinFormationDistance: max distance from formation that an enemy tries to join.
    breakFormationDistance: max distance from player that an enemy leaves the formation.
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

    private (int cols, int rows) GetFormationDimensions(int totalCount)
    {
        // If there are fewer enemies than max columns, return enemies. Otherwise, return max columns.
        int cols = (totalCount < formationColumns) ? totalCount : formationColumns;

        // Rows are automatically decided based on {enemy count, columns}
        int rows = Mathf.Max(1, Mathf.CeilToInt(totalCount / (float)cols));

        return (cols, rows);
    }

    private Vector3 GetGridSpacing(int index, int totalCount, Vector3 forwardDirection, int cols, int rows)
    {
        // Calculate (column, row) of the current index
        int C = index % cols;
        int R = Mathf.FloorToInt(index / cols);

        // Calculate the centre of the grid (by removing half the total width/length)
        float centreX = C * formationSpacing.x - (cols - 1) * formationSpacing.x * 0.5f;
        float centreZ = R * formationSpacing.y - (rows - 1) * formationSpacing.y * 0.5f;

        // Project the grid offset into world space using the formation's right and forward axes.
        Vector3 right = Vector3.Cross(Vector3.up, forwardDirection).normalized;
        Vector3 forward = forwardDirection;
        return right * centreX + forward * centreZ;
    }

    private Vector3 GetSpawnPosition(int index)
    {
        Vector2 noise = Random.insideUnitCircle * spawnJitter;

        // anchorToPlayer represents the forward direction of the formation.
        // If anchorToPlayer is not available or nonsense, use fallback.
        Vector3 anchorToPlayer = transform.forward;
        if (_playerTransformRef != null) anchorToPlayer = _playerTransformRef.position - _formationAnchor;
        if (anchorToPlayer.sqrMagnitude < 0.001f) anchorToPlayer = Vector3.forward;

        var (cols, rows) = GetFormationDimensions(Mathf.Max(index + 1, formationColumns));
        Vector3 offset = GetGridSpacing(index, Mathf.Max(index + 1, cols * rows), anchorToPlayer, cols, rows);

        Vector3 spawn = _formationAnchor + offset + new Vector3(noise.x, 0f, noise.y);
        spawn.y = transform.position.y;
        return spawn;
    }

    private float GetAnchorStopRadius()
    {
        var (cols, rows) = GetFormationDimensions(aliveEnemies.Count);
        float depth = (rows - 1) * formationSpacing.y;

        // base distance that enemies should stay away from player + 1/2 the formation depth + small extra offset
        return attackRingRadius + depth * 0.5f + anchorStandoffBuffer;
    }
}
