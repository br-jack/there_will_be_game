using Hammer;
using UnityEngine;

public class GlobalManager : MonoBehaviour
{
    public static GlobalManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public Quaternion CalibrationQuaternion = new Quaternion(1, 1, 1, 1);
    public static HammerBehaviour HammerBehaviour { get; private set; }


}
