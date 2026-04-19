using UnityEngine;

public class CivilianSpawner : MonoBehaviour
{
    [SerializeField] private GameObject civilianPrefab;

    [Header("Gizmos")]
    [SerializeField] private Color gizmoColor = Color.yellow;
    [SerializeField] private float gizmoRadius = 0.5f;

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

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        foreach (Transform child in transform)
        {
            Gizmos.DrawSphere(child.position, gizmoRadius);
        }
    }
}
