using UnityEngine;

/// <summary>
/// Smoothly moves an IK target transform toward a source transform.
/// Attach to the IK target GameObject and assign the source (e.g. hammer grip point).
/// </summary>
public class IKTargetFollower : MonoBehaviour
{
    [SerializeField] private Transform source;
    [SerializeField] private float followSpeed = 15f;
    [SerializeField] private bool matchRotation = true;

    private void LateUpdate()
    {
        if (source == null)
            return;

        transform.position = Vector3.Lerp(transform.position, source.position, followSpeed * Time.deltaTime);

        if (matchRotation)
            transform.rotation = Quaternion.Slerp(transform.rotation, source.rotation, followSpeed * Time.deltaTime);
    }
}
