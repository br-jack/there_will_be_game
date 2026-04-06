using UnityEngine;
using UnityEngine.InputSystem;


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
        
        if(horseAnimator != null)
        {
            speed = horseMovement.GetCurrentSpeed();
            horseAnimator.SetFloat("Speed", speed);
            //Debug.LogError("horszz");
          
            if (speed  > 15)
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

            }
        }
        else
        {
            Debug.LogError("man there's no animator for the horse (HorseAnim)");
        }
    }
}
