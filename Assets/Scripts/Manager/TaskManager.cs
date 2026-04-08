using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance;
    public List<BaseTask> activeTasks = new List<BaseTask>();

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
        TaskHUD.Instance.RemoveTaskFromUI(task); //remove old version
        TaskHUD.Instance.AddTaskToUI(task); // add the task back to the UI with the check box ticked
        StartCoroutine(RemoveTaskAfterDelay(task, 2f)); // remove the task from the UI again after a short delay
        //RewardSystem.Instance.GrantReward(task);
        Debug.Log($"Task complete: {task.taskName}");
    }

    private IEnumerator RemoveTaskAfterDelay(BaseTask task, float delay)
    {
        yield return new WaitForSeconds(delay);
        TaskHUD.Instance.RemoveTaskFromUI(task);
    }
}