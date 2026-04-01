#if (UNITY_IOS || UNITY_ANDROID) 
using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_ANDROID
using UnityEngine.InputSystem.Android;
#endif

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
            
            _attitudeSensor = InputSystem.GetDevice<AttitudeSensor>();
            if (_attitudeSensor == null)
            {
                Debug.LogWarning("AttitudeSensor is not available");
                _attitudeSensor = AttitudeSensor.current;
            }
            
#if UNITY_ANDROID
            //attempt to get android game rotation vector
            //This unfortunately doesn't seem to work with Android Unity Remote
            AttitudeSensor androidSensor = InputSystem.GetDevice<AndroidGameRotationVector>();
            if (androidSensor == null)
            {
                androidSensor = InputSystem.GetDevice<AndroidRotationVector>();
            }

            if (androidSensor != null)
            {
                _attitudeSensor = androidSensor;
            }
#endif
            
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

        public void Rumble()
        {
            //Vibration unfortunately doesn't seem to be supported with Unity Remote
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
#endif