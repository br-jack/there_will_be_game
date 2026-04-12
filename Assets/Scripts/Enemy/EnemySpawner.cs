using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public struct Wave
    {
        public float meleeInterval;
        public float rangedInterval;
        public float duration;
        public int maxAlive;
        [Range(0f,1f)] public float meleeSheildProbability;
        [Range(0f,1f)] public float rangedShieldProbability;
    }

    [SerializeField] private GameObject meleeEnemyPrefab;

    [SerializeField] private GameObject rangedEnemyPrefab;


    [SerializeField] private float minDistanceFromPlayer = 15f;

    [SerializeField] private float maxDistanceFromPlayer = 100f;

    private float navMeshSearchRadius = 2.5f;

    [Header("Waves")]

    // Spawner stays on the last wave once the waves have ran out.
    [SerializeField] private Wave[] waves;

    private Transform player;
    private readonly List<StandardEnemyAI> aliveEnemies = new List<StandardEnemyAI>();

    private int currentWaveIndex = 0;
    private float waveTimer = 0f;
    private float meleeTimer = 0f;
    private float rangedTimer = 0f;

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void Update()
    {
        // Prune dead enemies. This is the only cleanup mechanism.
        aliveEnemies.RemoveAll(e => e == null);

        if (player == null || waves == null || waves.Length == 0)
        {
            return;
        }

        Wave currentWave = waves[currentWaveIndex];

        // Advance to the next wave and reset the wave timers.
        waveTimer += Time.deltaTime;
        if (waveTimer >= currentWave.duration && currentWaveIndex < waves.Length - 1)
        {
            currentWaveIndex++;
            waveTimer = 0f;
            meleeTimer = 0f;
            rangedTimer = 0f;
            currentWave = waves[currentWaveIndex];
        }

        // Update timers
        meleeTimer += Time.deltaTime;
        rangedTimer += Time.deltaTime;

        if (meleeTimer >= currentWave.meleeInterval && aliveEnemies.Count < currentWave.maxAlive)
        {
            TrySpawnEnemy(meleeEnemyPrefab, meleeSheildProbability);
            meleeTimer = 0f;
        }

        if (rangedTimer >= currentWave.rangedInterval && aliveEnemies.Count < currentWave.maxAlive)
        {
            TrySpawnEnemy(rangedEnemyPrefab, rangedShieldProbability);
            rangedTimer = 0f;
        }
    }

    private void TrySpawnEnemy(GameObject prefab, float chanceOfShield)
    {
        if (prefab == null)
        {
            return;
        }

        // Uses the navmesh to spawn enemies so tries multiple attempts.
        for (int attempt = 0; attempt < 10; attempt++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float distance = Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);
            Vector3 offset = new Vector3(Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);
            Vector3 candidate = player.position + offset;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, navMeshSearchRadius, NavMesh.AllAreas))
            {
                GameObject spawned = Instantiate(prefab, hit.position, Quaternion.identity);
                StandardEnemyAI ai = spawned.GetComponent<StandardEnemyAI>();
                if (ai != null)
                {
                    aliveEnemies.Add(ai);
                }
                return;
            }
        }
        // Code only gets here if a position can't be found.
    }
}
