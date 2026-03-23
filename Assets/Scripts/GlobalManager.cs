using System;
using System.IO.Ports;
using Hammer;
using UnityEngine;

public class GlobalManager : MonoBehaviour
{
    public static GlobalManager Instance { get; private set; }

    public Hammer.IController hammerController = new IMUController();

    public Quaternion CalibrationQuaternion = new Quaternion(1, 1, 1, 1);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Application.targetFrameRate = 60;
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        hammerController.Connect();
    }

    public void OnDisable()
    {
        hammerController.Cleanup();
    }
}
