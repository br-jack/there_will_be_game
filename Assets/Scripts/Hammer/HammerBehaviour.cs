using System;
using System.Globalization;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;

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


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            try
            {
                stream = new SerialPort("COM3", 115200);
                stream.ReadTimeout = 50;
                stream.Open();
                open = true;
            }
            catch (System.Exception)
            {
                open = false;
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


            //TODO this is where the issue is lol
            Debug.Log(stream.ReadChar());


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


        }

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
