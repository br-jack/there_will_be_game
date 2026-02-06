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

        private Vector3 wmpOffset = Vector3.zero;


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
        void Awake()
        {
            StartingRotation = transform.rotation;
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
            do {
                ret = Wiimote.ReadWiimoteData();
                if (ret > 0 && Wiimote.current_ext == ExtensionController.MOTIONPLUS) {
                    Vector3 offset = new Vector3(  -Wiimote.MotionPlus.PitchSpeed,
                                                    Wiimote.MotionPlus.YawSpeed,
                                                    Wiimote.MotionPlus.RollSpeed) / 95f; // Divide by 95Hz (average updates per second from wiimote)
                    wmpOffset += offset;

                    transform.Rotate(offset, Space.Self);
                }
            } while (ret > 0);
            // ReadWiimoteData() returns 0 when nothing is left to read.
            // So by doing this we continue to update the Wiimote's attitude until it is "up to date."

            

            /*
            while (ret > 0) {
                //not sure re efficiency, this may be v slow and laggy
                transform.Rotate( new Vector3(
                    -Wiimote.MotionPlus.PitchSpeed, 
                    -Wiimote.MotionPlus.RollSpeed, 
                    -Wiimote.MotionPlus.YawSpeed) / 190f);
                
                ret = Wiimote.ReadWiimoteData();
            }   
            */
           

            //Unity Remote
            //transform.rotation = Quaternion.Inverse(Input.gyro.attitude * _startingRotation);


            /*
            also doesn't work because of input system
            if (Input.GetKeyDown(KeyCode.Space)) {
                print("Looking for connected controllers");
                ConnectController();
                }
            */
            
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
