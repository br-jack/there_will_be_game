using UnityEngine;

public class GlobalManager : MonoBehaviour
{
    public static GlobalManager Instance { get; private set; }
    
    public Hammer.IController hammerController;
    
    //FOR TEST ONLY, UNNEEDED, REMOVE
    public bool flipXInAttitudeQuaternion;
    public bool flipYInAttitudeQuaternion;
    public bool flipZInAttitudeQuaternion;
    public int indexSentToXInOuputQuaternion = 0;
    public int indexSentToYInOuputQuaternion = 1;
    public int indexSentToZInOuputQuaternion = 2;

    //THIS FUNCTION IS TEST ONLY TOO: 
    private void Update()
    {
        hammerController.UpdateTestQuaternionVariables(
            flipXInAttitudeQuaternion,
            flipYInAttitudeQuaternion,
            flipZInAttitudeQuaternion,
            indexSentToXInOuputQuaternion,
            indexSentToYInOuputQuaternion,
            indexSentToZInOuputQuaternion);
    }

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
