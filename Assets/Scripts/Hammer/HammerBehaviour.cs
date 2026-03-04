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

        public static HammerBehaviour Instance { get; private set; }

        SerialPort stream;
        [SerializeField] private bool closePort;

        internal bool open;
        [SerializeField] private int one = 1, two = 3, three = 2, four = 4;


        [SerializeField] private Quaternion attitude;

        private readonly int timeoutMs = 50;
        [SerializeField] private float x = 0, y = 0, z = 0;
        private static Vector3 eulerRotationAdjustment;

        [SerializeField] private Quaternion CalibrationQuaternion = new Quaternion(0, 0, 0, 0);

        public void CalibrateHammer()
        {
            CalibrationQuaternion = transform.rotation;
        }
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
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


        // Update is called once per frame
        void Update()
        {
            eulerRotationAdjustment = new Vector3(x, y, z);
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

                Quaternion imuData = new Quaternion(float.Parse(quaternionString[one]), 
                                                    float.Parse(quaternionString[two]), 
                                                    float.Parse(quaternionString[three]), 
                                                    float.Parse(quaternionString[four]));
                transform.rotation = imuData * CalibrationQuaternion;
                


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
