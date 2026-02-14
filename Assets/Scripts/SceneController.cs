using UnityEngine.SceneManagement;

public static class SceneController
{
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