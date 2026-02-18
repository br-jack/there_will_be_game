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

        

        public Vector3 wiimoteAttitude; //should be private, can change later as only one method uses i think!

        public float accelAdjustmentRatio; //0.02 seems reasonable

        
        //debug
        public bool accelEnabled;
        public bool gyroEnabled;
        public float accelPitch;
        public float accelRoll;

        public float accelPitchReordered;
        public float accelRollReordered;

        public Quaternion accelAttitude;
        public Quaternion gyroAttitude;


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
            wiimoteAttitude = Vector3.zero; //have to set it to something! assume same as hammer
            ConnectWiimote();
        }

        // Update is called once per frame
        void Update()
        {
            Assert.IsTrue(WiimoteManager.HasWiimote(), "A Wiimote must be connected");

            Wiimote wm = WiimoteGlobal.wiimote;
            
            //WE APPLY ROTATIONS IN THE ORDER: ROLL, PITCH, YAW
            //unity does it in this order naturally, but it's a good order!
            int ret;
            do
            {
                ret = wm.ReadWiimoteData();
                Vector3 accel = new Vector3(
                    wm.Accel.GetCalibratedAccelData()[0],
                    wm.Accel.GetCalibratedAccelData()[1],
                    wm.Accel.GetCalibratedAccelData()[2]);
                Vector3 gyro = new Vector3(  
                    -wm.MotionPlus.PitchSpeed,
                    wm.MotionPlus.YawSpeed,
                    -wm.MotionPlus.RollSpeed)/95f;

                
                if (gyroEnabled) transform.Rotate(gyro);
                
                wiimoteAttitude = transform.rotation.eulerAngles; //seems silly. remove wiimoteattitude i think

                if (accelEnabled) {

                    accelRoll = Mathf.Rad2Deg *  Mathf.Atan2(-accel.x,accel.z);
                    accelPitch = Mathf.Rad2Deg * Mathf.Atan2(accel.y,Mathf.Sqrt(Mathf.Pow(accel.x,2)+Mathf.Pow(accel.z,2)));

                    accelRollReordered = Mathf.Rad2Deg * Mathf.Atan2(-accel.x,Mathf.Sqrt(Mathf.Pow(accel.y,2)+Mathf.Pow(accel.z,2)));
                    accelPitchReordered = Mathf.Rad2Deg * Mathf.Atan2(accel.y,accel.z);

                    //roll white hammer w/ main axis system
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation, 
                        Quaternion.Euler(wiimoteAttitude.x,wiimoteAttitude.y,accelRoll),
                        accelAdjustmentRatio);
                    
                    //pitch white hammer w/ main axis system
                    wiimoteAttitude = transform.rotation.eulerAngles;
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        Quaternion.Euler(accelPitch,wiimoteAttitude.y,wiimoteAttitude.z),
                        accelAdjustmentRatio);

                    
                    Transform agh = GameObject.Find("accelGhostHammer").transform;
                    Transform argh = GameObject.Find("accelReorderedGhostHammer").transform;
                    //set green hammer to main axis system estimate
                    agh.rotation = Quaternion.Euler(new Vector3(accelPitch,0,accelRoll));

                    //set purple hammer to reordered axis system estimate
                    argh.rotation = Quaternion.Euler(new Vector3(accelPitchReordered,0,0))
                        *Quaternion.AngleAxis(accelRollReordered,Vector3.forward);
                    if (accel.z < 0) argh.Rotate(new Vector3(0,180,0),Space.World);
                    //float regularProportion = Mathf.Pow(Mathf.Cos(accelRoll),2);
                    float reorderedProportion = Mathf.Abs(accel.y);// maybe something like Mathf.Pow(Mathf.Cos(Mathf.Deg2Rad*accelRoll),2)
                    print("reordered proportion: "+reorderedProportion);
                    GameObject.Find("accelCombinedGhostHammer").transform.rotation = 
                        Quaternion.Slerp(agh.rotation,argh.rotation,reorderedProportion);
                }
                
            } while (ret > 0);

            
            

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
