using UnityEngine;
using UnityEngine.UI;

public class UIScreenArrow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform target;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private RectTransform arrowRectTransform;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (player == null || target == null || arrowRectTransform == null || mainCamera == null)
            return;

        Vector3 toTarget = target.position - player.position;
        toTarget.y = 0f;

        Vector3 cameraForward = mainCamera.transform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        Vector3 cameraRight = mainCamera.transform.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();

        float x = Vector3.Dot(toTarget.normalized, cameraRight);
        float y = Vector3.Dot(toTarget.normalized, cameraForward);

        float angle = Mathf.Atan2(-x, y) * Mathf.Rad2Deg;

        arrowRectTransform.localRotation = Quaternion.Euler(0f, 0f, angle + 180f);
    }
}
