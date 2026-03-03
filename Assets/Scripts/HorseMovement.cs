using UnityEngine;
using UnityEngine.InputSystem;

public class HorseMovement : MonoBehaviour
{
    public float acceleration = 12f;
    public float deceleration = 2f; //ambient deceleration when no acceleration or braking/reverse

    public float brake = 20f;
    public float maxSpeed = 14f;

    public float turnSpeed = 70f;
    public float turnSpeedAtZero = 100f;
    
    private float _currentSpeed = 0f;

    private float speedPercent;

    private float scaledJumpForce;

    private Rigidbody _rb;
    
    private float _throttleInput;
    private float _turnInput;
    private float _brakeInput;

    public float jumpForce = 8f;
    private bool _jumpHeld;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    public LayerMask groundMask;
    public float groundCheckDistance = 0.3f;

    private float _groundedTimer = 0f;

    private bool _jumpPressed;
    
    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 moveVector = context.ReadValue<Vector2>();
        _turnInput = moveVector.x;
        _throttleInput = moveVector.y;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
            _jumpPressed = true;
            _jumpHeld = true;
        if (context.canceled) 
            _jumpHeld = false;
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

    private void Update()
    {
        //_jumpHeld = Input.GetButton("Jump");
    }

    private void FixedUpdate()  
    {
        HandleMovement();
        if (_rb.linearVelocity.y < 0) //speed up fall for feel  
        { 
            _rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime; 
        }
        else if (_rb.linearVelocity.y > 0 && !_jumpHeld) //smaller jump when jump button not held
        { 
            _rb.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime; 
        }
    }

    private void HandleMovement()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.2f;
        bool grounded = Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance, groundMask);

        //scale jumping to speed
        speedPercent = _currentSpeed / maxSpeed;
        scaledJumpForce = jumpForce * speedPercent * 1.2f;

        scaledJumpForce = Mathf.Clamp(scaledJumpForce, 0.0f, jumpForce);

        if (_jumpPressed && grounded && _groundedTimer > 0.1f && _currentSpeed > 2f)
        {
            //_rb.AddForce(Vector3.up * scaledJumpForce, ForceMode.Impulse);
            Vector3 v = _rb.linearVelocity;
            v.y = jumpForce;
            _rb.linearVelocity = v;

        }

        _jumpPressed = false;

        //restrict turning more at higher speeds
        float effectiveTurnSpeed = Mathf.Lerp(turnSpeedAtZero, turnSpeed, speedPercent);

        if (grounded) //prevents accelerating and decelerating whilst midair
        {
            _groundedTimer += Time.fixedDeltaTime;

            float netForce = 0f;

            netForce += acceleration * _throttleInput;
            netForce -= brake * _brakeInput;

            _currentSpeed += netForce * Time.fixedDeltaTime;

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
            _currentSpeed = Mathf.Clamp(_currentSpeed, -1.0f, maxSpeed);
        } else
        {
            _groundedTimer = 0f;
            effectiveTurnSpeed *= 0.2f;
        }
            
        Quaternion turnRotation = Quaternion.Euler(0f, _turnInput * effectiveTurnSpeed * Time.fixedDeltaTime, 0f);

        _rb.MoveRotation(_rb.rotation * turnRotation);
        

        Vector3 forwardMovement = transform.forward * (_currentSpeed * Time.fixedDeltaTime); 
        //Looking into another way to get around it
        _rb.MovePosition(_rb.position + forwardMovement);
    }
}
