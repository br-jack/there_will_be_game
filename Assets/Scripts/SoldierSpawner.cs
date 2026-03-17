using UnityEngine;

public class SoldierSpawner : MonoBehaviour
{
    public GameObject enemyPrefab; 
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private int maxEnemies = 5;
    [SerializeField] private int currentEnemies = 0;
    [SerializeField] private int spawnRadius = 1;

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
