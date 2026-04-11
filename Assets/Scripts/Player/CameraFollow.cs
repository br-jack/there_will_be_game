using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;

    [SerializeField] private Vector3 offsetFromPlayerPosition = new Vector3(0, 6.0f, -6);
    [SerializeField] private Vector3 playerLookAtOffset = Vector3.up * 1.0f;

    void LateUpdate()
    {
        transform.position = player.position + player.TransformDirection(offsetFromPlayerPosition);
        transform.LookAt(player.position + playerLookAtOffset);
    }
}
