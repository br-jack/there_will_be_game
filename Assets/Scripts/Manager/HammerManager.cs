using UnityEngine;

public class HammerManager : MonoBehaviour
{
    public static HammerManager Instance { get; private set; }

    [SerializeField] private IMUConfig imuConfigSO;
    
    public Hammer.IController hammerController;
    
    //for testing, you can uncomment this to do axis flips and switches for unity remote
    /*
    public bool flipXInAttitudeQuaternion = true;
    public bool flipYInAttitudeQuaternion = true;
    public bool flipZInAttitudeQuaternion = false;
    public int indexSentToXInOutputQuaternion = 0;
    public int indexSentToYInOutputQuaternion = 1;
    public int indexSentToZInOutputQuaternion = 2;

    private void Update()
    {
        /*
        if (hammerController is Hammer.UnityRemoteController unityRemoteController)
        {
            unityRemoteController.UpdateTestQuaternionVariables(
                flipXInAttitudeQuaternion,
                flipYInAttitudeQuaternion,
                flipZInAttitudeQuaternion,
                indexSentToXInOutputQuaternion,
                indexSentToYInOutputQuaternion,
                indexSentToZInOutputQuaternion);
        }
    } 
    */

    public Quaternion CalibrationQuaternion = new Quaternion(1, 1, 1, 1);
    public void SaveCalibration(Quaternion calibrationQuaternion)
    {
        CalibrationQuaternion = calibrationQuaternion;
        PlayerPrefs.SetFloat("x", calibrationQuaternion.x);
        PlayerPrefs.SetFloat ("y", calibrationQuaternion.y);
        PlayerPrefs.SetFloat("z", calibrationQuaternion.z);
        PlayerPrefs.SetFloat("w", calibrationQuaternion.w);
        PlayerPrefs.Save();
    }

    public void LoadCalibration()
    {
        CalibrationQuaternion.x = PlayerPrefs.GetFloat("x", 1);
        CalibrationQuaternion.y = PlayerPrefs.GetFloat("y", 1);
        CalibrationQuaternion.z = PlayerPrefs.GetFloat("z", 1);
        CalibrationQuaternion.w = PlayerPrefs.GetFloat("w", 1);
    }

    private void Awake()
    {
        LoadCalibration();
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        //Aim for around 60fps
        //Application.targetFrameRate = 60;
        
        if (120 <= Screen.currentResolution.refreshRateRatio.value)
        {
            QualitySettings.vSyncCount = 2;
        }
        else
        {
            QualitySettings.vSyncCount = 1;
        }
        
        // Caligula is a PC game, this is only for testing with Unity Remote        
        #if (UNITY_IOS || UNITY_ANDROID)
            hammerController = new Hammer.UnityRemoteController();
        #else
            hammerController = new Hammer.IMUController(imuConfigSO);
        #endif
        
        hammerController.Connect();
    }

    private void OnDestroy()
    {
        if (hammerController != null)
        {
            hammerController.Cleanup();
            hammerController = null;
        }
    }
}
