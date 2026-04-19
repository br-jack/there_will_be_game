using UnityEngine;

public class TaskArrow3D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform target;

    private Camera camera;

    private void Start()
    {
        camera = Camera.main;
    }

    private void Update()
    {
        Vector3 direction = target.position - player.position;

        direction.y = 0f; // Flatten the y

        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        direction.Normalize();

        // Convert world direction into camera space
        Vector3 localDir = camera.transform.InverseTransformDirection(direction);

        // Calculate angle for rotation
        float angle = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;

        // Rotate arrow around Z so it behaves like UI
        transform.localRotation = Quaternion.Euler(0f, 0f, -angle);
    }

    // gonna need these eventually
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void Show(bool show)
    {
        gameObject.SetActive(show);
    }
}
