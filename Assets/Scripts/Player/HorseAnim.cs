using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class HorseAnim : MonoBehaviour
{
    [SerializeField] private float speed;
    private Animator _horseAnimator;
    private horseMovementGaits _horseMovementGaits;
    private CharacterController _cc;
    private int framesSinceGround = 0;
    private bool currentlyJumping = false;

    public void animateJump()
    {
        _horseAnimator.SetTrigger("Jump");
        Debug.LogError("horszz");
        currentlyJumping = true;
    }

    void Awake()
    {
        
        _horseAnimator = GetComponent<Animator>();
        _horseMovementGaits = GetComponentInParent<horseMovementGaits>();
        _cc = GetComponentInParent<CharacterController>();

        if (_horseAnimator.enabled) //this is here because the animator panel is bugged in this unity version :(
        {
            _horseAnimator.Rebind();
            _horseAnimator.Update(0);
        }
    }

    void OnEnable()
    {
        _horseMovementGaits.jumpStarted += animateJump;
    }

    
    void OnDisable()
    {
        _horseMovementGaits.jumpStarted -= animateJump;
    }

    void ResetAll()
    {
        _horseAnimator.ResetTrigger("Gallop0");
        _horseAnimator.ResetTrigger("Gallop3");
    }
    

    // Update is called once per frame
    void Update()
    {
       // Action jump = _horseMovementGaits.jumpStarted;

        if(_horseAnimator != null)
        {

            speed = _horseMovementGaits.getCurrentSpeed();
            _horseAnimator.SetFloat("Speed", speed);

            //Debug.LogError("horszz");
            bool grounded = _cc.isGrounded;

            //we probably need to reset the jump trigger if we didn't jump. idk how it works
            // if ( && grounded)
            // {
            //     _horseAnimator.SetTrigger("Jump");
            // }
            // else
            // {
            //     _horseAnimator.ResetTrigger("Jump");
            // }
            _horseAnimator.SetBool("Grounded",grounded);
            if (grounded)
            {
                framesSinceGround = 0;
                _horseAnimator.SetBool("Falling",!grounded);
                currentlyJumping = false;
            }
            else
            {
                framesSinceGround++;
                if(framesSinceGround > 10 && !currentlyJumping)
                {
                    _horseAnimator.SetBool("Falling",!grounded);
                }
            }
                

            //replaced speed checks with gaits
            if (_horseMovementGaits.getCurrentGait() == gait.galloping 
            || _horseMovementGaits.getCurrentGait() == gait.cantering)
            {
                _horseAnimator.SetInteger("Gait",3);
                //Debug.LogError("horszz");
            }
            else if (_horseMovementGaits.getCurrentGait() == gait.trotting){
                _horseAnimator.SetInteger("Gait",2);

            }
            //should be check for walking gait here, but 'still' gait is currently not implemented
            else if (_cc.velocity.magnitude > 0.1f) 
            {
                _horseAnimator.SetInteger("Gait",1);

                
                //Debug.LogError("hors");
            }
            else
            {
                _horseAnimator.SetInteger("Gait",0);

                //horseAnimator.ResetTrigger("Gallop1");

            }
        }
        else
        {
            Debug.LogError("man there's no animator for the horse (HorseAnim)");
        }
    }
}
