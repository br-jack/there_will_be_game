using UnityEngine;

namespace Hammer
{
    public class UnityRemoteController : IController
    {
        public void Connect()
        {
            Input.gyro.enabled = true;
        }

        public void Update()
        {
            if (Input.touchCount > 0) Input.gyro.enabled = true;
        }

        public Quaternion GetAttitude()
        {
            return Input.gyro.attitude;
        }

        public Vector3 GetAcceleration()
        {
            return Input.acceleration;
        }

        public void Cleanup()
        {
            Input.gyro.enabled = false;
        }
    }
}