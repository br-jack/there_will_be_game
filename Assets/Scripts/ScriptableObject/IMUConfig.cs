using UnityEngine;

[CreateAssetMenu(fileName = "IMUConfig", menuName = "Scriptable Objects/IMUConfig")]
public class IMUConfig : ScriptableObject
{
    public string portOverride = null;
    
    public bool enableRumble = true;
    
    public int readTimeout = 50;

    public int writeTimeout = 100;

    public RumbleInstance defaultRumbleInstance;
}
