using Unity.VisualScripting;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;

    void LateUpdate()
    {
        transform.position = player.position + player.TransformDirection(new Vector3(0, 5, -10));
        transform.LookAt(player.position + Vector3.up * 2f);
    }
}
