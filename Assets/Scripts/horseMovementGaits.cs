using UnityEngine;
using UnityEngine.InputSystem;

public class horseMovementGaits : MonoBehaviour
{
    private float _throttleInput;
    private float _turnInput;
    private float _brakeInput;
    private bool _jumpButtonPressed;
    private bool _jumpButtonHeld;

    private Rigidbody _rb;
    

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
        Debug.Log("hi! steering");
        _turnInput = context.ReadValue<float>();
        _rb.AddRelativeForce(new Vector3(_turnInput,0,0));

    }

    public void onAccelerate(InputAction.CallbackContext context)
    {
        Debug.Log("accelerating!");
        _throttleInput = context.ReadValue<float>();
        _rb.AddRelativeForce(new Vector3(0,0,_throttleInput));
    }

    public void onBrake(InputAction.CallbackContext context)
    {
        Debug.Log("braking!");
        _brakeInput = context.ReadValue<float>();
        _rb.AddRelativeForce(new Vector3(0,0,-_brakeInput));
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Throttle: "+_throttleInput+", Turn: "+_turnInput+", JumpHeld: "+_jumpButtonHeld+", JumpPressed: "+_jumpButtonPressed+", brake: "+_brakeInput);
    }
}
