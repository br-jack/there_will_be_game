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
    //a bit of copy pasting from https://gist.github.com/Mike-Schvedov/b833a89b16e8b5a44df5707923169936
    //has happened here, not sure whether any actually remains, come back and check
    public UnityEvent<gait> gaitChange;
    public UnityEvent jump;
    public float trotSpeed;
    public float canterSpeed;
    public float gallopSpeed;
    public float chargeRequiredToCanter = 10f;
    public float chargeRequiredToGallop =20f;
    public float jumpHeight = 4f; 
    public float turnSpeed = 60f;
    public float acceleration = 2f;
    public float deceleration = 6f;
    public float minTimeBetweenJumps = 0.25f; //for how long are you unable to jump after jumping
    public float chargeDecay;
    public float currentRunCharge; //should be private, but i want to see it! 
    public gait gait; //currently pointless, just to see gait in editor
    public float currentSpeed = 0f; //just to see in editor
    public float gravity = -9.81f; 

    private float jumpLockedTime;

    private Vector3 verticalVelocity = Vector3.zero;
    public Action jumpStarted;
    
    private float _throttleInput;
    private float _turnInput;
    private float _brakeInput;
    private bool _jumpInput;
    //private bool _jumpButtonPressed;
    //private bool _jumpButtonHeld;

    private CharacterController _cc;
    private Transform _tf;
    
    //These input functions (I believe) occur before Update(), 
    //so we can update _move in each of them and then apply it in update()
    public void OnJump(InputAction.CallbackContext context)
    {
        _jumpInput = context.ReadValue<bool>();
    }

    public void onSteer(InputAction.CallbackContext context)
    {
        Debug.Log("hi! steering");
        _turnInput = context.ReadValue<float>();

    }

    //not sure which of accelerate and brake will take priority!
    public void onAccelerate(InputAction.CallbackContext context)
    {
        Debug.Log("accelerating!");
        _throttleInput = context.ReadValue<float>();
    }

    public void onBrake(InputAction.CallbackContext context)
    {
        Debug.Log("braking!");
        _brakeInput = context.ReadValue<float>();
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
        if (runCharge <= 0f) newGait = gait.walking;
        else if (runCharge <= chargeRequiredToCanter) newGait = gait.trotting;
        else if (runCharge <= chargeRequiredToGallop) newGait = gait.cantering;
        else newGait = gait.galloping;
        setGait(newGait);
    }

    // Update is called once per frame
    void Update()
    {       
        if (jumpLockedTime > 0.0f) {jumpLockedTime -= Time.deltaTime;}

        //identify a target speed to accelerate towards
        float targetSpeed = 0f;
        if ((_throttleInput - _brakeInput) > 0.1f)  
        {
            if (currentRunCharge > chargeRequiredToGallop)
            {   
                targetSpeed = gallopSpeed;
                //set charge to a little over needed to maintain gallop
                currentRunCharge = chargeRequiredToGallop + 0.1f; 

            } else if (currentRunCharge > chargeRequiredToCanter)
            {
                targetSpeed = canterSpeed;

                //if within a range of canter speed, and accelerating, add charge towards galloping
                if (canterSpeed - currentSpeed < 0.25f) currentRunCharge += Time.deltaTime;
                else currentRunCharge = chargeRequiredToCanter + 0.1f; 
                //^^otherwise set charge to a little over needed to maintain canter

            } else 
            {
                targetSpeed = trotSpeed; 

                //if within a range of walk speed, and accelerating, add charge towards cantering
                if (trotSpeed - currentSpeed < 0.25f) currentRunCharge += Time.deltaTime; 
            }
        }
        else if (currentRunCharge > 0) {currentRunCharge -= Time.deltaTime * chargeDecay;}

        setGait(currentRunCharge); 
        

        //turn transform 
        if (Mathf.Abs(_turnInput) > 0.1f)
        {
            _tf.Rotate(Vector3.up * _turnInput * turnSpeed * Time.deltaTime);
        }
        //should see if it would be fun for turning to decrease charge speed, so you have to 
        //go in a straight line to increase gait

        //accelerate
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed,
            (currentSpeed < targetSpeed ? acceleration : deceleration) * Time.deltaTime);
        
        //apply gravity, or a small down force if already on the ground
        verticalVelocity.y += (_cc.isGrounded ? -1f : gravity) * Time.deltaTime; 
        if (_jumpInput && _cc.isGrounded) 
            {
                if (jumpLockedTime == 0)
                {
                    verticalVelocity.y += jumpHeight;
                    //not sure why we are invoking two things here! once i understand, ill try to do with just one
                    jumpStarted.Invoke();
                    jump.Invoke();
                    jumpLockedTime = minTimeBetweenJumps;
                }
                
                
            }
        
        Vector3 move = transform.forward * currentSpeed + verticalVelocity;
        _cc.Move(move * Time.deltaTime);

    }
}
