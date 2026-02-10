using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public List<EnemyMovement> aliveEnemies = new List<EnemyMovement>();
    public GameObject enemyPrefab; 
    public float spawnInterval = 2f;
    public int maxEnemies = 5;
    public int currentEnemies = 0;
    public enum FormationType
    {
        Grid
    }

    [Header("Formation")]
    public FormationType formationType = FormationType.Grid; 
    public int formationRows = 1;
    public int formationColumns = 5;
    public Vector2 formationSpacing = new Vector2(2f, 2f);
    public bool centerFormationOnSpawner = true;

    private float timer = 0.0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0.0f;
        }
        
        UpdateFormationTargets();
    }

    void SpawnEnemy()
    {
        if (currentEnemies < maxEnemies)
        {
            Vector3 spawnPosition = GetFormationPosition(currentEnemies);
            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, transform.rotation);
            EnemyMovement enemyMovement = enemy.GetComponent<EnemyMovement>();
            enemyMovement.spawner = this;
            aliveEnemies.Add(enemyMovement);
            currentEnemies++;
            UpdateFormationTargets();
        }
    }

    public void UpdateFormationTargets()
    {
        GameObject playerRef = GameObject.FindWithTag("Player");
        Vector3 anchorPosition = playerRef != null ? playerRef.transform.position : transform.position;

        for (int i=0; i < aliveEnemies.Count; i++)
        {
            EnemyMovement enemy = aliveEnemies[i];
            if (enemy == null) {
                continue;
            }

            Vector3 offset = GetGridSpacing(i);
            enemy.SetFormationTarget(anchorPosition + offset);
        }
    }

    private Vector3 GetGridSpacing(int index)
    {
        int columns = Mathf.Max(1, formationColumns);

        int col = index % columns;
        int row = Mathf.FloorToInt(index / columns);

        float x = col * formationSpacing.x;
        float z = row * formationSpacing.y;

        return new Vector3(x, 0f, z);
    }

    Vector3 GetFormationPosition(int index)
    {
        int columns = Mathf.Max(1, formationColumns);
        int rows = Mathf.Max(1, formationRows);

        int col = index % columns;
        int row = Mathf.FloorToInt(index / columns);

        if (row >= rows)
        {
            row = rows - 1;
        }

        float xOffset = col * formationSpacing.x;
        float zOffset = row * formationSpacing.y;

        if (centerFormationOnSpawner)
        {
            float totalWidth = (columns - 1) * formationSpacing.x;
            xOffset -= totalWidth * 0.5f;
        }

        return transform.position + new Vector3(xOffset, 0f, zOffset);
    }

}
