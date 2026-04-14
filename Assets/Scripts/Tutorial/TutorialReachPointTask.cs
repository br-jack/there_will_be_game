using UnityEngine;

public class TutorialReachPointTask : BaseTask
{
    [SerializeField] private string tutorialTaskName = "Approach the marked area";
    [SerializeField] private string tutorialTaskDescription = "Walk to the glowing marker";

    private bool playerReachedGoal = false;

    private void Awake()
    {
        taskName = tutorialTaskName;
        taskDescription = tutorialTaskDescription;
    }

    public override void StartTask()
    {
        playerReachedGoal = false;
        taskName = tutorialTaskName;
        taskDescription = tutorialTaskDescription;
        base.StartTask();
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
