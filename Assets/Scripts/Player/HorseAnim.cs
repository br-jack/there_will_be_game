using UnityEngine;
using UnityEngine.InputSystem;


public class HorseAnim : MonoBehaviour
{
    public float speed;
    Animator horseAnimator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        horseAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(horseAnimator != null)
        {
            Debug.LogError("horszz");
            if (speed > 10)
            {
                horseAnimator.SetTrigger("Gallop3");
                Debug.LogError("hors");

            }
            if (speed > 20)
            {
                horseAnimator.SetTrigger("Gallop0");
            }
        }
        else
        {
            Debug.LogError("man there's no animator for the horse (HorseAnim)");
        }
    }
}
