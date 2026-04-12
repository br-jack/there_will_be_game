using UnityEngine;

public class BuildingProximityIgnition : MonoBehaviour
{
    [SerializeField] private HammerFireController hammerFireController;
    [SerializeField] private FireTask fireTask;

    [SerializeField] private float igniteDistance = 5f;
    [SerializeField] private float requiredHoldTime = 1f;

    private BurnableBuilding targetBuilding;
    private float currentHoldTime = 0f;

    private void Update()
    {
        if (hammerFireController == null)
            return;

        // all the different checks to make sure igniting is possible

        // if hammer not on fire, reset target and timer
        if (!hammerFireController.IsOnFire)
        {
            targetBuilding = null;
            currentHoldTime = 0f;
            return;
        }
        BurnableBuilding closestBuilding = FindNearestBurnableBuildingInRange();
        // if no valid buildings nearby, reset target and timer
        if (closestBuilding == null)
        {
            targetBuilding = null;
            currentHoldTime = 0f;
            return;
        }
        // if we have a valid building, but it's different from our current target, switch targets and reset timer
        if (closestBuilding != targetBuilding)
        {
            targetBuilding = closestBuilding;
            currentHoldTime = 0f;
        }
        currentHoldTime += Time.deltaTime;

        // now check if hammer has been next to building for long enough
        if (currentHoldTime >= requiredHoldTime)
        {
            targetBuilding.IgniteBuilding();
            currentHoldTime = 0f;
            targetBuilding = null;
        }
    }

    private BurnableBuilding FindNearestBurnableBuildingInRange()
    {
        // scan scene for all buildings with the burnable script on it
        BurnableBuilding[] allBuildings = FindObjectsByType<BurnableBuilding>(FindObjectsSortMode.None);

        BurnableBuilding nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (BurnableBuilding building in allBuildings)
        {
            if (building == null || building.IsBurning)
                continue;

            float distance = Vector3.Distance(transform.position, building.transform.position);

            if (distance <= igniteDistance && distance < nearestDistance)
            {
                nearest = building;
                nearestDistance = distance;
            }
        }

        return nearest;
    }
}
