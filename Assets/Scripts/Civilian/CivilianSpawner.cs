using UnityEngine;
using UnityEngine.AI;

// old implementation we tried but then realised integrating into main spawner is much better
// NOT IN USE - feel free to remove after removing dependencies

public class CivilianSpawner : MonoBehaviour
{
    [SerializeField] private GameObject civilianPrefab;
    [SerializeField] private float navMeshSearchRadius = 50f;

    void Awake()
    {
        if (civilianPrefab == null)
        {
            return;
        }

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
