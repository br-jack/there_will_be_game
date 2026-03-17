using UnityEngine;

public class TakeScreenshot : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ScreenCapture.CaptureScreenshot("menuPicture.png");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
