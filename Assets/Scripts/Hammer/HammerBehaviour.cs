using System;
using System.Globalization;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using WiimoteApi;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

namespace Hammer
{
    //probs should set up a mechanism for calibrating the accelerometer. 
    // This will need the game to take the user through a short process.  
    //currently just uses whatever calibration values are in there. 
    public class HammerBehaviour : MonoBehaviour
    {

        //public Wiimote Wiimote { get; private set; }

        
        //Hammer should start flat with Pitch = 0
        public Quaternion StartingRotation { get; private set; }

        public Quaternion wiimoteAttitude; //should be private, can change later as only one method uses i think!

        public float accelAdjustmentRatio; //0.02 seems reasonable

        
        //debug
        public bool accelEnabled;
        public bool gyroEnabled;
        public int mult0;
        public int mult1;
        public int mult2;
        public int ind0;
        public int ind1;
        public int ind2;
        public float accelPitch;
        public float accelRoll;


        public void SceneSwitch()
        {
            SceneManager.LoadScene("hammerTest");
        }

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
                WiimoteGlobal.wiimote = WiimoteManager.Wiimotes[0];

                //If the wiimote wasn't connected through dolphin, it may still have blinking lights
                //even though it actually is still connected
                WiimoteGlobal.wiimote.SendPlayerLED(true, false, false, true);

                if (WiimoteGlobal.wiimote.Type == WiimoteType.WIIMOTEPLUS)
                {
                    //Running RequestIdentifyWiiMotionPlus() on a Wiimote Plus unfortunately fails,
                    //so we have to skip that check.
                    Debug.Log("Wii Remote Plus detected, skipping Motion Plus check.");
                    WiimoteGlobal.wiimote.ActivateWiiMotionPlus();
                }
                else
                {
                    WiimoteGlobal.wiimote.RequestIdentifyWiiMotionPlus();

                    if (WiimoteGlobal.wiimote.wmp_attached)
                    {
                        WiimoteGlobal.wiimote.ActivateWiiMotionPlus();
                        Debug.Log("Connected with Wii Motion Plus Extension.");
                    }
                    else
                    {
                        WiimoteGlobal.wiimote.ActivateWiiMotionPlus();
                        Debug.LogWarning("Wii remote doesn't have motion plus :( have to activate anyways as check seems to not work");
                    }
                }
                
                //Default input mode only sends button data, so for accelerometer / gyro data 
                //we need to request a mode with extension bytes
                WiimoteGlobal.wiimote.SendDataReportMode(InputDataType.REPORT_BUTTONS_ACCEL_EXT16);
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
            WiimoteGlobal.wiimote = null;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            
        }

        //Called once before start when the game starts
        void Awake()
        {
            StartingRotation = transform.rotation;
            wiimoteAttitude = StartingRotation; //have to set it to something! assume same as hammer
            ConnectWiimote();
        }

        // Update is called once per frame
        void Update()
        {
            Assert.IsTrue(WiimoteManager.HasWiimote(), "A Wiimote must be connected");

            //TODO As well as making this more efficient, we can probs use the "slow mode" booleans to improve accuracy
            int ret;
            Vector3 gyroOffset = Vector3.zero;
            Vector3 accelOffset = Vector3.zero;
            
            if (gyroEnabled) { //debug
            do {
                ret = WiimoteGlobal.wiimote.ReadWiimoteData();

                

                Vector3 accelDataForFrameTest = new Vector3(
                    WiimoteGlobal.wiimote.Accel.GetCalibratedAccelData()[0],
                    WiimoteGlobal.wiimote.Accel.GetCalibratedAccelData()[1],
                    WiimoteGlobal.wiimote.Accel.GetCalibratedAccelData()[2]);
                print("Accel data: "+accelDataForFrameTest);
                accelOffset += accelDataForFrameTest;

                
                //this is a test basically

                //GYROSCOPE
                //add all detected rotations throughout the frame to gyroOffset
                //we should integrate this! would give accurate total rotation
                if (ret > 0 && WiimoteGlobal.wiimote.current_ext == ExtensionController.MOTIONPLUS) {
                    gyroOffset += new Vector3(  -WiimoteGlobal.wiimote.MotionPlus.PitchSpeed,
                                                    WiimoteGlobal.wiimote.MotionPlus.YawSpeed,
                                                    -WiimoteGlobal.wiimote.MotionPlus.RollSpeed);
                }
            } while (ret > 0);
            }   else ret = WiimoteGlobal.wiimote.ReadWiimoteData();
            

            gyroOffset /= 95f; //divide by 95 because of the average rate of sending messages of the wiimote is 95Hz
                            //and speeds of rotations are sent in degrees per second (i think!)
                            //would be cool to actually count the number of updates per second but I'm not sure how. 
                            //i think that this is completely wrong. nothing like 95 messages sent per frame. but idk
                            //oh wait yeah ofc its wrong, we are not 1 frame per second!! 


            
            // ReadWiimoteData() returns 0 when nothing is left to read.
            // So by doing this we continue to update the Wiimote's attitude until it is "up to date."
            wiimoteAttitude *= Quaternion.Euler(gyroOffset);

            
            //ACCELEROMETER adjustment
            if (accelEnabled) { 
            Vector3 accel = new Vector3(
                (mult0)*WiimoteGlobal.wiimote.Accel.GetCalibratedAccelData()[ind0], //x 
                (mult1)*WiimoteGlobal.wiimote.Accel.GetCalibratedAccelData()[ind1], //y 
                (mult2)*WiimoteGlobal.wiimote.Accel.GetCalibratedAccelData()[ind2]); //z 
            
            
            accel.Normalize();

            float accel_roll;
            float accel_yaw;
            float accel_pitch;

            if (Math.Abs(accel.z) < 0.05) accel_roll = wiimoteAttitude.eulerAngles.z;
                else accel_roll = Mathf.Atan2(accel.x,accel.z) * Mathf.Rad2Deg;
            if (Math.Abs(accel.y) < 0.05) accel_yaw = wiimoteAttitude.eulerAngles.y;
                else accel_yaw = Mathf.Atan2(accel.x,accel.y) * Mathf.Rad2Deg;
            if (Math.Abs(accel.z) < 0.05) accel_pitch = wiimoteAttitude.eulerAngles.x;
                else accel_pitch =Mathf.Atan2(accel.y,accel.z) * Mathf.Rad2Deg;
            
            accelPitch = accel_pitch; //only to print, i know these lines look silly
            accelRoll = accel_roll;
            
                
            //float accel_yaw_guess = wiimoteAttitude.eulerAngles.y;
            

            Quaternion accel_suggested_attitude = Quaternion.Euler(new Vector3(accel_pitch,accel_yaw,-accel_roll)); //always zero yaw is probs silly
            /*
            wiimoteAttitude = Quaternion.Slerp(
                wiimoteAttitude,
                accel_suggested_attitude,
                accelAdjustmentRatio
            );
            */
            wiimoteAttitude = accel_suggested_attitude;
            }
            
            transform.localRotation = wiimoteAttitude;


            //Unity Remote
            //transform.rotation = Quaternion.Inverse(Input.gyro.attitude * _startingRotation);

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
            CleanupWiimotes();
        }
    }

}
