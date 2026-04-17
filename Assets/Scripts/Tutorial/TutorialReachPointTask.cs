using UnityEngine;

public class TutorialReachPointTask : BaseTask
{
    private bool playerReachedGoal = false;

    private void Start()
    {
        taskName = "Approach the marked area";
        taskDescription = "Walk to the glowing marker";
        StartTask();
    }

    public void MarkGoalReached()
    {
        if (!playerReachedGoal)
        {
            playerReachedGoal = true;
            taskDescription = "Completed";
            TaskHUD.Instance.RefreshUI();
            CheckCompletion();
        }
    }

    public override void CheckCompletion()
    {
        if (playerReachedGoal)
        {
            CompleteTask();
        }
    }
}
