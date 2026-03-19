using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [SerializeField] private InputAction restartAction;

    void Awake()
    {
        restartAction.performed += context => ReloadScene();
    }
    
    public static void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }
    
    public static void LoadMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public static void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public static void LoadGame()
    {
        SceneManager.LoadScene("MainScene");
    }
        public static void LoadIntro()
    {
        SceneManager.LoadScene("IntroScene");
    }
    
    public static void LoadCalibration()
    {
        SceneManager.LoadScene("hammerTest");
    }
}