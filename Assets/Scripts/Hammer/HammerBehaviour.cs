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
        
        [SerializeField] private bool closePort;

        internal bool open;
        [SerializeField] private int one = 1, two = 3, three = 2, four = 4;

        private readonly int timeoutMs = 50;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            Connect();

        }

        private void Connect()
        {
            try
            {
                stream = new SerialPort("COM3", 9600)
                //stream = new SerialPort("/dev/cu.usbmodem101", 9600)
                //stream = new SerialPort("/dev/ttyACM0", 9600)
                {
                    ReadTimeout = timeoutMs
                };
                stream.DtrEnable = true;
                stream.Open();
                open = true;
                Debug.Log("Connected (allegedly)");
            }
            catch (System.Exception e)
            {
                open = false;
                Debug.Log("Failed to connect to port: ");
                Debug.Log(e);
            }
        }

        public void CalibrateHammer()
        {
            GlobalManager.Instance.CalibrationQuaternion = imuData;
        }

        // Update is called once per frame
        void Update()
        {
            if (closePort)
            {
                open = false;
                stream.Close();
                Debug.Log("Port closed");
            }

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

                imuData = new Quaternion(float.Parse(quaternionString[one]), 
                                                    float.Parse(quaternionString[two]), 
                                                    float.Parse(quaternionString[three]), 
                                                    float.Parse(quaternionString[four]));

                Vector3 incorrectRotation = (Quaternion.Inverse(GlobalManager.Instance.CalibrationQuaternion) * imuData).eulerAngles;
                Vector3 correctedRotation = new Vector3(-incorrectRotation.x, incorrectRotation.y, -incorrectRotation.z);
                
                transform.rotation = Quaternion.Euler(correctedRotation);
                


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
            open = false;
            stream.Close();
            Debug.Log("Port closed");
        }

        public void WriteToArduino(string message)
        {
            if (open)
            {
                stream.WriteLine(message);
                stream.BaseStream.Flush();
            }

        }

    }

}
