using UnityEngine;

public class TutorialReachPointTask : BaseTask
{
    [SerializeField] private string tutorialTaskName = "Approach the marked area";
    [SerializeField] private string tutorialTaskDescription = "Walk to the glowing marker";

    private bool playerReachedGoal = false;

    public override void StartTask()
    {
        playerReachedGoal = false;
        isComplete = false;

        taskName = tutorialTaskName;
        taskDescription = tutorialTaskDescription;

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
        taskDescription = "Completed";
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
