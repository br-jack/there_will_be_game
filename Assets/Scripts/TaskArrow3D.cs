using UnityEngine;

public class TaskArrow3D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform target;

    [Header("Position Settings")]
    [SerializeField] private float heightAbovePlayer = 4f;
    [SerializeField] private Vector3 modelRotationOffset = new Vector3(90f, 0f, 0f);

    private void Update()
    {
        UpdatePosition();
        UpdateRotation();
    }

    private void UpdatePosition()
    {
        transform.position = player.position + Vector3.up * heightAbovePlayer;
    }

    private void UpdateRotation()
    {
        Vector3 direction = target.position - player.position;

        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

        transform.rotation = targetRotation * Quaternion.Euler(modelRotationOffset);
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
