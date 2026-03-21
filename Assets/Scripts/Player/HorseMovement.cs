
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class HorseMovement : MonoBehaviour
{
    public float acceleration = 12f;
    public float deceleration = 2f; //ambient deceleration when no acceleration or braking/reverse

    public float brake = 20f;
    public float maxSpeed = 30f;
    public float steerTorque = 10f;

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

    public Action jumpStarted;

    public Transform horseVisual;
    
    private float _throttleInput;
    private float _turnInput;
    private float _brakeInput;

    public float jumpForce = 5f; // originally 8f
    private bool _jumpButtonPressed;
    private bool _jumpButtonHeld;
    public float fallMultiplier = 7.5f; // originally 2.5f
    public float lowJumpMultiplier = 4f; // originally 2f
    public LayerMask groundMask;
    [SerializeField] [Range(0f, 1f)] private float groundCheckDistance = 0.3f;
    [SerializeField] [Range(0.05f, 0.5f)] private float groundProbeRadius = 0.18f;
    [SerializeField] [Range(0.05f, 0.75f)] private float groundProbeStartHeight = 0.25f;
    [SerializeField] [Range(0f, 0.5f)] private float forwardGroundProbeOffset = 0.18f;
    [SerializeField] [Range(0f, 75f)] private float maxGroundAngle = 60f;
    [SerializeField] [Range(0f, 100f)] private float pull = 45f;

    [SerializeField] [Range(0f, 1f)] private float wallCheckDistance = 0.40f;
    [SerializeField] private LayerMask wallCheckMask;

    [SerializeField] [Range(0.5f, 3f)] private float bumpVelocityThreshold = 6.0f;
    private Vector3 _groundNormal = Vector3.up;

    private float _groundedTimer = 0f;
    
    //drifting
    private bool _isDrifting;
    private bool _driftButtonPressed;
    private float _driftTimer = 0f; // hard turn must be held to drift

    private float _currentLean = 0f;

    [Serializable]
    public struct DriftSettings
    {
        public float driftLateralFriction;   // how slippery sideways movement becomes
        public float normalLateralFriction;  // normal grip
        public float driftAngularBoost;      // extra rotation force during drift
        public float driftKickoutForce;        // sideways push
        public float driftSteerThreshold;    // how hard the player must steer
        public float driftSpeedThreshold;      // minimum speed to drift
    }
    
    [SerializeField] private DriftSettings driftSettings;

    
    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 moveVector = context.ReadValue<Vector2>();
        _turnInput = moveVector.x;
        _throttleInput = moveVector.y;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _jumpButtonPressed = true;
            _jumpButtonHeld = true;
        }

        if (context.canceled) 
        {
            _jumpButtonHeld = false;
        }
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

    public void onDrift(InputAction.CallbackContext context)
    {
        if (context.performed)
            _driftButtonPressed = true;
        if (context.canceled) 
            _driftButtonPressed = false;
    }

    private void ApplyDriftPhysics()
    {
        Vector3 localVel = transform.InverseTransformDirection(_rb.linearVelocity);
        localVel.x *= driftSettings.driftLateralFriction;
        _rb.linearVelocity = transform.TransformDirection(localVel);

        _rb.AddForce(transform.right * _turnInput * driftSettings.driftKickoutForce, ForceMode.Acceleration);
    }

    private void RestoreNormalGrip()
    {
        Vector3 localVel = transform.InverseTransformDirection(_rb.linearVelocity);
        localVel.x *= driftSettings.normalLateralFriction;
        _rb.linearVelocity = transform.TransformDirection(localVel);
    }

    private void Awake() 
    {
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
    }

    private void FixedUpdate()  
    {
        if (_ignoreGroundTimer > 0f)
        {
            _ignoreGroundTimer -= Time.fixedDeltaTime;
        }

        HandleMovement();
        
        float targetLean;
        if (_isDrifting && _isGrounded  && _driftButtonPressed)
        {
            ApplyDriftPhysics();
            targetLean = _turnInput * 30f;

            float driftAngle = _turnInput;
            
            if (Mathf.Abs(_turnInput) > 0.1f)
                _currentLean = driftAngle * 30f;

            if (_brakeInput > 0f)
            {
                _currentSpeed -= brake * 0.7f * Time.fixedDeltaTime;
                driftSettings.driftLateralFriction = Mathf.Lerp(driftSettings.driftLateralFriction, 0.6f, Time.fixedDeltaTime * 2f);
                _currentLean = Mathf.Lerp(_currentLean, 0f, Time.deltaTime * 1.5f);
            }
            else
            {
                driftSettings.driftLateralFriction = 0.3f;
            }

            if (_throttleInput == 0f && _brakeInput == 0f)
            {
                _currentSpeed -= deceleration * 0.5f * Time.fixedDeltaTime;
                _currentLean = Mathf.Lerp(_currentLean, 0f, Time.deltaTime * 0.8f);
                driftSettings.driftLateralFriction = 0.3f;
            }
        }
        else if (_isGrounded)
        {
            RestoreNormalGrip();
            targetLean = 0f;
        }
        else
        {
            targetLean = _currentLean;
        }

        _currentLean = Mathf.Lerp(_currentLean, targetLean, Time.deltaTime * 5f);
        Quaternion leanRot = Quaternion.Euler(0f, targetLean, 0f);
        horseVisual.localRotation = Quaternion.Lerp(
            horseVisual.localRotation,
            leanRot,
            Time.deltaTime * 5f
        );

        if (!_isGrounded && _rb.linearVelocity.y < 0f) //speed up fall for feel  
        { 
            _rb.linearVelocity += Physics.gravity * ((fallMultiplier - 1f) * Time.fixedDeltaTime); 
        }
        else if (!_isGrounded && _rb.linearVelocity.y > 0f && !_jumpButtonHeld) //smaller jump when jump button not held
        { 
            _rb.linearVelocity += Physics.gravity * ((lowJumpMultiplier - 1f) * Time.fixedDeltaTime); 
        }
    }

    private bool Jump(bool grounded)
    {
        /*Not currently using this:
        scaledJumpForce = jumpForce * speedPercent * 1.2f;
        scaledJumpForce = Mathf.Clamp(scaledJumpForce, 0.0f, jumpForce);*/

        if (!_jumpButtonPressed)
        {
            return false;
        }

        if (!grounded || _groundedTimer <= 0.1f)
        {
            _jumpButtonPressed = false;
            return false;
        }
        
        jumpStarted?.Invoke();

        Vector3 velocity = _rb.linearVelocity;
        float seperatingSpeed = Vector3.Dot(velocity, _groundNormal);

        // Remove the component of speed perpendicular to the ground direction (if it's there).
        if (seperatingSpeed > 0f)
        {
            velocity -= _groundNormal * seperatingSpeed;
        }

        _rb.linearVelocity = velocity;
        _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        _isGrounded = false;
        _groundedTimer = 0f;
        _jumpButtonPressed = false;
        _ignoreGroundTimer = 0.15f;
        _timerSinceOnGround = Mathf.Infinity;

        return true;
    }

    private void Turn(bool grounded)
    {
        //restrict turning more at higher speeds
        float effectiveTurnSpeed = Mathf.Lerp(turnSpeedAtZero, turnSpeed, speedPercent);

        if (!grounded)
        {
            effectiveTurnSpeed *= 0.2f;
        }

        if (_isDrifting)
        {
            effectiveTurnSpeed *= 1.2f;
        }
        
        Quaternion turnRotation = Quaternion.Euler(0f, _turnInput * effectiveTurnSpeed * Time.fixedDeltaTime, 0f);
        _rb.MoveRotation(_rb.rotation * turnRotation);

        //_rb.AddTorque(Vector3.up * _turnInput * effectiveTurnSpeed * 0.05f, ForceMode.Acceleration); try only have torque added for drifting not when turning normally
    }

    // private void CalculateSpeed()
    // {
    //     if (_throttleInput > 0.0f)
    //     {
    //         _currentSpeed += acceleration * _throttleInput * Time.fixedDeltaTime;
    //     }
    //     else if (_throttleInput < 0.0f)
    //     {
    //         _currentSpeed -= deceleration * -_throttleInput * Time.fixedDeltaTime;
    //     }

    //     _currentSpeed -= brake * _brakeInput * Time.fixedDeltaTime;

    //     if (Mathf.Abs(_throttleInput) < 0.01f && Mathf.Abs(_brakeInput) < 0.01f)
    //     {
    //         _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0, deceleration * Time.fixedDeltaTime);
    //     }
        
    //     _currentSpeed = Mathf.Clamp(_currentSpeed, -1.0f, maxSpeed);
    // }

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
        
        _currentSpeed = Mathf.Clamp(_currentSpeed, -3.0f, maxSpeed);
    }

    private void HandleMovement()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.2f;
        bool grounded = CheckForGroundBelow(out RaycastHit groundHit, groundCheckDistance);

        if (!grounded)
        {
            grounded = PullToGround(out groundHit);
        }

        _isGrounded = grounded;
        _groundNormal = grounded ? groundHit.normal : Vector3.up;

        // If there's a small bump, there's only a small upwards velocity, so zero the velocity
        if (grounded && _rb.linearVelocity.y > 0f && _rb.linearVelocity.y < bumpVelocityThreshold)
        {
            Vector3 v = _rb.linearVelocity;
            v.y = 0f;
            _rb.linearVelocity = v;
        }

        //scale jumping to speed
        speedPercent = _currentSpeed / maxSpeed;
        
        bool JumpedThisFrame = Jump(grounded);

        if (grounded && !JumpedThisFrame) //prevents accelerating and decelerating whilst midair
        {
            _groundedTimer += Time.fixedDeltaTime;
            _timerSinceOnGround = 0f;
            
            CalculateSpeed();
        } 
        else if (!grounded)
        {
            _groundedTimer = 0f;
            _timerSinceOnGround += Time.fixedDeltaTime;
        }

        grounded = grounded && !JumpedThisFrame;
        _isGrounded = grounded;

        if (Mathf.Abs(_turnInput) > driftSettings.driftSteerThreshold && _currentSpeed > driftSettings.driftSpeedThreshold  && _driftButtonPressed)
        {
            _driftTimer += Time.fixedDeltaTime;
            if (_driftTimer > 0.2f) _isDrifting = true;
        }
        else if (grounded)
        {
            _driftTimer = 0f;
            _isDrifting = false;
        }

        if (_isDrifting && _driftButtonPressed)
        {
            if (_currentSpeed < driftSettings.driftSpeedThreshold * 0.6f && Mathf.Abs(_turnInput) < 0.4f) //drift stops as turn relaxes
            {
                _isDrifting = false;
                _driftTimer = 0f;
            }
        }

        Turn(grounded);
        
        Vector3 movementDirection = transform.forward;

        // If grounded, movement is projected along the slope rather than through it.
        if (grounded)
        {
            movementDirection = Vector3.ProjectOnPlane(movementDirection, _groundNormal);
            
            if (movementDirection.sqrMagnitude < 0.001f)
            {
                movementDirection = Vector3.Cross(transform.right, _groundNormal);
            }

            movementDirection = movementDirection.normalized;
        }
        
        Vector3 wallDetectionRayOrigin = transform.position + Vector3.up * 1.0f;
        
        bool wallHit = Physics.Raycast(wallDetectionRayOrigin, movementDirection, out RaycastHit hit, wallCheckDistance, wallCheckMask);
        // Debug.DrawRay(wallDetectionRayOrigin, movementDirection * wallCheckDistance, Color.blue);
        //Prevent shooting up walls when slamming into one by redirecting velocity against it
        
        Vector3 directionOfMovement = Vector3.up;
        if (grounded) directionOfMovement = _groundNormal;
        
        if (wallHit)
        {
            // // If we hit the wall, move along it instead of through it.
            // Vector3 slideDirection = Vector3.ProjectOnPlane(movementDirection, hit.normal);

            // slideDirection = Vector3.ProjectOnPlane(slideDirection, directionOfMovement);

            // // If there's nowhere to move, stop trying to move.
            // if (slideDirection.sqrMagnitude < 0.0001f) return;

            // movementDirection = slideDirection.normalized;
            Vector3 planeVelocity = Vector3.ProjectOnPlane(_rb.linearVelocity, directionOfMovement);
            float speedIntoWall = Vector3.Dot(planeVelocity, -hit.normal);

            if (speedIntoWall > 0f)
            {
                Vector3 wallNormal = hit.normal.normalized;
                float intoWallAmount = Vector3.Dot(planeVelocity, -wallNormal);
                if (intoWallAmount > 0f) {
                    Vector3 velocityIntoWall = -wallNormal * intoWallAmount;
                    _rb.linearVelocity -= velocityIntoWall;
                }
                _currentSpeed = 0f;
            }
            movementDirection = Vector3.zero;


        }
        
        Vector3 desiredVelocity = movementDirection * _currentSpeed;
        Vector3 currentPlanarVelocity = Vector3.ProjectOnPlane(_rb.linearVelocity, directionOfMovement);
        Vector3 accel = (desiredVelocity - currentPlanarVelocity) / Time.fixedDeltaTime;
        accel.y = 0.0f;
        _rb.AddForce(accel, ForceMode.Acceleration);

        // Extra ground adhesion to stop the horse jumping when it reaches small steps etc.
        if (grounded)
        {
            float upSpeed = Vector3.Dot(_rb.linearVelocity, _groundNormal);

            if (upSpeed > 0f && upSpeed < bumpVelocityThreshold)
            {
                _rb.linearVelocity -= _groundNormal * upSpeed;
            }

            _rb.AddForce(-_groundNormal * pull, ForceMode.Acceleration);
        }
        
        // _rb.linearVelocity += forwardMovement; //#Shay: doing this fixes clipping into walls but breaks everything else.
        // AccelerateTo(_rb, forwardMovement, 100.0f);
        //Looking into another way to get around it
        // _rb.MovePosition(_rb.position + forwardMovement);
    }

    private bool CheckForGroundBelow(out RaycastHit groundHit, float extraDistance)
    {
        groundHit = default;

        if (_ignoreGroundTimer > 0f) return false;

        Vector3 rayOrigin;
        
        if (_rb != null)
        {
            // Ensure the probe starts above the lowest point even when the centre of mass is offset.
            float comAdjustment = Mathf.Max(0f, -_rb.centerOfMass.y);
            rayOrigin = _rb.worldCenterOfMass + Vector3.up * (groundProbeStartHeight + comAdjustment);
        }
        else
        {
            rayOrigin = transform.position + Vector3.up * groundProbeStartHeight;
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

                if (RayToGroundBelow(rayOrigin + forwardOffset, castDistance, out groundHit))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool RayToGroundBelow(Vector3 rayOrigin, float maxDistance, out RaycastHit groundHit)
    {
        if (Physics.SphereCast(rayOrigin, groundProbeRadius, Vector3.down, out groundHit, maxDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            if (Vector3.Angle(groundHit.normal, Vector3.up) <= maxGroundAngle) return true;
        }

        if (Physics.Raycast(rayOrigin, Vector3.down, out groundHit, maxDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            if (Vector3.Angle(groundHit.normal, Vector3.up) <= maxGroundAngle) return true;
        }

        groundHit = default;
        return false;
    }

    private bool PullToGround(out RaycastHit groundHit)
    {
        groundHit = default;

        if (_timerSinceOnGround > maxTimeToPullToGround) return false;
        if (!CheckForGroundBelow(out groundHit, groundCheckDistance * 3f)) return false;

        float seperatingSpeed = Vector3.Dot(_rb.linearVelocity, groundHit.normal);

        if (seperatingSpeed > maxUpSpeedToPullToGround) return false;

        // Otherwise, it should pull the player to the ground.
        if (seperatingSpeed > 0f)
        {
            _rb.linearVelocity -= groundHit.normal * seperatingSpeed;
        }
        return true;
    }
}
