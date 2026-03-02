using UnityEngine;

namespace Hammer
{
    public class PhoneManager : IRotatable
    {
        public void Connect()
        {
            throw new System.NotImplementedException();
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