using UnityEngine;

public class FootballTask : BaseTask
{
    private bool Scored = false;
    public int goals = 0;

    void Start()
    {
        taskName = "Score a goal against AS Roma FC";
        taskDescription = $"{goals}/1 Scored";
        StartTask();
    }

    public void goalScored()
    {
        Scored = true;
        goals++;
        Debug.Log($"goals Scored: {goals}/1");
        taskDescription = $"{goals}/1 Scored";
        TaskHUD.Instance.RefreshUI();
        CheckCompletion();
    }

    public override void CheckCompletion()
    {
        if (Scored == true)
            CompleteTask();
            GameObject[] walls = GameObject.FindGameObjectsWithTag("PitchWall");
            foreach(GameObject wall in walls) wall.SetActive(false);
    }
}