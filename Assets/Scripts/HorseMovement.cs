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
    
    [SerializeField] private float _currentSpeed = 0f;

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

    [SerializeField] [Range(0f, 1f)] private float wallCheckDistance = 0.40f;
    [SerializeField] private LayerMask wallCheckMask;

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
            _rb.linearVelocity += Physics.gravity * ((fallMultiplier - 1) * Time.fixedDeltaTime); 
        }
        else if (_rb.linearVelocity.y > 0 && !_jumpHeld) //smaller jump when jump button not held
        { 
            _rb.linearVelocity += Physics.gravity * ((lowJumpMultiplier - 1) * Time.fixedDeltaTime); 
        }
    }

    private void Jump(bool grounded)
    {
        /*Not currently using this:
        scaledJumpForce = jumpForce * speedPercent * 1.2f;
        scaledJumpForce = Mathf.Clamp(scaledJumpForce, 0.0f, jumpForce);*/

        if (_jumpPressed && grounded && _groundedTimer > 0.1f && _currentSpeed > 2f)
        {
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        _jumpPressed = false;
    }

    private void Turn(bool grounded)
    {
        //restrict turning more at higher speeds
        float effectiveTurnSpeed = Mathf.Lerp(turnSpeedAtZero, turnSpeed, speedPercent);

        if (!grounded)
        {
            effectiveTurnSpeed *= 0.2f;
        }
        
        Quaternion turnRotation = Quaternion.Euler(0f, _turnInput * effectiveTurnSpeed * Time.fixedDeltaTime, 0f);

        _rb.MoveRotation(_rb.rotation * turnRotation);
    }

    private void CalculateSpeed()
    {
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
    }

    private void HandleMovement()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.2f;
        bool grounded = Physics.Raycast(rayOrigin, Vector3.down, wallCheckDistance, groundMask);

        //scale jumping to speed
        speedPercent = _currentSpeed / maxSpeed;
        
        Jump(grounded);
        
        if (grounded) //prevents accelerating and decelerating whilst midair
        {
            _groundedTimer += Time.fixedDeltaTime;

            CalculateSpeed();
        } else
        {
            _groundedTimer = 0f;
        }

        Turn(grounded);
        
        // bool wallHit = Physics.SphereCast(transform.position + Vector3.up * 4.0f, 0.2f, transform.forward, out _, wallCheckDistance, wallCheckMask);
        // if (wallHit)
        // {
        //     _currentSpeed = 0;
        // }

        Vector3 forwardMovement = transform.forward * _currentSpeed;

        Vector3 accel = (forwardMovement - _rb.linearVelocity) / Time.fixedDeltaTime;
        accel.y = 0.0f;
        
        _rb.AddForce(accel, ForceMode.Acceleration);
        
        // _rb.linearVelocity += forwardMovement; //#Shay: doing this fixes clipping into walls but breaks everything else.
        // AccelerateTo(_rb, forwardMovement, 100.0f);
        //Looking into another way to get around it
        // _rb.MovePosition(_rb.position + forwardMovement);
    }
}
