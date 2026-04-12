using UnityEngine;

public class BuildingProximityIgnition : MonoBehaviour
{
    [SerializeField] private HammerFireController hammerFireController;
    [SerializeField] private BurnableBuilding burnableBuilding;
    [SerializeField] private FireTask fireTask;

    [SerializeField] private float igniteDistance = 5f;
    [SerializeField] private float requiredHoldTime = 2f;

    private bool hasIgnited = false;
    private float currentHoldTime = 0f;

    private void Update()
    {
        if (burnableBuilding == null || hammerFireController == null)
            return;

        if (!hammerFireController.IsOnFire && !hasIgnited && !burnableBuilding.IsBurning)
        {
            currentHoldTime = 0f;
            return;
        }

        float distance = Vector3.Distance(transform.position, burnableBuilding.transform.position);

        if (distance <= igniteDistance)
        {
            currentHoldTime += Time.deltaTime;

            if (currentHoldTime >= requiredHoldTime)
            {
                burnableBuilding.IgniteBuilding();

                if (fireTask != null)
                    fireTask.BuildingBurned();

                hasIgnited = true;
            }
        }
        else
        {
            currentHoldTime = 0f;
        }
    }
}
