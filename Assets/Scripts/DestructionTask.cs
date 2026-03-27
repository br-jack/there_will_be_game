using UnityEngine;

public class DestructionTask : BaseTask
{
    public int buildingsRequired = 3;
    private int buildingsDestroyed = 0;

    void Start()
    {
        taskName = "Destroy 3 buildings";
        taskDescription = $"Destroy {buildingsRequired} buildings";
        StartTask();
    }

    public void BuildingDestroyed()
    {
        buildingsDestroyed++;
        Debug.Log($"Buildings destroyed: {buildingsDestroyed}/{buildingsRequired}");
        CheckCompletion();
    }

    public override void CheckCompletion()
    {
        if (buildingsDestroyed >= buildingsRequired)
            CompleteTask();
    }
}