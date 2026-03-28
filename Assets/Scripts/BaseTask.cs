using UnityEngine;

public abstract class BaseTask : MonoBehaviour
{
    public string taskName;
    public string taskDescription;
    public bool isComplete = false;

    public virtual void StartTask()
    {
        TaskManager.Instance.RegisterTask(this);
    }

    public void CompleteTask()
    {
        if (isComplete) return;
        isComplete = true;
        TaskManager.Instance.OnTaskCompleted(this);
    }

    // Each task type overrides this to define its own completion logic
    public abstract void CheckCompletion();
}