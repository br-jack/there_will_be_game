using UnityEngine;

public class HorseMovement : MonoBehaviour
{
    public float acceleration = 10f;
    public float deceleration = 6f;
    public float maxSpeed = 14f;

    public float turnSpeed = 70f;
    public float turnSpeedAtZero = 100f;

    private float currentSpeed = 0f;

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        float throttleInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        if (throttleInput > 0f)
        {
            currentSpeed += acceleration * throttleInput * Time.fixedDeltaTime;
        }
        else
        {
            currentSpeed -= deceleration * Time.fixedDeltaTime;
        }

        currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);

        float speedPercent = currentSpeed / maxSpeed;
        float effectiveTurnSpeed = Mathf.Lerp(turnSpeedAtZero, turnSpeed, speedPercent);

        Rigidbody rb = GetComponent<Rigidbody>();

        Quaternion turnRotation = Quaternion.Euler(0f, turnInput * effectiveTurnSpeed * Time.fixedDeltaTime, 0f);

        rb.MoveRotation(rb.rotation * turnRotation);

        Vector3 newPosition = rb.position + transform.forward * currentSpeed * Time.fixedDeltaTime;

        rb.MovePosition(newPosition);
    }
}
