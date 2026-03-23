using UnityEngine;
using UnityEngine.Android;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Android;
using Gyroscope = UnityEngine.InputSystem.Gyroscope;

namespace Hammer
{
    public class UnityRemoteController : IController
    {
        private AttitudeSensor _attitudeSensor;
        private LinearAccelerationSensor _linearAccelerationSensor;
        private Gyroscope _gyroscope;

        private Vector3 _rotation;
        
        public void Connect()
        {
            Debug.Log("Attempting to connect to Unity Remote");
            
            // Determine if a Gyroscope sensor device is present.
            // if (Gyroscope.current != null)
                // Debug.Log("Gyroscope present");
            // InputSystem.EnableDevice(Gyroscope.current);
            
            _attitudeSensor = InputSystem.GetDevice<AndroidGameRotationVector>();
            if (_attitudeSensor == null)
            {
                _attitudeSensor = InputSystem.GetDevice<AndroidRotationVector>();
                if (_attitudeSensor == null)
                {
                    _attitudeSensor = AndroidGameRotationVector.current;

                    // _attitudeSensor = InputSystem.GetDevice<AttitudeSensor>();
                    if (_attitudeSensor == null)
                    {
                        Debug.LogError("AttitudeSensor is not available");
                    }
                }
            }
            if (_attitudeSensor != null)
            {
                InputSystem.EnableDevice(_attitudeSensor);
            }

            _linearAccelerationSensor = InputSystem.GetDevice<LinearAccelerationSensor>();
            if (_linearAccelerationSensor == null)
            {
                Debug.LogError("LinearAccelerationSensor is not available");
                _linearAccelerationSensor = LinearAccelerationSensor.current;
            }

            if (_linearAccelerationSensor != null)
            {
                InputSystem.EnableDevice(_linearAccelerationSensor);
            }
            
            _gyroscope = InputSystem.GetDevice<Gyroscope>();
            InputSystem.EnableDevice(_gyroscope);

            _rotation = Vector3.zero;
        }

        public void Update()
        {
            if (Input.touchCount > 0)
            {
                Connect();
            }
            _rotation += _gyroscope.angularVelocity.ReadValue();
        }

        public Quaternion GetAttitude()
        {
            return Quaternion.Euler(_rotation);
        }

        public Vector3 GetAcceleration()
        {
            return _linearAccelerationSensor.acceleration.ReadValue();
        }

        public void Cleanup()
        {
            InputSystem.DisableDevice(_attitudeSensor);
            InputSystem.DisableDevice(_linearAccelerationSensor);
            _attitudeSensor = null;
            _linearAccelerationSensor = null;
        }
    }
}