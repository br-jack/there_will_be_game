using System;
using System.IO.Ports;
using UnityEngine;

namespace Hammer
{
    public class HammerBehaviour : MonoBehaviour
    {

        Quaternion gameRotationVector;
        SerialPort stream;

        private bool portOpen;
        private readonly int timeoutMs = 50;

        Rigidbody rigidBody;

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
                    stream = new SerialPort(port, 9600)
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
        void Update()
        {

            if (!stream.IsOpen)
            {
                Debug.Log("Port is not open for reading.");
                return;
            }
            while (true)
            {
                string receivedData = stream.ReadLine();
                Debug.Log($"Received: {receivedData}");
                if (receivedData == null)
                {
                    return;
                }

                try
                {
                    Debug.Log(receivedData.Trim());

                    string[] imuOutput = receivedData.Split(':');
                    if (imuOutput[0] == "q")
                    {
                        gameRotationVector = new Quaternion(
                                                 float.Parse(imuOutput[1]),
                                                 float.Parse(imuOutput[3]),
                                                 float.Parse(imuOutput[2]),
                                                 float.Parse(imuOutput[4]));

                        Quaternion incorrectRotation = (Quaternion.Inverse(GlobalManager.Instance.CalibrationQuaternion) * gameRotationVector);
                        Quaternion correctedRotation = new(-incorrectRotation.x, incorrectRotation.y, -incorrectRotation.z, incorrectRotation.w);
                        transform.localRotation = correctedRotation;
                        return;
                    }
                    if (imuOutput[0] == "a")
                    {

                        Vector3 acceleration = new(
                            float.Parse(imuOutput[1]),
                            float.Parse(imuOutput[2]),
                            float.Parse(imuOutput[3])
                            );

                        rigidBody.AddForce(acceleration * rigidBody.mass);

                    }
                }
                catch (TimeoutException)
                {
                    Debug.LogWarning("Timeout occurred while reading data.");
                    return;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Error reading data: {ex.Message}");
                    return;
                }
            }
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
