using System;
using System.Globalization;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using WiimoteApi;

namespace Hammer
{
    
    public class HammerBehaviour : MonoBehaviour
    {

        public Wiimote Wiimote { get; private set; }

        
        //Hammer should start flat with Pitch = 0
        public Quaternion StartingRotation { get; private set; }

        private Quaternion attitude;


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
                Wiimote.SendDataReportMode(InputDataType.REPORT_BUTTONS_EXT19);
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
            
        }

        //Called once before start when the game starts
        void Awake()
        {
            //StartingRotation = transform.rotation;
            ConnectWiimote();
        }

        // Update is called once per frame
        void Update()
        {
            Assert.IsTrue(WiimoteManager.HasWiimote(), "A Wiimote must be connected");
            
            if (Wiimote.Button.a)
            {
                print("Calibrating Wiimote!");
                transform.SetPositionAndRotation(transform.position, StartingRotation);
                Wiimote.MotionPlus.SetZeroValues();
            }

            //TODO As well as making this more efficient, we can probs use the "slow mode" booleans to improve accuracy
            int ret;
            Vector3 gyroOffset = Vector3.zero;

            do {
                ret = Wiimote.ReadWiimoteData();
                //add all detected rotations throughout the frame to gyroOffset
                if (ret > 0 && Wiimote.current_ext == ExtensionController.MOTIONPLUS) {
                    gyroOffset += new Vector3(  -Wiimote.MotionPlus.PitchSpeed,
                                                    Wiimote.MotionPlus.YawSpeed,
                                                    Wiimote.MotionPlus.RollSpeed);
                }
            } while (ret > 0);

            gyroOffset /= 95f; //divide by 95 because of the average rate of sending messages of the wiimote is 95Hz
                            //and speeds of rotations are sent in degrees per second (i think!)
                            //would be cool to actually count the number of updates per second but I'm not sure how. 


            // ReadWiimoteData() returns 0 when nothing is left to read.
            // So by doing this we continue to update the Wiimote's attitude until it is "up to date."

            transform.Rotate(gyroOffset, Space.Self);

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

        void OnDisable()
        {
            CleanupWiimotes();
        }
    }

}
