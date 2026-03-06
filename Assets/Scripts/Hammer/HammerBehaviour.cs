using System;
using System.IO.Ports;
using UnityEngine;

namespace Hammer
{
    public class HammerBehaviour : MonoBehaviour
    {

        Quaternion imuData;
        SerialPort stream;

        private bool portOpen;
        private readonly int timeoutMs = 50;

        void Start()
        {
            Connect();
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
            GlobalManager.Instance.CalibrationQuaternion = imuData;
        }
        void Update()
        {

            if (!stream.IsOpen)
            {
                Debug.Log("Port is not open for reading.");
                return;
            }

            try
            {
                stream.ReadTimeout = timeoutMs;
                string receivedData = stream.ReadLine();
                Debug.Log($"Received: {receivedData}");
                Debug.Log(receivedData.Trim());

                string[] quaternionString = receivedData.Split(':');
                if (quaternionString[0] != "q")
                {
                    return;
                }

                imuData = new Quaternion(float.Parse(quaternionString[1]),
                                             float.Parse(quaternionString[3]),
                                             float.Parse(quaternionString[2]),
                                             float.Parse(quaternionString[4]));

                Quaternion incorrectRotation = (Quaternion.Inverse(GlobalManager.Instance.CalibrationQuaternion) * imuData);
                Quaternion correctedRotation = new(-incorrectRotation.x, incorrectRotation.y, -incorrectRotation.z, incorrectRotation.w);

                transform.localRotation = correctedRotation;

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
