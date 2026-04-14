using System;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance;
    public List<BaseTask> activeTasks = new List<BaseTask>();
    public static Action<BaseTask> OnAnyTaskCompleted;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterTask(BaseTask task)
    {
        if (!activeTasks.Contains(task))
        {
            activeTasks.Add(task);
            TaskHUD.Instance.AddTaskToUI(task);
            Debug.Log($"Task registered: {task.taskName}");
        }
    }

    public void OnTaskCompleted(BaseTask task)
    {
        activeTasks.Remove(task);
        TaskHUD.Instance.RemoveTaskFromUI(task); //remove and add back checked as complete
        TaskHUD.Instance.AddTaskToUI(task);
        //RewardSystem.Instance.GrantReward(task);
        OnAnyTaskCompleted?.Invoke(task);
        Debug.Log($"Task complete: {task.taskName}");
    }
}