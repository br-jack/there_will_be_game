using System;
using System.Globalization;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using WiimoteApi;
using UnityEngine.InputSystem;

namespace Hammer
{
    //probs should set up a mechanism for calibrating the accelerometer. 
    // This will need the game to take the user through a short process.  
    //currently just uses whatever calibration values are in there. 
    public class HammerBehaviour : MonoBehaviour
    {

        public Wiimote Wiimote { get; private set; }

        
        //Hammer should start flat with Pitch = 0
        public Quaternion StartingRotation { get; private set; }

        public float FilterRatio;

        public Quaternion attitude;


        void ConnectWiimote() {
            if (WiimoteManager.HasWiimote())
            {
                Debug.LogWarning("Attempting to find a Wiimote even though one is already connected!");
            }
            
            WiimoteManager.FindWiimotes(); // Poll native bluetooth drivers to find Wiimotes

            if (WiimoteManager.HasWiimote())
            {
                Assert.IsTrue(WiimoteManager.Wiimotes.Count == 1, "Only one Wiimote should be connected at a time");
                //Use the first wiimote: others ignored
                Wiimote = WiimoteManager.Wiimotes[0];

                //If the wiimote wasn't connected through dolphin, it may still have blinking lights
                //even though it actually is still connected
                Wiimote.SendPlayerLED(true, false, false, true);

                if (Wiimote.Type == WiimoteType.WIIMOTEPLUS)
                {
                    //Running RequestIdentifyWiiMotionPlus() on a Wiimote Plus unfortunately fails,
                    //so we have to skip that check.
                    Debug.Log("Wii Remote Plus detected, skipping Motion Plus check.");
                    Wiimote.ActivateWiiMotionPlus();
                }
                else
                {
                    Wiimote.RequestIdentifyWiiMotionPlus();

                    if (Wiimote.wmp_attached)
                    {
                        Wiimote.ActivateWiiMotionPlus();
                        Debug.Log("Connected with Wii Motion Plus Extension.");
                    }
                    else
                    {
                        Debug.LogWarning("Wii remote doesn't have motion plus :(");
                    }
                }
                
                //Default input mode only sends button data, so for accelerometer / gyro data 
                //we need to request a mode with extension bytes
                Wiimote.SendDataReportMode(InputDataType.REPORT_BUTTONS_ACCEL_EXT16);
            }
        }
        
        private void CleanupWiimotes()
        {
            Debug.Log("Cleaning up Wiimote connections.");
            
            //Iterate from the end of the collection to prevent errors from the cleanup function removing each element
            for (int index = WiimoteManager.Wiimotes.Count - 1; index >= 0; index--)
            {
                Wiimote remote = WiimoteManager.Wiimotes[index];
                //TODO manually reset LED, rumble etc. before cleaning without it crashing
                // remote.SendPlayerLED(true, false, false, false);
                WiimoteManager.Cleanup(remote);
            }

            //Ensure reference to removed wiimote isn't used
            Wiimote = null;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            print("force activating wii motion plus!");
            Wiimote.ActivateWiiMotionPlus();
        }

        //Called once before start when the game starts
        void Awake()
        {
            StartingRotation = transform.rotation;
            ConnectWiimote();
        }

            
        // Update is called once per frame
        void Update() 
        {
           
                
            
            
            Assert.IsTrue(WiimoteManager.HasWiimote(), "A Wiimote must be connected");

            int ret;
            Vector3 gyroOffset = Vector3.zero;

            do { 
                ret = Wiimote.ReadWiimoteData();

                //GYROSCOPE
                //add all detected rotations throughout the frame to gyroOffset
                if (ret > 0)// && Wiimote.current_ext == ExtensionController.MOTIONPLUS 
                            // would be good but doesn't seem to work with actual extnesion (not built in)
                    {gyroOffset += new Vector3( 
                        Wiimote.MotionPlus.PitchSpeed, 
                        Wiimote.MotionPlus.RollSpeed, 
                        Wiimote.MotionPlus.YawSpeed);
                    } 

                
            } while (ret > 0); // ReadWiimoteData() returns 0 when nothing is left to read.
            // So by doing this we continue to update the Wiimote's attitude until it is "up to date."

            //ACCELEROMETER ADJUSTMENT
            float accel_x = Wiimote.Accel.GetCalibratedAccelData()[0];
            float accel_y = Wiimote.Accel.GetCalibratedAccelData()[1];
            float accel_z = Wiimote.Accel.GetCalibratedAccelData()[2];

            float accelPitch = Mathf.Atan2(accel_y, accel_z) * Mathf.Rad2Deg;
            float accelRoll  = Mathf.Atan2(-accel_x, Mathf.Sqrt(accel_y*accel_y + accel_z*accel_z)) * Mathf.Rad2Deg;

            gyroOffset /= 95f;
            //divide by 95 because of the average rate of sending messages of the wiimote is 95Hz
            //and speeds of rotations are sent in degrees per second (i think!)

            //probs shouldn't have so much switching between quaternions and euler! 
            // i want to use quaternions but i am not cool enough to understand them yet
            attitude *= Quaternion.Euler(gyroOffset);
            Vector3 accel = new Vector3(accel_x, accel_y, accel_z).normalized;
            Vector3 estimatedGravity = attitude * Vector3.down;
            Quaternion gravityError = Quaternion.FromToRotation(estimatedGravity, accel);
            float correctionStrength = 1f - FilterRatio; 
            Quaternion correction = Quaternion.Slerp(Quaternion.identity, gravityError, correctionStrength);

            attitude = correction * attitude;


            /*
            commenting out while i try to only quaternion
            Vector3 euler_attitude = attitude.eulerAngles;
            //euler_attitude.x = FilterRatio * euler_attitude.x + (1f - FilterRatio) * accelPitch;
            //euler_attitude.z = FilterRatio * euler_attitude.z + (1f - FilterRatio) * accelRoll;
            attitude = Quaternion.Euler(euler_attitude);
            */
            
            transform.localRotation = attitude;
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
            CleanupWiimotes();
        }
    }

}
