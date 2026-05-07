using UnityEngine;

public class TaskArrowManager : MonoBehaviour
{
    public static TaskArrowManager Instance { get; private set; }
    [SerializeField] private TaskArrow3D taskArrow;

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

        taskArrow.SetTarget(target);
        taskArrow.gameObject.SetActive(true);
    }

    public void HideArrow()
    {
        if (taskArrow != null)
        {
            taskArrow.gameObject.SetActive(false);
        }
    }
}
