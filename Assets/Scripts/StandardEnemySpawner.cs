using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[Serializable]
public struct EnemySpawnEntry
{
    [Tooltip("Enemy prefab to spawn.")]
    public GameObject prefab;
    [Tooltip("Relative weight — higher means more likely to be picked.")]
    public int weight;
}

[Serializable]
public struct WaveDefinition
{
    [Tooltip("Enemy types and weights for this wave.")]
    public EnemySpawnEntry[] enemies;
    [Tooltip("Total number of enemies to spawn this wave.")]
    public int count;
}

public class StandardEnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [Tooltip("Used when NOT in wave mode. Weighted random selection from this list.")]
    [SerializeField] private EnemySpawnEntry[] enemyTypes;

    [HideInInspector] public List<StandardEnemyAI> aliveEnemies = new List<StandardEnemyAI>();

    [Header("Spawn Points")]
    [Tooltip("Drag in empty GameObjects to mark where enemies can spawn. If none are set, enemies spawn around this object's position.")]
    [SerializeField] private Transform[] spawnPoints;
    [Tooltip("Random offset around each spawn point.")]
    [SerializeField] private float spawnRadius = 2f;

    [Header("Directional Spawning")]
    [Tooltip("Bias spawning ahead of the player's movement direction.")]
    [SerializeField] private bool directionalSpawn = true;
    [Tooltip("How far ahead of the player to place the spawn center. Scales with player speed.")]
    [SerializeField] private float aheadDistanceMultiplier = 2f;
    [Tooltip("Base distance ahead even when the player is slow or still.")]
    [SerializeField] private float aheadBaseDistance = 10f;
    [Tooltip("How wide the forward spawn arc is (in degrees from center). 90 = full semicircle ahead.")]
    [SerializeField] private float aheadArcHalfAngle = 70f;
    [Tooltip("Player speed below this uses fallback (random around spawn points / spawner).")]
    [SerializeField] private float directionalSpeedThreshold = 2f;

    [Header("Spawn Clustering")]
    [Tooltip("Spawn enemies in tight squads instead of individually.")]
    [SerializeField] private bool useClustering = true;
    [Tooltip("Number of enemies per cluster.")]
    [SerializeField] private int clusterSize = 3;
    [Tooltip("How tightly packed the cluster is. Enemies spawn within this radius of the cluster center.")]
    [SerializeField] private float clusterRadius = 3f;

    [Header("Player Distance")]
    [Tooltip("Enemies won't spawn closer than this distance to the player.")]
    [SerializeField] private float minPlayerDistance = 15f;
    [Tooltip("Enemies won't spawn farther than this distance from the player.")]
    [SerializeField] private float maxPlayerDistance = 60f;

    [Header("Off-Screen Spawning")]
    [Tooltip("Only spawn enemies outside the camera's view.")]
    [SerializeField] private bool requireOffScreen = true;
    [Tooltip("Extra padding beyond the screen edge (in viewport units). 0.05 = 5% past the edge.")]
    [SerializeField] private float offScreenPadding = 0.05f;

    [Header("NavMesh Validation")]
    [Tooltip("Snap spawn positions to the nearest NavMesh point.")]
    [SerializeField] private bool requireNavMesh = true;
    [Tooltip("Max distance to search for a valid NavMesh point from the raw spawn position.")]
    [SerializeField] private float navMeshSearchRadius = 5f;

    [Header("Initial Spawn")]
    [Tooltip("How many enemies to spawn at the start of the game.")]
    [SerializeField] private int initialCount = 5;

    [Header("Continuous Spawning")]
    [Tooltip("Keep spawning enemies over time.")]
    [SerializeField] private bool continuousSpawn = true;
    [Tooltip("Seconds between each spawn.")]
    [SerializeField] private float spawnInterval = 4f;
    [Tooltip("How many enemies spawn each interval.")]
    [SerializeField] private int spawnBatchSize = 1;
    [Tooltip("Max enemies alive at once. 0 = unlimited.")]
    [SerializeField] private int maxAlive = 20;

    [Header("Waves (optional)")]
    [Tooltip("If enabled, spawns in waves with per-wave enemy composition. Overrides continuous spawning and enemyTypes.")]
    [SerializeField] private bool useWaves = false;
    [Tooltip("Define each wave's enemy composition and count.")]
    [SerializeField] private WaveDefinition[] waves;
    [Tooltip("Seconds to wait after a wave is cleared before the next one spawns.")]
    [SerializeField] private float timeBetweenWaves = 3f;

    /// <summary>Fired when a wave is fully cleared. Parameter is the wave index (0-based) that was just completed.</summary>
    public event Action<int> OnWaveCleared;
    /// <summary>Fired when all waves in the list have been completed (before looping).</summary>
    public event Action OnAllWavesCleared;

    public int CurrentWave => currentWave;
    public int AliveCount => aliveEnemies.Count;

    private float spawnTimer;
    private int currentWave;
    private bool waitingForNextWave;
    private float waveTimer;
    private bool waveClearedFired;
    private Transform playerTransform;
    private HorseMovement playerMovement;
    private Camera mainCam;
    private const int MaxSpawnAttempts = 10;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerMovement = player.GetComponent<HorseMovement>();
        }

        mainCam = Camera.main;

        if (useWaves)
        {
            if (waves == null || waves.Length == 0)
            {
                Debug.LogError("StandardEnemySpawner: useWaves is on but no waves defined!", this);
                return;
            }
            SpawnWave();
        }
        else
        {
            if (enemyTypes == null || enemyTypes.Length == 0)
            {
                Debug.LogError("StandardEnemySpawner: No enemy types assigned!", this);
                return;
            }
            SpawnBatch(initialCount, enemyTypes);
        }
    }

    private void Update()
    {
        if (useWaves)
        {
            UpdateWaves();
        }
        else if (continuousSpawn)
        {
            UpdateContinuous();
        }
    }

    private void UpdateContinuous()
    {
        if (maxAlive > 0 && aliveEnemies.Count >= maxAlive) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            int toSpawn = spawnBatchSize;
            if (maxAlive > 0)
            {
                toSpawn = Mathf.Min(toSpawn, maxAlive - aliveEnemies.Count);
            }
            SpawnBatch(toSpawn, enemyTypes);
        }
    }

    private void UpdateWaves()
    {
        if (!waitingForNextWave) return;
        if (aliveEnemies.Count > 0)
        {
            waveClearedFired = false;
            return;
        }

        if (!waveClearedFired)
        {
            waveClearedFired = true;
            OnWaveCleared?.Invoke(currentWave);

            if (waves != null && currentWave >= waves.Length - 1)
            {
                OnAllWavesCleared?.Invoke();
            }
        }

        waveTimer += Time.deltaTime;
        if (waveTimer >= timeBetweenWaves)
        {
            waitingForNextWave = false;
            waveTimer = 0f;
            currentWave++;
            SpawnWave();
        }
    }

    private void SpawnWave()
    {
        if (waves == null || waves.Length == 0) return;

        int waveIndex = currentWave % waves.Length;
        WaveDefinition wave = waves[waveIndex];

        if (wave.enemies == null || wave.enemies.Length == 0)
        {
            Debug.LogWarning($"StandardEnemySpawner: Wave {waveIndex} has no enemies defined.", this);
            return;
        }

        SpawnBatch(wave.count, wave.enemies);
        waitingForNextWave = true;
        waveClearedFired = false;
    }

    private void SpawnBatch(int count, EnemySpawnEntry[] entries)
    {
        if (entries == null || entries.Length == 0) return;

        if (useClustering && clusterSize > 1)
        {
            SpawnClustered(count, entries);
        }
        else
        {
            SpawnIndividual(count, entries);
        }
    }

    private void SpawnIndividual(int count, EnemySpawnEntry[] entries)
    {
        for (int i = 0; i < count; i++)
        {
            if (!TryGetValidSpawnPosition(out Vector3 pos)) continue;
            SpawnEnemyAt(pos, entries);
        }
    }

    private void SpawnClustered(int count, EnemySpawnEntry[] entries)
    {
        int spawned = 0;
        while (spawned < count)
        {
            // Find a valid center for this cluster
            if (!TryGetValidSpawnPosition(out Vector3 clusterCenter)) continue;

            int thisCluster = Mathf.Min(clusterSize, count - spawned);

            for (int i = 0; i < thisCluster; i++)
            {
                // Offset each enemy within the cluster radius
                Vector2 offset = UnityEngine.Random.insideUnitCircle * clusterRadius;
                Vector3 memberPos = clusterCenter + new Vector3(offset.x, 0f, offset.y);

                // Snap cluster members to NavMesh individually — skip if no valid spot
                if (requireNavMesh)
                {
                    if (!NavMesh.SamplePosition(memberPos, out NavMeshHit hit, navMeshSearchRadius, NavMesh.AllAreas))
                    {
                        spawned++;
                        continue;
                    }
                    memberPos = hit.position;
                }

                SpawnEnemyAt(memberPos, entries);
                spawned++;
            }
        }
    }

    private void SpawnEnemyAt(Vector3 pos, EnemySpawnEntry[] entries)
    {
        GameObject prefab = PickWeightedPrefab(entries);
        if (prefab == null) return;

        // Final NavMesh safety — never spawn off the NavMesh
        if (requireNavMesh)
        {
            if (!NavMesh.SamplePosition(pos, out NavMeshHit hit, navMeshSearchRadius, NavMesh.AllAreas)) return;
            pos = hit.position;
        }

        GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);
        StandardEnemyAI ai = enemy.GetComponent<StandardEnemyAI>();
        if (ai == null) return;

        ai.Initialize(this);
        aliveEnemies.Add(ai);
    }

    private GameObject PickWeightedPrefab(EnemySpawnEntry[] entries)
    {
        int totalWeight = 0;
        for (int i = 0; i < entries.Length; i++)
        {
            totalWeight += Mathf.Max(1, entries[i].weight);
        }

        int roll = UnityEngine.Random.Range(0, totalWeight);
        int cumulative = 0;
        for (int i = 0; i < entries.Length; i++)
        {
            cumulative += Mathf.Max(1, entries[i].weight);
            if (roll < cumulative) return entries[i].prefab;
        }

        return entries[entries.Length - 1].prefab;
    }

    private bool TryGetValidSpawnPosition(out Vector3 result)
    {
        for (int attempt = 0; attempt < MaxSpawnAttempts; attempt++)
        {
            Vector3 candidate = GetRawSpawnPosition();

            // Player distance check
            if (playerTransform != null)
            {
                float dist = Vector3.Distance(candidate, playerTransform.position);
                if (dist < minPlayerDistance || dist > maxPlayerDistance) continue;
            }

            // Off-screen check
            if (requireOffScreen && !IsOffScreen(candidate)) continue;

            // NavMesh snap
            if (requireNavMesh)
            {
                if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, navMeshSearchRadius, NavMesh.AllAreas)) continue;
                candidate = hit.position;
            }

            result = candidate;
            return true;
        }

        // All attempts failed — fall back to raw position with NavMesh snap only
        Vector3 fallback = GetRawSpawnPosition();
        if (requireNavMesh && NavMesh.SamplePosition(fallback, out NavMeshHit fallbackHit, navMeshSearchRadius, NavMesh.AllAreas))
        {
            fallback = fallbackHit.position;
        }
        result = fallback;
        return true;
    }

    private Vector3 GetRawSpawnPosition()
    {
        // If directional spawning is active and the player is moving, spawn ahead of them
        if (directionalSpawn && playerTransform != null && playerMovement != null
            && playerMovement.CurrentSpeed >= directionalSpeedThreshold)
        {
            return GetDirectionalPosition();
        }

        return GetStaticSpawnPosition();
    }

    private Vector3 GetDirectionalPosition()
    {
        Vector3 playerPos = playerTransform.position;
        Vector3 playerForward = playerTransform.forward;
        float speed = playerMovement.CurrentSpeed;

        // Distance ahead scales with player speed
        float aheadDist = aheadBaseDistance + speed * aheadDistanceMultiplier;

        // Pick a random angle within the forward arc
        float halfArc = aheadArcHalfAngle;
        float angle = UnityEngine.Random.Range(-halfArc, halfArc);
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
        Vector3 spawnDir = rotation * playerForward;

        // Random distance between min and the ahead distance (clamped to max player distance)
        float dist = UnityEngine.Random.Range(minPlayerDistance, Mathf.Min(aheadDist, maxPlayerDistance));

        Vector3 pos = playerPos + spawnDir * dist;
        pos.y = playerPos.y;
        return pos;
    }

    private Vector3 GetStaticSpawnPosition()
    {
        Vector3 center;
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform point = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
            center = point != null ? point.position : transform.position;
        }
        else
        {
            center = transform.position;
        }

        Vector2 offset = UnityEngine.Random.insideUnitCircle * spawnRadius;
        return new Vector3(center.x + offset.x, center.y, center.z + offset.y);
    }

    private bool IsOffScreen(Vector3 worldPos)
    {
        if (mainCam == null) return true;

        Vector3 vp = mainCam.WorldToViewportPoint(worldPos);

        // Behind the camera
        if (vp.z < 0f) return true;

        float min = -offScreenPadding;
        float max = 1f + offScreenPadding;
        return vp.x < min || vp.x > max || vp.y < min || vp.y > max;
    }

    public void RemoveEnemy(StandardEnemyAI enemy)
    {
        aliveEnemies.Remove(enemy);
    }

    private void OnDrawGizmosSelected()
    {
        // Spawn radii
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.4f);
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            foreach (Transform point in spawnPoints)
            {
                if (point == null) continue;
                Gizmos.DrawWireSphere(point.position, spawnRadius);
            }
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }

        // Player distance rings
        Gizmos.color = new Color(0.3f, 1f, 0.3f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, minPlayerDistance);
        Gizmos.color = new Color(0.3f, 0.3f, 1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, maxPlayerDistance);

        // Cluster radius preview
        if (useClustering)
        {
            Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, clusterRadius);
        }
    }
}
