using Hammer;
using UnityEngine;

public class GlobalManager : MonoBehaviour
{
    public static GlobalManager Instance { get; private set; }
    public Quaternion CalibrationQuaternion = new Quaternion(1, 1, 1, 1);

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

    public static HammerBehaviour HammerBehaviour { get; private set; }


}
