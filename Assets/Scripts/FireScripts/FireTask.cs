using UnityEngine;

public class FireTask : BaseTask
{
    private bool hammerIgnited = false;
    private bool buildingBurned = false;

    void Start()
    {
        taskName = "Burn the building";
        taskDescription = "Ignite the hammer";
        StartTask();
    }

    public void HammerIgnited()
    {
        if (hammerIgnited) return;

        hammerIgnited = true;

        taskDescription = "Go to the building";
        TaskHUD.Instance.RefreshUI();

        CheckCompletion();
    }

    public void BuildingBurned()
    {
        if (buildingBurned) return;

        buildingBurned = true;

        taskDescription = "Completed!";
        TaskHUD.Instance.RefreshUI();

        CheckCompletion();
    }

    public override void CheckCompletion()
    {
        if (hammerIgnited && buildingBurned)
        {
            CompleteTask();
        }
    }
}