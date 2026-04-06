using UnityEngine;
using UnityEngine.InputSystem;

public class horseMovementGaits : MonoBehaviour
{
    private float _throttleInput;
    private float _turnInput;
    private float _brakeInput;
    private bool _jumpButtonPressed;
    private bool _jumpButtonHeld;
    

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

    }

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
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Debug.Log("Throttle: "+_throttleInput+", Turn: "+_turnInput+", JumpHeld: "+_jumpButtonHeld+", JumpPressed: "+_jumpButtonPressed+", brake: "+_brakeInput);
    }
}
