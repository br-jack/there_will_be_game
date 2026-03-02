using UnityEngine;

namespace Hammer
{
    public class PhoneController : IRotatable
    {
        public static bool IsAvailable()
        {
#if UNITY_EDITOR
            return UnityEditor.EditorApplication.isRemoteConnected;
#else 
            return false;
#endif
        }
        
        public void Connect()
        {
            Input.gyro.enabled = true;
        }

        public Vector3 GetRotationOffset()
        {
            return Input.gyro.rotationRateUnbiased;
        }

        public void Cleanup()
        {
            Input.gyro.enabled = false;
        }
    }
}