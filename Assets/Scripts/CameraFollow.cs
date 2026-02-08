using Unity.VisualScripting;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;

    void LateUpdate()
    {
        transform.position = player.position + player.TransformDirection(new Vector3(0, 2.5f, -4));
        transform.LookAt(player.position + Vector3.up * 2f);
    }
}
