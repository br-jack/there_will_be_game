using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TaskHUD : MonoBehaviour
{
    public static TaskHUD Instance;
    public TextMeshProUGUI taskListText;

    private List<BaseTask> displayedTasks = new List<BaseTask>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddTaskToUI(BaseTask task)
    {
        if (!displayedTasks.Contains(task))
            displayedTasks.Add(task);

        RefreshUI();
    }

    public void RemoveTaskFromUI(BaseTask task)
    {
        displayedTasks.Remove(task);
        RefreshUI();
    }

    public void RefreshUI()
    {
        string display = "";
        foreach (BaseTask task in displayedTasks)
        {
            if (task.isComplete == true) {display += $"[x] {task.taskName}\n{task.taskDescription}\n\n";}
            else { display += $"[ ] {task.taskName}\n{task.taskDescription}\n\n"; }
        }
        taskListText.text = display;
    }
}