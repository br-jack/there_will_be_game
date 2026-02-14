using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void ExitGame()
    {
        if (Application.isEditor)
        {
            UnityEditor.EditorApplication.ExitPlaymode();
        }
        else
        {
            Application.Quit();
        }
    }
    
    public static void LoadMenu()
    {
        SceneManager.LoadScene("MainMenu");
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