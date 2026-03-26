using UnityEngine;

public class KillEnemiesTask : BaseTask
{
    public int EnemiesRequired = 3;
    private int EnemiesKilled = 0;

    void Start()
    {
        taskName = "Kill 3 enemies";
        taskDescription = $"Kill {EnemiesRequired} enemies";
        StartTask();
    }

    public void EnemyKilled()
    {
        EnemiesKilled++;
        Debug.Log($"Enemies killed: {EnemiesKilled}/{EnemiesRequired}");
        CheckCompletion();
    }

    public override void CheckCompletion()
    {
        if (EnemiesKilled >= EnemiesRequired)
            CompleteTask();
    }
}