using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class horseMovementGaits : MonoBehaviour
{   
    //a bit of copy pasting from https://gist.github.com/Mike-Schvedov/b833a89b16e8b5a44df5707923169936
    //has happened here, not sure whether any actually remains, come back and check
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float chargeRequiredToRun = 10f;
    public float turnSpeed = 60f;
    public float acceleration = 2f;
    public float deceleration = 2f;
    //public float jumpHeight = 4f; no jumping for now
    public float currentRunCharge; //should be private, but i want to see it! 
    private float currentSpeed = 0f;
    private float gravity = -9.81f;
    private Vector3 verticalVelocity = Vector3.zero;
    
    private float _throttleInput;
    private float _turnInput;
    private float _brakeInput;
    //private bool _jumpButtonPressed;
    //private bool _jumpButtonHeld;

    private Vector3 _move;
    private CharacterController _cc;
    private Transform _tf;
    
    //These input functions (I believe) occur before Update(), 
    //so we can update _move in each of them and then apply it in update()
    public void OnJump(InputAction.CallbackContext context)
    {
        Debug.Log("currently a ground horse");
        //if (_cc.isGrounded) _move += 
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

    // Update is called once per frame
    void Update()
    {
        //identify a target speed to accelerate towards
        float targetSpeed = 0f;
        if ((_throttleInput - _brakeInput) > 0.1f)
            if (currentRunCharge > chargeRequiredToRun)
            {
                targetSpeed = runSpeed;
                //set charge to a little over needed to maintain run
                currentRunCharge = chargeRequiredToRun + 0.1f; 
            } else 
            {
                targetSpeed = walkSpeed; 
                //if within a range of walk speed, and accelerating, add charge
                if (walkSpeed - currentSpeed < 0.25f) currentRunCharge += Time.deltaTime; 
            }
        else if (currentRunCharge > 0) currentRunCharge -= Time.deltaTime;


        //turn transform 
        if (Mathf.Abs(_turnInput) > 0.1f)
        {
            _tf.Rotate(Vector3.up * _turnInput * turnSpeed * Time.deltaTime);
        }

        //accelerate
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed,
            (currentSpeed < targetSpeed ? acceleration : deceleration) * Time.deltaTime);
        
        //apply gravity, or a small down force if already on the ground
        verticalVelocity.y += (_cc.isGrounded ? -1f : gravity) * Time.deltaTime; 
        
        Vector3 move = transform.forward * currentSpeed + verticalVelocity;
        _cc.Move(move * Time.deltaTime);

        Debug.Log("Throttle: "+_throttleInput+", Turn: "+_turnInput+/*", JumpHeld: "+_jumpButtonHeld+", JumpPressed: "+_jumpButtonPressed+*/", brake: "+_brakeInput);
    }
}
