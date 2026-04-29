using UnityEngine;

[CreateAssetMenu(fileName = "RumbleInstance", menuName = "Scriptable Objects/RumbleInstance")]
public class RumbleInstance : ScriptableObject
{
    public int duration = 500;
    public int fadeInterval = 30;
    public int fadeDuration = 150;
    public bool flipMotorDirection = false;
    [Range(0, 255)] public int startStrength = 255;
    [Range(0, 255)] public int endStrength = 255;
}
