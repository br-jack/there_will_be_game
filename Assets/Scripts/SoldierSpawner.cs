using UnityEngine;

public class SoldierSpawner : MonoBehaviour
{
    public GameObject enemyPrefab; 
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private int maxEnemies = 5;
    [SerializeField] private int currentEnemies = 0;
    [SerializeField] private float spawnRadius = 1f;

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

    private Vector3 RandomOffsetHelper()
    {
        Vector3 offset = Random.insideUnitCircle * spawnRadius;
        return new Vector3(offset.x, 0, offset.y)
    }

    void SpawnEnemy()
    {
        if (currentEnemies < maxEnemies)
        {
            GameObject enemy = Instantiate(enemyPrefab, transform.position + RandomOffsetHelper(), transform.rotation);
            currentEnemies++;
        }
    }
}
