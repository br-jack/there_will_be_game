using UnityEngine;

public class TutorialReachPointTask : BaseTask
{
    [SerializeField] private string tutorialTaskName = "Approach the marked area";

    private bool playerReachedGoal = false;

    public override void StartTask()
    {
        playerReachedGoal = false;
        isComplete = false;

        taskName = tutorialTaskName;

        base.StartTask();
        TaskHUD.Instance.RefreshUI();
    }

    public void MarkGoalReached()
    {
        if (playerReachedGoal || isComplete)
        {
            return;
        }

        playerReachedGoal = true;
        TaskHUD.Instance.RefreshUI();
        CheckCompletion();
    }

    public override void CheckCompletion()
    {
        if (playerReachedGoal)
        {
            CompleteTask();
        }
    }
}
