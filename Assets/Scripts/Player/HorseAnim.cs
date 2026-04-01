using UnityEngine;


public class HorseAnim : MonoBehaviour
{
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
            if (Input.GetKeyDown(KeyCode.W))
            {
                horseAnimator.SetTrigger("Gallop3");
                Debug.LogError("hors");

            }
            if (Input.GetKeyDown(KeyCode.S))
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
