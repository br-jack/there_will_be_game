using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
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