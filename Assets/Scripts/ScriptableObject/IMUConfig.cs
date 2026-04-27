using UnityEngine;

[CreateAssetMenu(fileName = "IMUConfig", menuName = "Scriptable Objects/IMUConfig")]
public class IMUConfig : ScriptableObject
{
    public bool enableRumble = true;
    
    public int readTimeout = 50;

    public int writeTimeout = 100;

    public int rumbleFadeInterval = 30;

    public int defaultDuration = 5000;
}
