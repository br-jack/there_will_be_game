using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class HorseAnim : MonoBehaviour
{
    public float speed;
    Animator horseAnimator;
    public GameObject horseMovObj;
    HorseMovement horseMovement;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        horseAnimator = GetComponent<Animator>();
        horseMovement = horseMovObj.GetComponent<HorseMovement>();
        if (horseAnimator.enabled) //this is here because the animator panel is bugged in this unity version :(
        {
        horseAnimator.Rebind();
        horseAnimator.Update(0);
        } 
    }
    void ResetAll()
    {
        horseAnimator.ResetTrigger("Gallop0");
        horseAnimator.ResetTrigger("Gallop3");
    }

    // Update is called once per frame
    void Update()
    {
        Action jump = horseMovement.jumpStarted;

        if(horseAnimator != null)
        {
            // TODO:
            // currently holding the jump button loops the animation even when hitting the floor

            speed = horseMovement.GetCurrentSpeed();
            horseAnimator.SetFloat("Speed", speed);
            //Debug.LogError("horszz");
            bool grounded = horseMovement.CheckForGroundBelow(out RaycastHit groundHit, 1.0f);

            if (horseMovement.JumpButtonHeld && grounded)
            {
                horseAnimator.SetTrigger("Jump");
            }
            else
            {
                horseAnimator.ResetTrigger("Jump");
            }
            horseAnimator.SetBool("Grounded",grounded);
            if (speed  > 19)
            {
                horseAnimator.SetTrigger("Gallop3");
                horseAnimator.ResetTrigger("Gallop1");
                horseAnimator.ResetTrigger("Gallop0");
                //Debug.LogError("horszz");
            }
            else if (speed > 8){
                horseAnimator.SetTrigger("Gallop1");
                horseAnimator.ResetTrigger("Gallop3");
                horseAnimator.ResetTrigger("Gallop0");
            }
            else if (speed > 1)
            {
                horseAnimator.SetTrigger("Gallop0");
                horseAnimator.ResetTrigger("Gallop1");
                horseAnimator.ResetTrigger("Idle");
                
                //Debug.LogError("hors");
            }
            else
            {
                horseAnimator.SetTrigger("Idle");
                horseAnimator.ResetTrigger("Gallop0");
                //horseAnimator.ResetTrigger("Gallop1");

            }
        }
        else
        {
            Debug.LogError("man there's no animator for the horse (HorseAnim)");
        }
    }
}
