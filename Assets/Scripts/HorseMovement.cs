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

    private float speedPercent;

    private float scaledJumpForce;

    private Rigidbody _rb;

    private CapsuleCollider col;
    
    private float _throttleInput;
    private float _turnInput;
    private float _brakeInput;

    public float jumpForce = 8.5f;
    public LayerMask groundMask;
    public float groundCheckDistance = 1.0f;

    private bool _jumpPressed;
    
    // public void OnMove(InputAction.CallbackContext context)
    // {
    //     Vector2 moveVector = context.ReadValue<Vector2>();
    //     _turnInput = moveVector.x;
    //     _throttleInput = moveVector.y;
    // }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
            _jumpPressed = true;
    }

    public void onSteer(InputAction.CallbackContext context)
    {
        _turnInput = context.ReadValue<float>();
    }

    public void onAccelerate(InputAction.CallbackContext context)
    {
        _throttleInput = context.ReadValue<float>();
    }

    public void onBrake(InputAction.CallbackContext context)
    {
        _brakeInput = context.ReadValue<float>();
    }

    private void Awake() 
    {
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()  
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.2f;
        bool grounded = Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance, groundMask);

        //scale jumping to speed
        speedPercent = _currentSpeed / maxSpeed;
        scaledJumpForce = jumpForce * speedPercent;

        if (_jumpPressed && grounded && _currentSpeed > 2f)
        {
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        _jumpPressed = false;

        if (grounded) //prevents accelerating and decelerating whilst midair
        {
            if (_throttleInput > 0.0f)
            {
                _currentSpeed += acceleration * _throttleInput * Time.fixedDeltaTime;
            }
            if (_brakeInput > 0.0f)
            {
                _currentSpeed -= deceleration * Time.fixedDeltaTime;
            }
            if (_throttleInput == 0.0f && _brakeInput == 0.0f)
            {
                if (_currentSpeed > 0f)
                {
                    _currentSpeed -= deceleration * Time.fixedDeltaTime;
                } else if (_currentSpeed < 0f)
                {
                    _currentSpeed += deceleration * Time.fixedDeltaTime;
                }
            }
            // code for using onMove - vector 2 via joystick
            // if (_throttleInput < 0.0f)
            // {
            //     _currentSpeed -= deceleration * Time.fixedDeltaTime;
            // }
            // else 
            // {
            //     if (_currentSpeed > 0f)
            //     {
            //         _currentSpeed -= deceleration * Time.fixedDeltaTime;
            //     } else if (_currentSpeed < 0f)
            //     {
            //         _currentSpeed += deceleration * Time.fixedDeltaTime;
            //     }
                
            //     if (Mathf.Abs(_currentSpeed) < 0.01f) _currentSpeed = 0f;
            // }

            _currentSpeed = Mathf.Clamp(_currentSpeed, -1.0f, maxSpeed);
        }

        //restrict turning more at higher speeds
        float effectiveTurnSpeed = Mathf.Lerp(turnSpeedAtZero, turnSpeed, speedPercent);
        if (!grounded) {effectiveTurnSpeed *= 0.2f;}
            
        Quaternion turnRotation = Quaternion.Euler(0f, _turnInput * effectiveTurnSpeed * Time.fixedDeltaTime, 0f);

        _rb.MoveRotation(_rb.rotation * turnRotation);
        

        Vector3 forwardMovement = transform.forward * (_currentSpeed * Time.fixedDeltaTime); 
        
        _rb.MovePosition(_rb.position + forwardMovement);
    }
}
