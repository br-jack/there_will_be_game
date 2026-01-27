using UnityEngine;

public class HorseMovement : MonoBehaviour
{
    public float acceleration = 10f;
    public float deceleration = 6f;
    public float maxSpeed = 14f;

    public float turnSpeed = 70f;
    public float turnSpeedAtZero = 100f;

    private float currentSpeed = 0f;

    private Rigidbody _rb;
    
    private float _throttleInput;
    private float _turnInput;

    void Start() 
    {
        _rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        _throttleInput = Input.GetAxis("Vertical");
        _turnInput = Input.GetAxis("Horizontal");
    }

    void FixedUpdate()  
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        if (_throttleInput > 0f)
        {
            currentSpeed += acceleration * _throttleInput * Time.fixedDeltaTime;
        }
        else
        {
            currentSpeed -= deceleration * Time.fixedDeltaTime;
        }

        currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);

        float speedPercent = currentSpeed / maxSpeed;
        float effectiveTurnSpeed = Mathf.Lerp(turnSpeedAtZero, turnSpeed, speedPercent);

        Quaternion turnRotation = Quaternion.Euler(0f, _turnInput * effectiveTurnSpeed * Time.fixedDeltaTime, 0f);

        _rb.MoveRotation(_rb.rotation * turnRotation);

        Vector3 newPosition = _rb.position + ((currentSpeed * Time.fixedDeltaTime) * transform.forward);

        _rb.MovePosition(newPosition);
    }
}
