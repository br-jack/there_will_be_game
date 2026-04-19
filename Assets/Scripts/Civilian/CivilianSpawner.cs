using UnityEngine;

public class CivilianSpawner : MonoBehaviour
{
    [SerializeField] private GameObject civilianPrefab;

    void Awake()
    {
        if (civilianPrefab == null)
        {
            Debug.LogWarning("CivilianSpawner has no civilianPrefab assigned.", this);
            return;
        }

        foreach (Transform point in transform)
        {
            Instantiate(civilianPrefab, point.position, point.rotation);
        }
    }
}
