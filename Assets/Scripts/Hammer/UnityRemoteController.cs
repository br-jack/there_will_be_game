using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Android;
using Gyroscope = UnityEngine.InputSystem.Gyroscope;

namespace Hammer
{
    public class UnityRemoteController : IController
    {
        private AttitudeSensor _attitudeSensor;
        
        public void Connect()
        {
            // Determine if a Gyroscope sensor device is present.
            if (Gyroscope.current != null)
                Debug.Log("Gyroscope present");
            InputSystem.EnableDevice(Gyroscope.current);
            
            _attitudeSensor = InputSystem.GetDevice<AndroidRotationVector>();
            if (_attitudeSensor == null)
            {
                _attitudeSensor = InputSystem.GetDevice<AndroidGameRotationVector>();
                if (_attitudeSensor == null)
                    Debug.LogError("AttitudeSensor is not available");
            }

            if (_attitudeSensor != null)
            {
                InputSystem.EnableDevice(_attitudeSensor);
            }
        }

        public void Update()
        {
            if (Input.touchCount > 0)
            {
                InputSystem.EnableDevice(_attitudeSensor);
            }
        }

        public Quaternion GetAttitude()
        {
            return _attitudeSensor.attitude.value;
        }

        public Vector3 GetAcceleration()
        {
            return Input.acceleration;
        }

        public void Cleanup()
        {
            InputSystem.DisableDevice(Gyroscope.current);
        }
    }
}