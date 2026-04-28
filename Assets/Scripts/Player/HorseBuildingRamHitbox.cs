using UnityEngine;

public class HorseBuildingRamHitbox : MonoBehaviour
{
    [SerializeField] private PlayerPowerUpReceiver powerUpReceiver;
    [SerializeField] private float minimumRamSpeed = 2f;

    private Vector3 previousPosition;
    private float currentSpeed;

    private void Start()
    {
        previousPosition = transform.position;
    }

    private void Update()
    {
        Vector3 movement = transform.position - previousPosition;
        currentSpeed = movement.magnitude / Time.deltaTime;
        previousPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {

        if (!powerUpReceiver.canRamBuildings)
        {
            return;
        }

        if (currentSpeed < minimumRamSpeed)
        {
            return;
        }

        DestructibleObject destructibleObject = other.GetComponentInParent<DestructibleObject>();

        if (destructibleObject == null)
        {
            return;
        }

        Vector3 impactPoint = other.ClosestPoint(transform.position);
        destructibleObject.BreakFromHorseRam(impactPoint);
    }
}
