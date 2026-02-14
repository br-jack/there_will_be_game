using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    private void Awake()
    {
        //Make sure only one scene manager is loaded at a time
        //(not sure if there's a better way to do this without using a tag)
        GameObject[] sceneManagers = GameObject.FindGameObjectsWithTag("SceneController");
        if (sceneManagers.Length > 1)
        {
            Destroy(this.gameObject);
        }
        
        DontDestroyOnLoad(gameObject);
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