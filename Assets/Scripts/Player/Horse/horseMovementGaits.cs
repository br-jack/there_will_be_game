using System;
using System.Collections;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class horseMovementGaits : MonoBehaviour
{   
    [Header("Movement Variables")]
    public UnityEvent<gait> gaitChange;
    public UnityEvent jump;
    public float trotSpeed;
    public float canterSpeed;
    public float gallopSpeed;
    public float reverseSpeed;
    public float chargeRequiredToCanter;
    public float chargeRequiredToGallop;
    public float jumpHeight = 20f; 
    public float turnSpeedGroundedWalk;
    public float turnSpeedGroundedTrot;
    public float turnSpeedGroundedCanter;
    public float turnSpeedGroundedGallop;
    public float turnSpeedMidair;
    public float acceleration = 2f;
    public float deceleration = 6f;
    public float minTimeBetweenJumps; //for how long are you unable to jump after jumping
    public float chargeDecay;
    public float gravity;
    public static Action OnTutorialJump;

    [Header("Read-only in editor")]
    public float currentRunCharge; //should be private, but i want to see it! 
    public gait gait; //currently pointless, just to see gait in editor
    public float currentSpeed = 0f; //just to see in editor

    public bool canControl { get; set; } = true;
    public float tutorialSpeedMultiplier { get; set; } = 1f;
    public bool tutorialAllowJump { get; set; } = true;

    private float jumpLockedTime;

    [SerializeField] private Vector3 verticalVelocity = Vector3.zero; //serialised as bugged i think
    public Action jumpStarted;
    
    private float _throttleInput;
    private float _turnInput;
    private float _brakeInput;
    private bool _jumpInput;
    
    //private bool _jumpButtonPressed;
    //private bool _jumpButtonHeld;

    [Header("Drifting Settings")]
    public float driftLateralSlippage = 0.8f;
    public float driftSteerMultiplier = 1.5f;
    public float driftSpeedThreshold = 19f;
    public float driftFrictionMultiplyer = 0.8f;
    public Transform horseVisual; //not added yet

    private bool _isDrifting;
    private bool _driftInput; 
    private Vector3 _driftVelocity;

    private float _visualLean;

    private CharacterController _cc;
    private Transform _tf;
    
    //These input functions (I believe) occur before Update(), 
    //so we can update the _xInput variables in them, and then use those variables in Update()
    public void OnJump(InputAction.CallbackContext context)
    {
        _jumpInput = context.ReadValueAsButton();
    }

    public void onSteer(InputAction.CallbackContext context)
    {
        //Debug.Log("hi! steering");
        _turnInput = context.ReadValue<float>();

    }

    //not sure which of accelerate and brake will take priority!
    public void onAccelerate(InputAction.CallbackContext context)
    {
        //Debug.Log("accelerating!");
        _throttleInput = context.ReadValue<float>();
    }

    public void onBrake(InputAction.CallbackContext context)
    {
        //Debug.Log("braking!");
        _brakeInput = context.ReadValue<float>();
    }

    public void OnDrift(InputAction.CallbackContext context)
    {
        _driftInput = context.ReadValueAsButton();
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _cc = GetComponent<CharacterController>();
        _tf = GetComponent<Transform>();
    }

    public gait getCurrentGait() 
    {
        return gait;
    }

    public float getCurrentSpeed()
    {
        return currentSpeed;
    }

    private void setGait(gait newGait)
    {
        if (gait != newGait) 
        {
            gaitChange.Invoke(newGait);
            gait = newGait;
        }
    }

    private void setGait(float runCharge)
    {
        gait newGait;
        if (runCharge <= 0f) {
            if (currentSpeed < -0.1f) newGait = gait.reverse;
            else if (currentSpeed >  0.1f) newGait = gait.walking;
            else newGait = gait.still;
        }
        else if (runCharge <= chargeRequiredToCanter) newGait = gait.trotting;
        else if (runCharge <= chargeRequiredToGallop) newGait = gait.cantering;
        else newGait = gait.galloping;
        setGait(newGait);
    }

    private float getTurnSpeed()
    {
        
        if (!_cc.isGrounded) 
        {
            return turnSpeedMidair;
        }
        else if (gait == gait.walking || gait == gait.still)
        {
            return turnSpeedGroundedWalk;
        }
        else if (gait == gait.trotting)
        {
            return turnSpeedGroundedTrot;
        }
        else if (gait == gait.cantering)
        {
            return turnSpeedGroundedCanter;
        }
        else /*if (gait == gait.galloping)*/
        {
            return turnSpeedGroundedGallop;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!canControl)
        {
            currentSpeed = 0f;
            _throttleInput = 0f;
            _brakeInput = 0f;
            _turnInput = 0f;
            _jumpInput = false;
            verticalVelocity = Vector3.zero;
            return;
        }
        if (jumpLockedTime > 0.0f) {jumpLockedTime -= Time.deltaTime;} 
        else {jumpLockedTime = 0.0f;}

        //this code for calculating target speed has become very messy and hard to understand. I will try to rewrite it more cleanly soon!
        //identify a target speed to accelerate towards
        float targetSpeed = 0f; 
        if (((_throttleInput - _brakeInput) < -0.1f) && currentSpeed <= 0.1f) targetSpeed = reverseSpeed; //reversing
        if ((_throttleInput - _brakeInput) > 0.1f)  //accelerating
        {
            if (currentRunCharge > chargeRequiredToGallop)
            {   
                targetSpeed = gallopSpeed;
                //set charge to a little over needed to maintain gallop
                currentRunCharge = chargeRequiredToGallop + 0.1f; 

            } else if (currentRunCharge > chargeRequiredToCanter)
            {
                targetSpeed = canterSpeed;

                //if within a range of canter speed and grounded, add charge towards galloping
                if ((canterSpeed - currentSpeed < 0.25f) && _cc.isGrounded) currentRunCharge += Time.deltaTime;
                else if (_cc.isGrounded) currentRunCharge = chargeRequiredToCanter + 0.1f; 
                //^^otherwise, if grounded, reset charge to a little over needed to maintain canter

            } else 
            {
                targetSpeed = trotSpeed; 

                //if within a range of trot speed and grounded, add charge towards cantering
                if ((trotSpeed - currentSpeed < 0.25f) && _cc.isGrounded) currentRunCharge += Time.deltaTime;
                else if (_cc.isGrounded) currentRunCharge = 0.1f; 
                //^^otherwise reset charge
            }
        }
        else if (currentRunCharge > 0) {currentRunCharge -= Time.deltaTime * chargeDecay;}

        setGait(currentRunCharge); 
        
        float turnSpeed = getTurnSpeed();

        //turn transform 
        if (Mathf.Abs(_turnInput) > 0.1f)
        {
            _tf.Rotate(Vector3.up * _turnInput * turnSpeed * Time.deltaTime );
        }
        //should see if it would be fun for turning to decrease charge speed, so you have to 
        //go in a straight line to increase gait

        // apply tutorial movement limiter
        targetSpeed *= tutorialSpeedMultiplier;

        //accelerate
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed,
            (currentSpeed < targetSpeed ? acceleration : deceleration) * Time.deltaTime);
        

        //apply gravity if midair, or a small static push if on the ground
        verticalVelocity.y = _cc.isGrounded ? Mathf.Max(verticalVelocity.y-1f,-1f) : verticalVelocity.y + (gravity * Time.deltaTime); 
        if (_jumpInput && _cc.isGrounded && tutorialAllowJump) 
            {
                if (jumpLockedTime <= 0.0f)
                {
                    verticalVelocity.y += jumpHeight;
                    //not sure why we are invoking two things here! once i understand, ill try to do with just one
                    jumpStarted.Invoke();
                    jump.Invoke();
                    OnTutorialJump?.Invoke();
                    jumpLockedTime = minTimeBetweenJumps;
                }
                
                
            }

        HandleDriftLogic();
        ApplyMovement();

    }

    private void HandleDriftLogic()
    {
        if (_driftInput && _cc.isGrounded && currentSpeed > driftSpeedThreshold && Mathf.Abs(_turnInput) > 0.8f)
        {
            _isDrifting = true;
        }
        else if (_cc.isGrounded)
        {
            _isDrifting = false;
        }

        float finalTurnSpeed = getTurnSpeed();
        if (_isDrifting) finalTurnSpeed *= driftSteerMultiplier;
        
        _tf.Rotate(Vector3.up * _turnInput * finalTurnSpeed * Time.deltaTime);

        //purely visual (doesnt affect acc rotation but a fake visula horse instead for effect)
        float targetLean = _isDrifting ? _turnInput * 50f : 0f;
        _visualLean = Mathf.Lerp(_visualLean, targetLean, Time.deltaTime * 5f);
        
        if (horseVisual != null)
        {
            horseVisual.localRotation = Quaternion.Euler(0, _visualLean, 0);
        }
    }

    private void ApplyMovement()
    {
        Vector3 forwardMove = transform.forward * currentSpeed;

        if (_isDrifting && _cc.isGrounded)
        {
            Vector3 lateralDirection = transform.right * _turnInput;
            _driftVelocity = Vector3.Lerp(_driftVelocity, lateralDirection * currentSpeed * driftLateralSlippage, Time.deltaTime * 2f);
            forwardMove *= driftFrictionMultiplyer; //drifting reduces forward move due to friction
        }
        else
        {
            _driftVelocity = Vector3.Lerp(_driftVelocity, Vector3.zero, Time.deltaTime * 5f);
        }

        Vector3 finalMove = forwardMove + _driftVelocity + (Vector3.up * verticalVelocity.y);
        
        _cc.Move(finalMove * Time.deltaTime);
    }
}
