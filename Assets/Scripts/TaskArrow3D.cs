using UnityEngine;

public class TaskArrow3D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform target;

    [Header("Position Settings")]
    [SerializeField] private float heightAbovePlayer = 4f;
    [SerializeField] private float followSmooth = 100000f;

    private void Update()
    {
        UpdatePosition();
        UpdateRotation();
    }

    private void UpdatePosition()
    {
        Vector3 desiredPosition = player.position + Vector3.up * heightAbovePlayer;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            Time.deltaTime * followSmooth
        );
    }

    private void UpdateRotation()
    {
        Vector3 direction = target.position - player.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90f, 0f, 0f);
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
