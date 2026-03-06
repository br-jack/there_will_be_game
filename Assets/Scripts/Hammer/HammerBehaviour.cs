using System;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;

namespace Hammer
{
    public class HammerBehaviour : MonoBehaviour
    {

        Quaternion gameRotationVector;
        List<Vector3> accelerations = new List<Vector3>();
        SerialPort stream;

        private bool portOpen=false;
        private readonly int timeoutMs = 50;

        public Rigidbody rigidBody;

        void Start()
        {
            Connect();
            rigidBody = GetComponent<Rigidbody>();
        }

        private void Connect()
        {
            try
            {
                string port = null;
                if (Application.platform.Equals(RuntimePlatform.WindowsEditor) || Application.platform.Equals(RuntimePlatform.WindowsPlayer))
                {
                    port = "COM3";
                }

                if (Application.platform.Equals(RuntimePlatform.OSXEditor) || Application.platform.Equals(RuntimePlatform.OSXPlayer))
                {
                    port = "/dev/cu.usbmodem101";
                }

                if (Application.platform.Equals(RuntimePlatform.LinuxPlayer) || Application.platform.Equals(RuntimePlatform.LinuxServer))
                {
                    port = "/dev/ttyACM0";
                }

                if (!string.IsNullOrEmpty(port))
                {
                    stream = new SerialPort(port, 19200)
                    {
                        ReadTimeout = timeoutMs
                    };
                }
                stream.DtrEnable = true;
                stream.Open();
                stream.ReadTimeout = timeoutMs;
                portOpen = true;
                Debug.Log("Connected (allegedly)");
            }
            catch (System.Exception e)
            {
                portOpen = false;
                Debug.LogWarning("Failed to connect to port: ");
                Debug.LogWarning(e);
            }
        }
        public void CalibrateHammer()
        {
            GlobalManager.Instance.CalibrationQuaternion = gameRotationVector;
            
        }

        void ParseStream()
        {

            string recievedData = null;
            bool rotationReading = false;
            int accelerationReadings = 0;
            
            // return one rotation (whatever the last one is) and a list of accel values?

            while (stream.BytesToRead > 0)
            {
                try
                {
                recievedData = stream.ReadLine();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Error reading data: {ex.Message}");
                    return;
                }

                if (recievedData == null)
                {
                    return;
                }

                Debug.Log(recievedData);
                string[] parsedData = recievedData.Trim().Split(':');

                // just get all acceleration readings
                if (parsedData[0] == "a" && accelerationReadings < 5)
                {
                    Vector3 acceleration = new(
                        float.Parse(parsedData[1]),
                        float.Parse(parsedData[2]),
                        float.Parse(parsedData[3])
                        );
                    accelerations.Add(acceleration);
                    accelerationReadings++;
                }
            
                if (parsedData[0] == "q")
                {
                    gameRotationVector = new Quaternion(
                                                     float.Parse(parsedData[1]),
                                                     float.Parse(parsedData[3]),
                                                     float.Parse(parsedData[2]),
                                                     float.Parse(parsedData[4]));
                    rotationReading = true;
                }

            }

        }

        void UpdateRotation()
        {           
            Quaternion incorrectRotation = (Quaternion.Inverse(GlobalManager.Instance.CalibrationQuaternion) * gameRotationVector);
            Quaternion correctedRotation = new(-incorrectRotation.x, incorrectRotation.y, -incorrectRotation.z, incorrectRotation.w);
            transform.localRotation = correctedRotation;
        }

        void Update()
        {


            if (!stream.IsOpen)
            {
                Debug.Log("Port is not open for reading.");
                return;
            }

            ParseStream();
            UpdateRotation();

            accelerations.Clear();


        }


            
        public void OnCollisionEnter(Collision collision)
        {

            if (collision.gameObject.CompareTag("Enemy"))
            {
                Destroy(collision.gameObject);
            }
        }

        void OnDisable()
        {
            portOpen = false;
            stream.Close();
            Debug.Log("Port closed");
        }

    }

}
