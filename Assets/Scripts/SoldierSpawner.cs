using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab; 
    public float spawnInterval = 2f;
    public int maxEnemies = 5;
    public int currentEnemies = 0;
    public int spawnRadius = 1;

    private float timer = 0.0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0.0f;
        }
    }

    void SpawnEnemy()
    {
        if (currentEnemies < maxEnemies)
        {
            GameObject enemy = Instantiate(enemyPrefab, transform.position, transform.rotation);
            currentEnemies++;
        }
    }
}
