using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        
    }
    
    public static void LoadMenu()
    {
        SceneManager.LoadScene("Menu");
    }
    
    public static void LoadGame()
    {
        SceneManager.LoadScene("MainScene");
    }
    
    public static void LoadCalibration()
    {
        SceneManager.LoadScene("hammerTest");
    }
}