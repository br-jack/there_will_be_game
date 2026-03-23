using System;
using System.IO.Ports;
using Hammer;
using UnityEngine;

public class GlobalManager : MonoBehaviour
{
    public static GlobalManager Instance { get; private set; }
    
    public Hammer.IController hammerController;

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
        
// Caligula is a PC game, this is only for Unity Remote        
#if (UNITY_IOS || UNITY_ANDROID)
        hammerController = new UnityRemoteController();
#else
        hammerController = new IMUController();
#endif
        
        hammerController.Connect();
    }

    public void OnDisable()
    {
        hammerController.Cleanup();
    }
}
