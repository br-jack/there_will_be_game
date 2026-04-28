using UnityEngine;
using UnityEngine.UI;

public class endAnimator : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    [SerializeField] private Sprite png0;
    [SerializeField] private Sprite png1;
    [SerializeField] private Sprite png2;
    [SerializeField] private Sprite png3;
    [SerializeField] private int startTime;
    Image endImage;
    AudioSource screenAudio;
    int timer = 0;
    int counter = 0;
    [SerializeField] int timeBetween = 10;
    void Start()
    {
        endImage = GetComponent<Image>();
        screenAudio = GetComponent<AudioSource>();
        screenAudio.time = startTime;
    }

    // Update is called once per frame
    void Update()
    {
        timer++;
        if (timer > timeBetween)
        {
            NextImage();
            timer = 0;
        }
    }
    void NextImage()
    {
        counter++;
        counter %= 4;
        if (counter == 0)
        {
            endImage.sprite = png0;
        }
        else if (counter == 1){
            endImage.sprite = png1;
        }
        else if (counter == 2)
        {
            endImage.sprite = png2;
        }
        else if (counter == 3)
        {
            endImage.sprite = png3;
        }
    }
}
