using UnityEngine;

public class BuildingProximityIgnition : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private HammerFireController hammerFireController;

    [SerializeField] private BurnableBuilding burnableBuilding;
    [SerializeField] private float igniteDistance = 10f;


    private bool hasIgnited = false;

    private void Update()
    {
        if (player == null || hammerFireController == null)
            return;

        if (hasIgnited)
            return;

        if (!hammerFireController.IsOnFire)
            return;

        float distance = Vector3.Distance(burnableBuilding.gameObject.transform.position, transform.position);

        if (distance <= igniteDistance)
        {
            burnableBuilding.IgniteBuilding();
            hasIgnited = true;
        }
    }
}
