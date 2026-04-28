using UnityEngine;

[CreateAssetMenu(fileName = "IMUConfig", menuName = "Scriptable Objects/IMUConfig")]
public class IMUConfig : ScriptableObject
{
    public string portOverride = null;
    
    public bool enableRumble = true;
    
    public int readTimeout = 50;

    public int writeTimeout = 100;

    public int rumbleFadeInterval = 30;

    public bool flipDefaultRumbleDirection = false;

    public int defaultRumbleDuration = 5000;

    [Range(0, 255)] public int defaultRumbleStartStrength = 255;
    [Range(0, 255)] public int defaultRumbleEndStrength = 255;

    public int defaultRumbleFadeDuration = 150;
}
