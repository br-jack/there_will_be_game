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
    public float CurrentSpeed => _currentSpeed;

    private bool _isGrounded = false;
    private float maxUpSpeedToPullToGround = 2.5f;
    private float maxTimeToPullToGround = 0.2f;
    private float _ignoreGroundTimer = 0f;
    private float _timerSinceOnGround = Mathf.Infinity;
    public bool IsGrounded => _isGrounded;

    private float speedPercent;
    
    public float GetCurrentSpeed()
    {
        return Mathf.Abs(_currentSpeed);
    }

    private float scaledJumpForce;

    private Rigidbody _rb;
    
    private float _throttleInput;
    private float _turnInput;
    private float _brakeInput;

    public float jumpForce = 5f; // originally 8f
    private bool _jumpHeld;
    public float fallMultiplier = 7.5f; // originally 2.5f
    public float lowJumpMultiplier = 4f; // originally 2f
    public LayerMask groundMask;
    [SerializeField] [Range(0f, 1f)] private float groundCheckDistance = 0.3f;

    [SerializeField] [Range(0f, 1f)] private float wallCheckDistance = 0.40f;
    [SerializeField] private LayerMask wallCheckMask;

    [SerializeField] [Range(0.5f, 3f)] private float bumpVelocityThreshold = 6.0f;
    private Vector3 _groundNormal = Vector3.up;

    private float _groundedTimer = 0f;
    [SerializeField] [Range(0.05f, 0.5f)] private float groundProbeRadius = 0.18f;

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

        float seperatingSpeed = Vector3.Dot(_rb.linearVelocity, _groundNormal);

        // Remove the component of speed perpendicular to the ground direction (if it's there).
        if (separatingSpeed > 0f)
        {
            velocity -= _groundNormal * separatingSpeed;
        }

        _isGrounded = false;
        _jumpPressed = false;
        _timerSinceOnGround = Mathf.infinity;
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

        if (Mathf.Abs(_throttleInput) < 0.01f && Mathf.Abs(_brakeInput) < 0.01f)
        {
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0, deceleration * Time.fixedDeltaTime);
        }
        
        _currentSpeed = Mathf.Clamp(_currentSpeed, -1.0f, maxSpeed);
    }

    private void HandleMovement()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.2f;
        bool grounded = CheckForGroundBelow(out RaycastHit groundHit, groundCheckDistance);
        _isGrounded = grounded;

        // If there's a small bump, there's only a small upwards velocity, so zero the velocity
        if (grounded && _rb.linearVelocity.y > 0f && _rb.linearVelocity.y < bumpVelocityThreshold)
        {
            Vector3 v = _rb.linearVelocity;
            v.y = 0f;
            _rb.linearVelocity = v;
        }

        //scale jumping to speed
        speedPercent = _currentSpeed / maxSpeed;
        
        Jump(grounded);
        
        if (grounded) //prevents accelerating and decelerating whilst midair
        {
            _groundedTimer += Time.fixedDeltaTime;
            _timerSinceOnGround = 0f;
            CalculateSpeed();
        } else
        {
            _groundedTimer = 0f;
            _timerSinceOnGround += Time.fixedDeltaTime;
        }

        Turn(grounded);
        
        Vector3 movementDirection = transform.forward;
        
        Vector3 wallDetectionRayOrigin = transform.position + Vector3.up * 1.0f;
        bool wallHit = Physics.Raycast(wallDetectionRayOrigin, movementDirection, out RaycastHit hit, wallCheckDistance, wallCheckMask);
        // Debug.DrawRay(wallDetectionRayOrigin, movementDirection * wallCheckDistance, Color.blue);
        //Prevent shooting up walls when slamming into one by redirecting velocity against it
        if (wallHit)
        {
            movementDirection = Vector3.ProjectOnPlane(movementDirection, hit.normal).normalized;
        }
        
        Vector3 desiredVelocity = movementDirection * _currentSpeed;

        Vector3 accel = (desiredVelocity - _rb.linearVelocity) / Time.fixedDeltaTime;
        accel.y = 0.0f;
        
        _rb.AddForce(accel, ForceMode.Acceleration);
        
        // _rb.linearVelocity += forwardMovement; //#Shay: doing this fixes clipping into walls but breaks everything else.
        // AccelerateTo(_rb, forwardMovement, 100.0f);
        //Looking into another way to get around it
        // _rb.MovePosition(_rb.position + forwardMovement);
    }

    private bool CheckForGroundBlow(out RaycastHit groundHit, float extraDistance)
    {
        groundHit = default;

        if (_ignoreGroundTimer > 0f) return false;

        Vector3 rayOrigin;
        
        if (_rb != null)
        {
            rayOrigin = _rb.worldCenterOfMass + Vector3.up * groundProbeStartHeight;
        }
        else
        {
            rayOrigin = _rb.worldCenterOfMass + Vector3.up * groundProbeStartHeight;
        }

        float castDistance = groundProbeStartHeight + extraDistance;

        if (RayToGroundBelow(rayOrigin, castDistance, out groundHit)) return true;

        // Checks for ground a little bit forward in the direction of movement.
        if (forwardGroundProbeOffset > 0f && Mathf.Abs(_currentSpeed) >= 0.05f)
        {
            Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);

            if (flatForward.sqrMagnitude > 0.0001f)
            {
                Vector3 forwardOffset = flatForward.normalized * (forwardGroundProbeOffset * Mathf.Sign(_currentSpeed));

                if (TryGroundCast(rayOrigin + forwardOffset, castDistance, out groundHit))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool RayToGroundBelow(Vector3 rayOrigin, float maxDistance, out RaycastHit groundHit)
    {
        if (Physics.SphereCast(rayOrigin, groundProbeRadius, Vector3.down, out groundHit, castDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            if (Vector3.Angle(groundHit.normal, Vector3.up) <= maxGroundAngle) return true;
        }

        groundHit = default;
        return false;
    }

    private bool PullToGround(out RaycastHit groundHit)
    {
        float upSpeed = Vector3.Dot(_rb.linearVelocity, groundHit.normal);

        if (_timerSinceOnGround > maxTimeToPullToGround)
        {
            return false
        }
        if (upSpeed > maxUpSpeedToPullToGround)
        {
            return false;
        }

        // Otherwise, it should pull the player to the ground.
        if (separatingSpeed > 0f)
        {
            _rb.linearVelocity -= groundHit.normal * separatingSpeed;
        }
        return true;
    }
}


