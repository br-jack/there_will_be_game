using Unity.VisualScripting;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public horseMovementGaits horseMovement;
    private Camera camera;

    [SerializeField] private Vector3 offsetFromPlayerPosition = new Vector3(0, 6.0f, -6);
    [SerializeField] private Vector3 playerLookAtOffset = Vector3.up * 1.0f;

    [Header("Dynamic FOV")]
    [SerializeField] private float baseFOV = 60f;
    [SerializeField] private float maxFOV = 75f;
    [SerializeField] private float speedForMaxFOV = 35f;
    [SerializeField] private float fovSmoothSpeed = 5f;

    private void Start()
    {
        camera = GetComponent<Camera>();
        camera.fieldOfView = baseFOV;
        horseMovement = player.GetComponent<horseMovementGaits>();
    }

    void LateUpdate()
    {
        transform.position = player.position + player.TransformDirection(offsetFromPlayerPosition);
        transform.LookAt(player.position + playerLookAtOffset);

        float currentSpeed = horseMovement.getCurrentSpeed();
        float speed01 = Mathf.Clamp01(currentSpeed / speedForMaxFOV);
        float targetFOV = Mathf.Lerp(baseFOV, maxFOV, speed01);

        camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, targetFOV, Time.deltaTime * fovSmoothSpeed);
    }
}
