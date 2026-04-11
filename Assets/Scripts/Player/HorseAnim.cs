using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class HorseAnim : MonoBehaviour
{
    [SerializeField] private float speed;
    private Animator _horseAnimator;
    private HorseMovement _horseMovement;
    void Awake()
    {
        _horseAnimator = GetComponent<Animator>();
        _horseMovement = GetComponentInParent<HorseMovement>();
        if (_horseAnimator.enabled) //this is here because the animator panel is bugged in this unity version :(
        {
            _horseAnimator.Rebind();
            _horseAnimator.Update(0);
        } 
    }
    void ResetAll()
    {
        _horseAnimator.ResetTrigger("Gallop0");
        _horseAnimator.ResetTrigger("Gallop3");
    }

    // Update is called once per frame
    void Update()
    {
        Action jump = _horseMovement.jumpStarted;

        if(_horseAnimator != null)
        {

            speed = _horseMovement.GetCurrentSpeed();
            _horseAnimator.SetFloat("Speed", speed);
            //Debug.LogError("horszz");
            bool grounded = _horseMovement.CheckForGroundBelow(out RaycastHit groundHit, 1.0f);

            if (_horseMovement.JumpButtonPressed && grounded)
            {
                _horseAnimator.SetTrigger("Jump");
            }
            else
            {
                _horseAnimator.ResetTrigger("Jump");
            }
            _horseAnimator.SetBool("Grounded",grounded);
            if (speed  > 19)
            {
                _horseAnimator.SetTrigger("Gallop3");
                _horseAnimator.ResetTrigger("Gallop1");
                _horseAnimator.ResetTrigger("Gallop0");
                //Debug.LogError("horszz");
            }
            else if (speed > 8){
                _horseAnimator.SetTrigger("Gallop1");
                _horseAnimator.ResetTrigger("Gallop3");
                _horseAnimator.ResetTrigger("Gallop0");
            }
            else if (speed > 1)
            {
                _horseAnimator.SetTrigger("Gallop0");
                _horseAnimator.ResetTrigger("Gallop1");
                _horseAnimator.ResetTrigger("Idle");
                
                //Debug.LogError("hors");
            }
            else
            {
                _horseAnimator.SetTrigger("Idle");
                _horseAnimator.ResetTrigger("Gallop0");
                //horseAnimator.ResetTrigger("Gallop1");

            }
        }
        else
        {
            Debug.LogError("man there's no animator for the horse (HorseAnim)");
        }
    }
}
