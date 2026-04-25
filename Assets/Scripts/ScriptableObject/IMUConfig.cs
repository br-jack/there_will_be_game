using UnityEngine;

[CreateAssetMenu(fileName = "IMUConfig", menuName = "Scriptable Objects/IMUConfig")]
public class IMUConfig : ScriptableObject
{
    public int timeoutMs;

    public int rumbleFadeInterval;
}
