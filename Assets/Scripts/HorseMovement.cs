using NUnit.Framework;
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
    public bool IsGrounded => _isGrounded;

    private float speedPercent;
    
    public float GetCurrentSpeed()
    {
        return Mathf.Abs(_currentSpeed);
    }

    private float scaledJumpForce;

    private Rigidbody _rb;

    public Transform horseVisual;
    
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

    //drifting
    private bool _isDrifting;

    public float driftLateralFriction = 0.3f;   // how slippery sideways movement becomes
    public float normalLateralFriction = 1.0f;  // normal grip
    public float driftAngularBoost = 2.0f;      // extra rotation force during drift
    public float driftKickoutForce = 5f;        // sideways push
    public float driftSteerThreshold = 0.8f;    // how hard the player must steer
    public float driftSpeedThreshold = 20f;      // minimum speed to drift
    private float driftTimer = 0f; // hard turn must be held to drift

    private float _currentLean = 0f;

    
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

    private void ApplyDriftPhysics()
    {
        Vector3 localVel = transform.InverseTransformDirection(_rb.linearVelocity);
        localVel.x *= driftLateralFriction;
        _rb.linearVelocity = transform.TransformDirection(localVel);

        _rb.AddForce(transform.right * _turnInput * driftKickoutForce, ForceMode.Acceleration);

        _rb.AddTorque(Vector3.up * _turnInput * driftAngularBoost, ForceMode.Acceleration);
    }

    private void RestoreNormalGrip()
    {
        Vector3 localVel = transform.InverseTransformDirection(_rb.linearVelocity);
        localVel.x *= normalLateralFriction;
        _rb.linearVelocity = transform.TransformDirection(localVel);
    }

    private void Awake() 
    {
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        SkinnedMeshRenderer _hm = player.GetComponentInChildren<SkinnedMeshRenderer>();
    }

    private void Update()
    {
        //_jumpHeld = Input.GetButton("Jump");
    }

    private void FixedUpdate()  
    {
        bool grounded = Physics.Raycast(
            transform.position + Vector3.up * 0.2f,
            Vector3.down,
            groundCheckDistance,
            groundMask
        );

        HandleMovement();

        float targetLean;
        if (_isDrifting && grounded)
        {
            ApplyDriftPhysics();
            targetLean = _turnInput * 30f;

            if (_brakeInput > 0f)
            {
                _currentSpeed -= brake * 0.7f * Time.fixedDeltaTime;
                driftLateralFriction = Mathf.Lerp(driftLateralFriction, 0.6f, Time.fixedDeltaTime * 2f);
            }
            else
            {
                driftLateralFriction = 0.3f;
            }

            if (_throttleInput == 0f && _brakeInput == 0f)
            {
                _currentSpeed -= deceleration * 0.5f * Time.fixedDeltaTime;
                _currentLean = Mathf.Lerp(_currentLean, 0f, Time.deltaTime * 0.8f);
                driftLateralFriction = 0.3f;
            }
        }
        else if (grounded)
        {
            RestoreNormalGrip();
            targetLean = 0f;
        }
        else
        {
            targetLean = _currentLean;
        }

        float driftAngle = _turnInput;
        if (_isDrifting)
        {
            if (Mathf.Abs(_turnInput) > 0.1f)
                _currentLean = driftAngle * 30f;

            if (_brakeInput > 0f)
                _currentLean = Mathf.Lerp(_currentLean, 0f, Time.deltaTime * 1.5f);
        }

        _currentLean = Mathf.Lerp(_currentLean, targetLean, Time.deltaTime * 5f);
        Quaternion leanRot = Quaternion.Euler(0f, targetLean, 0f);
        horseVisual.localRotation = Quaternion.Lerp(
            horseVisual.localRotation,
            leanRot,
            Time.deltaTime * 5f
        );

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
        _isGrounded = grounded;

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

        if (Mathf.Abs(_turnInput) > driftSteerThreshold && _currentSpeed > driftSpeedThreshold)
        {
            driftTimer += Time.fixedDeltaTime;
            if (driftTimer > 0.2f) _isDrifting = true;
        }
        else if (grounded)
        {
            driftTimer = 0f;
            _isDrifting = false;
        }

        if (_isDrifting)
        {
            if (_currentSpeed < driftSpeedThreshold * 0.6f && Mathf.Abs(_turnInput) < 0.4f) //drift stops as turn relaxes
            {
                _isDrifting = false;
                driftTimer = 0f;
            }
        }
            
        Quaternion turnRotation = Quaternion.Euler(0f, _turnInput * effectiveTurnSpeed * Time.fixedDeltaTime, 0f);
        //_rb.MoveRotation(_rb.rotation * turnRotation);
        _rb.AddTorque(Vector3.up * _turnInput * effectiveTurnSpeed * 0.03f, ForceMode.Acceleration);
        //_rb.AddTorque(Vector3.up * _turnInput * steerTorque, ForceMode.Acceleration);       

        //Vector3 forwardMovement = transform.forward * (_currentSpeed * Time.fixedDeltaTime);
        //_rb.MovePosition(_rb.position + forwardMovement);        
        Vector3 vel = _rb.linearVelocity;
        Vector3 forwardVel = transform.forward * _currentSpeed;
        vel.x = forwardVel.x;
        vel.z = forwardVel.z;
        _rb.linearVelocity = vel;

    }
}
