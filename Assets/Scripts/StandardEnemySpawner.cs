using UnityEngine;
using System.Collections.Generic;

public class StandardEnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public List<StandardEnemyAI> aliveEnemies = new List<StandardEnemyAI>();

    [Header("Spawn")]
    [SerializeField] private int spawnCount = 5;
    [SerializeField] private float spawnRadius = 10f;
    [SerializeField] private float spawnJitter = 0.5f;

    private void Start()
    {
        if (enemyPrefab != null)
        {
            SpawnEnemies(spawnCount, transform.position);
        }
    }

    public void SpawnEnemies(int count, Vector3 center)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 noise = Random.insideUnitCircle * spawnRadius;
            Vector3 pos = center + new Vector3(noise.x, 0f, noise.y);
            pos.y = center.y;

            GameObject enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
            StandardEnemyAI ai = enemy.GetComponent<StandardEnemyAI>();
            if (ai == null) continue;

            ai.Initialize(this);
            aliveEnemies.Add(ai);
        }
    }

    public void RemoveEnemy(StandardEnemyAI enemy)
    {
        aliveEnemies.Remove(enemy);
    }
}
