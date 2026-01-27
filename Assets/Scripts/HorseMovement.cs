using UnityEngine;
using UnityEngine.InputSystem;

public class HorseMovement : MonoBehaviour
{
    public float acceleration = 10f;
    public float deceleration = 6f;
    public float maxSpeed = 14f;

    public float turnSpeed = 70f;
    public float turnSpeedAtZero = 100f;

    private float _currentSpeed = 0f;

    private Rigidbody _rb;
    
    private float _throttleInput;
    private float _turnInput;
    
    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 moveVector = context.ReadValue<Vector2>();
        _turnInput = moveVector.x;
        _throttleInput = moveVector.y;
    }

    void Start() 
    {
        _rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()  
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        if (_throttleInput > 0f)
        {
            _currentSpeed += acceleration * _throttleInput * Time.fixedDeltaTime;
        }
        else
        {
            _currentSpeed -= deceleration * Time.fixedDeltaTime;
        }

        _currentSpeed = Mathf.Clamp(_currentSpeed, 0f, maxSpeed);

        //restrict turning more at higher speeds
        float speedPercent = _currentSpeed / maxSpeed;
        float effectiveTurnSpeed = Mathf.Lerp(turnSpeedAtZero, turnSpeed, speedPercent);

        Quaternion turnRotation = Quaternion.Euler(0f, _turnInput * effectiveTurnSpeed * Time.fixedDeltaTime, 0f);

        _rb.MoveRotation(_rb.rotation * turnRotation);

        Vector3 newPosition = _rb.position + ((_currentSpeed * Time.fixedDeltaTime) * transform.forward);

        _rb.MovePosition(newPosition);
    }
}
