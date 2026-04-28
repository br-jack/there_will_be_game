using UnityEngine;

public class TaskArrowManager : MonoBehaviour
{
    public static TaskArrowManager Instance { get; private set; }

    [SerializeField] private TaskArrow3D taskArrow;

    private Transform currentTarget;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        HideArrow();
    }

    public void PointTo(Transform target)
    {
        if (taskArrow == null || target == null)
        {
            return;
        }

        currentTarget = target;
        taskArrow.SetTarget(target);
        taskArrow.Show(true);
    }

    public void HideArrow()
    {
        currentTarget = null;

        if (taskArrow != null)
        {
            taskArrow.Show(false);
        }
    }

    public Transform GetCurrentTarget()
    {
        return currentTarget;
    }
}
