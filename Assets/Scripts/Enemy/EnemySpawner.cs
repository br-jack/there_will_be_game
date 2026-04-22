using System.Collections.Generic;
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

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject meleeUnshieldedEnemyPrefab;
    [SerializeField] private GameObject meleeShieldedEnemyPrefab;
    [SerializeField] private GameObject rapidEnemyPrefab;
    [SerializeField] private GameObject rangedEnemyPrefab;
    [SerializeField] private GameObject civilianPrefab;

    [SerializeField] private float minDistanceFromPlayer = 15f;
    [SerializeField] private float maxDistanceFromPlayer = 100f;
    private float navMeshSearchRadius = 2.5f;

    [Header("Waves")]
    // Spawner stays on the last wave once the waves have ran out.
    [SerializeField] private Wave[] waves;

    // Fires when a new wave begins. The int is the 1-based wave number.
    public event System.Action<int> OnWaveStarted;

    private Transform player;

    // Keep track of alive instances of each type.
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
        OnWaveStarted?.Invoke(1);
    }

    private void Update()
    {
        // Prune dead entries.
        aliveMeleeShielded.RemoveAll(e => e == null);
        aliveMeleeUnshielded.RemoveAll(e => e == null);
        aliveRanged.RemoveAll(e => e == null);
        aliveRapid.RemoveAll(e => e == null);
        aliveCivilians.RemoveAll(c => c == null);

        // Halt wave/break/spawn timers while disabled (paused or game over).
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

        // Advance to the next wave.
        waveTimer += Time.deltaTime;
        if (waveTimer >= currentWave.duration && currentWaveIndex < waves.Length - 1)
        {
            if (currentWave.clearRemainingOnEnd)
            {
                ClearAllSpawned();
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
        // Calculate remaining capacity for each type.
        int remainMeleeShielded = Mathf.Max(0, wave.meleeShielded - aliveMeleeShielded.Count);
        int remainMeleeUnshielded = Mathf.Max(0, wave.meleeUnshielded - aliveMeleeUnshielded.Count);
        int remainRanged = Mathf.Max(0, wave.ranged - aliveRanged.Count);
        int remainRapid = Mathf.Max(0, wave.rapid - aliveRapid.Count);
        int remainCivilians = Mathf.Max(0, wave.civilians - aliveCivilians.Count);

        int total = remainMeleeShielded + remainMeleeUnshielded + remainRanged + remainRapid + remainCivilians;
        if (total <= 0) return;

        // Weighted random pick.
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
    }

    private void SpawnOne(EnemyType type)
    {
        GameObject prefab;
        bool keepShield = false;

        switch (type)
        {
            case EnemyType.MeleeShielded:
                prefab = meleeShieldedEnemyPrefab;
                keepShield = true;
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
        for (int attempt = 0; attempt < 10; attempt++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float distance = Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);
            Vector3 offset = new Vector3(Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);
            Vector3 candidate = player.position + offset;

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
                        // Strip shield if this type shouldn't have one.
                        if (!keepShield && ai.shield != null)
                        {
                            ai.shield = null;
                        }

                        // Track in the correct list.
                        switch (type)
                        {
                            case EnemyType.MeleeShielded:   aliveMeleeShielded.Add(ai); break;
                            case EnemyType.MeleeUnshielded: aliveMeleeUnshielded.Add(ai); break;
                            case EnemyType.Ranged:          aliveRanged.Add(ai); break;
                            case EnemyType.Rapid:           aliveRapid.Add(ai); break;
                        }
                    }
                }
                return;
            }
        }
    }

    private void ClearAllSpawned()
    {
        ClearList(aliveMeleeShielded);
        ClearList(aliveMeleeUnshielded);
        ClearList(aliveRanged);
        ClearList(aliveRapid);
        ClearList(aliveCivilians);
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
