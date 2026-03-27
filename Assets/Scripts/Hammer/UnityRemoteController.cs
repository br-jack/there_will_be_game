using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Android;

namespace Hammer
{
    public class UnityRemoteController : IController
    {
        private AttitudeSensor _attitudeSensor;
        private LinearAccelerationSensor _linearAccelerationSensor;

        private bool _flipXInAttitudeQuaternion = true;
        private bool _flipYInAttitudeQuaternion = true;
        private bool _flipZInAttitudeQuaternion = false;
        private int _indexSentToXInOutputQuaternion = 0;
        private int _indexSentToYInOutputQuaternion = 1;
        private int _indexSentToZInOutputQuaternion = 2;

        //uncomment along with some code in global manager to do axis flips and switches in the editor
        /*
        public void UpdateTestQuaternionVariables(
            bool flipXInAttitudeQuaternion,
            bool flipYInAttitudeQuaternion,
            bool flipZInAttitudeQuaternion,
            int indexSentToXInOutputQuaternion,
            int indexSentToYInOutputQuaternion,
            int indexSentToZInOutputQuaternion
        ) {
            _flipXInAttitudeQuaternion = flipXInAttitudeQuaternion;
            _flipYInAttitudeQuaternion = flipYInAttitudeQuaternion;
            _flipZInAttitudeQuaternion = flipZInAttitudeQuaternion;
            _indexSentToXInOutputQuaternion = indexSentToXInOutputQuaternion;
            _indexSentToYInOutputQuaternion = indexSentToYInOutputQuaternion;
            _indexSentToZInOutputQuaternion = indexSentToZInOutputQuaternion;
            return;
        }   
        */
        
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
                    _attitudeSensor = InputSystem.GetDevice<AttitudeSensor>();
                    if (_attitudeSensor == null)
                    {
                        Debug.LogWarning("AttitudeSensor is not available");
                        _attitudeSensor = AttitudeSensor.current;
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
                Debug.LogWarning("LinearAccelerationSensor is not available");
                _linearAccelerationSensor = LinearAccelerationSensor.current;
            }

            if (_linearAccelerationSensor != null)
            {
                InputSystem.EnableDevice(_linearAccelerationSensor);
            }

            if (Touchscreen.current != null)
            {
                //While it can navigate the UI, the touch screen can also interfere with horse movement
                //so it's better to disable it.
                InputSystem.DisableDevice(Touchscreen.current);
            }
        
        }

        public void Update()
        {
            //Unity Remote doesn't immediately connect we keep running connect until it has
            if (_attitudeSensor == null ||
                _linearAccelerationSensor == null ||
                !_attitudeSensor.enabled ||
                !_linearAccelerationSensor.enabled)
            {
                Connect();
            }
        }

        public Quaternion GetAttitude()
        {
            if (_attitudeSensor == null)
            {
                return Quaternion.identity;
            }

            //these variables should be removed eventually! 
            //they are here to do axis flips and switches when testing, we can remove later
            float xMult,yMult,zMult = yMult = xMult = 1.0f;
            if (_flipXInAttitudeQuaternion) xMult = -1.0f;
            if (_flipYInAttitudeQuaternion) yMult = -1.0f;
            if (_flipZInAttitudeQuaternion) zMult = -1.0f;

            Quaternion sensorData = _attitudeSensor.attitude.ReadValue();
            return new Quaternion(
                xMult * sensorData[_indexSentToXInOutputQuaternion],
                yMult * sensorData[_indexSentToYInOutputQuaternion],
                zMult * sensorData[_indexSentToZInOutputQuaternion],
                sensorData.w);
        }

        public Vector3 GetAcceleration()
        {
            if (_linearAccelerationSensor == null)
            {
                return Vector3.zero;
            }
            
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