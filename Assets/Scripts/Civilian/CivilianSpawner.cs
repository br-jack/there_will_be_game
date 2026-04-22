using UnityEngine;
using UnityEngine.AI;

public class CivilianSpawner : MonoBehaviour
{
    [SerializeField] private GameObject civilianPrefab;

    // Search distance when snapping a spawn point onto the NavMesh.
    // Keep this generous — a spawner placed slightly above the terrain
    // should still resolve down to valid ground.
    [SerializeField] private float navMeshSearchRadius = 50f;

    void Awake()
    {
        if (civilianPrefab == null)
        {
            Debug.LogWarning("CivilianSpawner has no civilianPrefab assigned.", this);
            return;
        }

        // If no child spawn points are configured, fall back to spawning at our own position.
        if (transform.childCount == 0)
        {
            SpawnOne(transform.position, transform.rotation);
            return;
        }

        foreach (Transform point in transform)
        {
            SpawnOne(point.position, point.rotation);
        }
    }

    private void SpawnOne(Vector3 desiredPos, Quaternion rotation)
    {
        Vector3 finalPos = desiredPos;
        if (NavMesh.SamplePosition(desiredPos, out NavMeshHit hit, navMeshSearchRadius, NavMesh.AllAreas))
        {
            finalPos = hit.position;
        }
        else
        {
            Debug.LogWarning($"CivilianSpawner: no NavMesh within {navMeshSearchRadius}m of {desiredPos}. Spawning at raw position — civilian may fall or stand still.", this);
        }

        Instantiate(civilianPrefab, finalPos, rotation);
    }
}
