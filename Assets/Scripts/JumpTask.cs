using UnityEngine;

public class JumpTask : BaseTask
{
    public int JumpsRequired = 3;
    private int Jumps = 0;

    void Start()
    {
        taskName = "Jump 3 times";
        taskDescription = $"{Jumps}/{JumpsRequired} Done";
        StartTask();
    }

    public void JumpDone()
    {
        Jumps++;
        Debug.Log($"Jumps done: {Jumps}/{JumpsRequired}");
        taskDescription = $"{Jumps}/{JumpsRequired} Done";
        TaskHUD.Instance.RefreshUI();
        CheckCompletion();
    }

    public override void CheckCompletion()
    {
        if (Jumps >= JumpsRequired)
            CompleteTask();
    }
}