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
    //probs should set up a mechanism for calibrating the accelerometer. 
    // This will need the game to take the user through a short process.  
    //currently just uses whatever calibration values are in there. 

    public class HammerBehaviour : MonoBehaviour
    {
        SerialPort stream;
        public bool closePort;

        internal bool open;


        //Hammer should start flat with Pitch = 0
        public Quaternion StartingRotation { get; private set; }

        private Quaternion attitude;

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
                //stream - new SerialPort("COM3", 9600)
                //stream = new SerialPort("/dev/cu.usbmodem101", 9600)
                stream = new SerialPort("/dev/ttyACM0", 9600)
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
                Debug.Log("Failed to connect to COM3: ");
                Debug.Log(e);
            }
        }

        //Called once before start when the game starts
        void Awake()
        {
            StartingRotation = transform.rotation;
        }

        // Update is called once per frame
        void Update()
        {
            if (closePort)
            {
                open = false;
                stream.Close();
                Debug.Log("COM3 closed");
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

                transform.rotation = new Quaternion(float.Parse(quaternionString[1]), float.Parse(quaternionString[2]), float.Parse(quaternionString[3]), float.Parse(quaternionString[4]));

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

        //Assert.IsTrue(WiimoteManager.HasWiimote(), "A Wiimote must be connected");

        ////TODO As well as making this more efficient, we can probs use the "slow mode" booleans to improve accuracy
        //int ret;
        //Vector3 gyroOffset = Vector3.zero;
        //Vector3 accelOffset = Vector3.zero;

        //do {
        //    ret = WiimoteGlobal.wiimote.ReadWiimoteData();

        //    //ACCELEROMETER

        //    Vector3 accelDataForFrameTest = new Vector3(
        //        WiimoteGlobal.wiimote.Accel.GetCalibratedAccelData()[0],
        //        WiimoteGlobal.wiimote.Accel.GetCalibratedAccelData()[1],
        //        WiimoteGlobal.wiimote.Accel.GetCalibratedAccelData()[2]);
        //    print("Accel data: "+accelDataForFrameTest);
        //    accelOffset += accelDataForFrameTest;

        //    //this is a test basically

        //    //GYROSCOPE
        //    //add all detected rotations throughout the frame to gyroOffset
        //    //we should integrate this! would give accurate total rotation
        //    if (ret > 0 && WiimoteGlobal.wiimote.current_ext == ExtensionController.MOTIONPLUS) {
        //        gyroOffset += new Vector3(  -WiimoteGlobal.wiimote.MotionPlus.PitchSpeed,
        //                                        -WiimoteGlobal.wiimote.MotionPlus.RollSpeed,
        //                                        -WiimoteGlobal.wiimote.MotionPlus.YawSpeed);
        //    }
        //} while (ret > 0);

        //gyroOffset /= 95f;

        //transform.Rotate(gyroOffset, Space.Self);




        public void OnCollisionEnter(Collision collision)
        {

            if (collision.gameObject.CompareTag("Enemy"))
            {

                Destroy(collision.gameObject);
            }
        }

        void OnApplicationQuit()
        {
            open = false;
            stream.Close();
            Debug.Log("COM3 closed");
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
