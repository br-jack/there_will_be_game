using System.Collections.Generic;
using Enemy;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    private enum EnemyType { MeleeShielded, MeleeUnshielded, Ranged, Rapid, Civilian }

    [System.Serializable]
    public struct Wave
    {
        public float duration;
        public bool clearRemainingOnEnd;
        public float spawnInterval;
        public int meleeShielded;
        public int meleeUnshielded;
        public int ranged;
        public int rapid;
        public int civilians;
    }

    private const float BreakDuration = 5f;

    // Toggled by GameStateManager so spawning halts on pause / game over.
    [HideInInspector] public bool spawningEnabled = true;

    [Header("all Enemy Prefabs")]
    [SerializeField] private GameObject meleeUnshieldedEnemyPrefab;
    [SerializeField] private GameObject meleeShieldedEnemyPrefab;
    [SerializeField] private GameObject rapidEnemyPrefab;
    [SerializeField] private GameObject rangedEnemyPrefab;
    [SerializeField] private GameObject civilianPrefab; // if you make a new enemy, integrate it with the spawner here

    [SerializeField] private float minDistanceFromPlayer = 15f;
    [SerializeField] private float maxDistanceFromPlayer = 100f;
    private float navMeshSearchRadius = 2.5f;

    private float mapMinX, mapMaxX, mapMinZ, mapMaxZ;

    [Header("waves")]
    [SerializeField] private Wave[] waves; // keep in mind the spawner stays on the last wave forever but this won't matter if we end up capping the game time to 5 mins

    public event System.Action<int> OnWaveStarted;

    private Transform player;

    // these keep track of the currently active enemies of each type
    private readonly List<StandardEnemyAI> aliveMeleeShielded = new List<StandardEnemyAI>();
    private readonly List<StandardEnemyAI> aliveMeleeUnshielded = new List<StandardEnemyAI>();
    private readonly List<StandardEnemyAI> aliveRanged = new List<StandardEnemyAI>();
    private readonly List<StandardEnemyAI> aliveRapid = new List<StandardEnemyAI>();
    private readonly List<CivilianAI> aliveCivilians = new List<CivilianAI>();

    private int currentWaveIndex = 0;
    private float waveTimer = 0f;
    private float breakTimer = 0f;
    private bool onBreak = false;
    private float spawnTimer = 0f;

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        ComputeMapBounds();
        OnWaveStarted?.Invoke(1);
    }

    private void ComputeMapBounds()
    {
        mapMinX = -1000f; // it's actually closer to 800x800 but always keep this slightly larger than the map to be safe
        mapMaxX = 1000f;
        mapMinZ = -1000f;
        mapMaxZ = 1000f;
    }

    private void Update()
    {
        // remove all dead enemies immediately from the active lists
        aliveMeleeShielded.RemoveAll(e => e == null);
        aliveMeleeUnshielded.RemoveAll(e => e == null);
        aliveRanged.RemoveAll(e => e == null);
        aliveRapid.RemoveAll(e => e == null);
        aliveCivilians.RemoveAll(c => c == null);

        // when its paused (usually for hammer calibration) or game over, pause the waves
        if (!spawningEnabled) return;

        if (player == null || waves == null || waves.Length == 0)
        {
            return;
        }

        // Rest period between waves — no spawning.
        if (onBreak)
        {
            breakTimer += Time.deltaTime;
            if (breakTimer >= BreakDuration)
            {
                onBreak = false;
                currentWaveIndex++;
                waveTimer = 0f;
                spawnTimer = 0f;
                OnWaveStarted?.Invoke(currentWaveIndex + 1);
            }
            return;
        }

        Wave currentWave = waves[currentWaveIndex];

        waveTimer += Time.deltaTime;
        if (waveTimer >= currentWave.duration && currentWaveIndex < waves.Length - 1)
        {
            if (currentWave.clearRemainingOnEnd)
            {
                ClearList(aliveMeleeShielded);
                ClearList(aliveMeleeUnshielded);
                ClearList(aliveRanged);
                ClearList(aliveRapid);
                ClearList(aliveCivilians);
            }
            onBreak = true;
            breakTimer = 0f;
            return;
        }

        // Spawn on interval.
        if (currentWave.spawnInterval <= 0f) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= currentWave.spawnInterval)
        {
            spawnTimer = 0f;
            TrySpawnWeighted(currentWave);
        }
    }

    private void TrySpawnWeighted(Wave wave)
    {
        // calculate remaining enemies that we can possibly have left to spawn for each type before max cap is reached
        int remainMeleeShielded = Mathf.Max(0, wave.meleeShielded - aliveMeleeShielded.Count);
        int remainMeleeUnshielded = Mathf.Max(0, wave.meleeUnshielded - aliveMeleeUnshielded.Count);
        int remainRanged = Mathf.Max(0, wave.ranged - aliveRanged.Count);
        int remainRapid = Mathf.Max(0, wave.rapid - aliveRapid.Count);
        int remainCivilians = Mathf.Max(0, wave.civilians - aliveCivilians.Count);

        int total = remainMeleeShielded + remainMeleeUnshielded + remainRanged + remainRapid + remainCivilians;
        if (total <= 0) return;

        int roll = Random.Range(0, total);

        if (roll < remainMeleeShielded)
        {
            SpawnOne(EnemyType.MeleeShielded);
        }
        else if (roll < remainMeleeShielded + remainMeleeUnshielded)
        {
            SpawnOne(EnemyType.MeleeUnshielded);
        }
        else if (roll < remainMeleeShielded + remainMeleeUnshielded + remainRanged)
        {
            SpawnOne(EnemyType.Ranged);
        }
        else if (roll < remainMeleeShielded + remainMeleeUnshielded + remainRanged + remainRapid)
        {
            SpawnOne(EnemyType.Rapid);
        }
        else
        {
            SpawnOne(EnemyType.Civilian);
        }
        // if we add more to spawner, add them here
    }

    private void SpawnOne(EnemyType type)
    {
        GameObject prefab;

        switch (type)
        {
            case EnemyType.MeleeShielded:
                prefab = meleeShieldedEnemyPrefab;
                break;
            case EnemyType.MeleeUnshielded:
                prefab = meleeUnshieldedEnemyPrefab;
                break;
            case EnemyType.Ranged:
                prefab = rangedEnemyPrefab;
                break;
            case EnemyType.Rapid:
                prefab = rapidEnemyPrefab;
                break;
            case EnemyType.Civilian:
                prefab = civilianPrefab;
                break;
            default:
                return;
        }

        if (prefab == null) return;

        // Try to find a valid NavMesh position near the player.
        for (int attempt = 0; attempt < 50; attempt++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float distance = Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);
            Vector3 offset = new Vector3(Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);
            Vector3 candidate = player.position + offset;

            if ((candidate.x < mapMinX || candidate.x > mapMaxX || candidate.z < mapMinZ || candidate.z > mapMaxZ)) continue;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, navMeshSearchRadius, NavMesh.AllAreas))
            {
                GameObject spawned = Instantiate(prefab, hit.position, Quaternion.identity);

                if (type == EnemyType.Civilian)
                {
                    CivilianAI civ = spawned.GetComponent<CivilianAI>();
                    if (civ != null) aliveCivilians.Add(civ);
                }
                else
                {
                    StandardEnemyAI ai = spawned.GetComponent<StandardEnemyAI>();
                    if (ai != null)
                    {
                        // I don't think we need this anymore because it's seperated into 2 different prefabs but kept just in case
                        switch (type)
                        {
                            case EnemyType.MeleeShielded: aliveMeleeShielded.Add(ai); break;
                            case EnemyType.MeleeUnshielded: aliveMeleeUnshielded.Add(ai); break;
                            case EnemyType.Ranged: aliveRanged.Add(ai); break;
                            case EnemyType.Rapid: aliveRapid.Add(ai); break;
                        }
                    }
                }
                return;
            }
        }
    }

    private void ClearList<T>(List<T> list) where T : Component
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] != null)
            {
                Destroy(list[i].gameObject);
            }
        }
        list.Clear();
    }
}
