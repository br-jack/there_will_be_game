using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [SerializeField] private InputActionReference restartAction;

    void Awake()
    {
        restartAction.action.performed += context => ReloadScene();
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
    public static void LoadEnd1()
    {
        SceneManager.LoadScene("EndScene 1");
    }
    public static void LoadEnd2()
    {
        SceneManager.LoadScene("EndScene 2");
    }
    public static void LoadEnd3()
    {
        SceneManager.LoadScene("EndScene 3");
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
        SceneManager.LoadScene("TutorialScene");
    }

    public static void LoadCalibration()
    {
        SceneManager.LoadScene("hammerTest");
    }

    public static void LoadTutorial()
    {
        SceneManager.LoadScene("TutorialScene");
    }
}