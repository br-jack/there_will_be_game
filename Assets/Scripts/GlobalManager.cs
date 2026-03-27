using UnityEngine;

public class GlobalManager : MonoBehaviour
{
    public static GlobalManager Instance { get; private set; }
    
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
        
        // Caligula is a PC game, this is only for testing with Unity Remote        
        #if (UNITY_IOS || UNITY_ANDROID)
            hammerController = new Hammer.UnityRemoteController();
        #else
            hammerController = new Hammer.IMUController();
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
