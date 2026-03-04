using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
                if (Application.platform.Equals(RuntimePlatform.WindowsEditor) || Application.platform.Equals(RuntimePlatform.WindowsPlayer))
                {
                stream = new SerialPort("COM3", 9600)
                {
                    ReadTimeout = timeoutMs
                };
                }

                if (Application.platform.Equals(RuntimePlatform.OSXEditor) || Application.platform.Equals(RuntimePlatform.OSXPlayer))
                {
                    stream = new SerialPort("/dev/cu.usbmodem101", 9600)
                    {
                        ReadTimeout = timeoutMs
                    };
                }

                if (Application.platform.Equals(RuntimePlatform.LinuxPlayer) || Application.platform.Equals(RuntimePlatform.LinuxServer))
                {
                stream = new SerialPort("/dev/ttyACM0", 9600)
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
                Debug.Log("Failed to connect to port: ");
                Debug.Log(e);
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

                // TODO make this use only quaternions?
                Vector3 incorrectRotation = (Quaternion.Inverse(GlobalManager.Instance.CalibrationQuaternion) * imuData).eulerAngles;
                Vector3 correctedRotation = new Vector3(-incorrectRotation.x, incorrectRotation.y, -incorrectRotation.z);
                
                transform.localRotation = Quaternion.Euler(correctedRotation);
                
            }
            catch (TimeoutException)
            {
                Debug.Log("Timeout occurred while reading data.");
                return;
            }
            catch (Exception ex)
            {
                Debug.Log($"Error reading data: {ex.Message}");
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

        void OnDisable ()
        {
            portOpen = false;
            stream.Close();
            Debug.Log("Port closed");
        }

    }

}
