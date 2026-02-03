using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using WiimoteApi;

public class hammer_behaviour : MonoBehaviour
{

    public Wiimote Wiimote { get; private set; }
    
    private readonly Quaternion _startingRotation = Quaternion.Euler(new Vector3(270,0,0));

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
            Wiimote.SendPlayerLED(true, false, false, false);

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
            Wiimote.SendDataReportMode(InputDataType.REPORT_EXT21);
        }
    }
    
    private void CleanupWiimotes()
    {
        Debug.Log("Cleaning up Wiimote connections.");
        
        //Iterate from the end of the collection to prevent errors from the cleanup function removing each element
        for (int index = WiimoteManager.Wiimotes.Count - 1; index >= 0; index--)
        {
            Wiimote remote = WiimoteManager.Wiimotes[index];
            WiimoteManager.Cleanup(remote);
        }

        //Ensure reference to removed wiimote isn't used
        Wiimote = null;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Hammer should start flat with Pitch = 0
        ConnectWiimote();
    }

    // Update is called once per frame
    void Update()
    {   

        //TODO As well as making this more efficient, we can probs use the "slow mode" booleans to improve accuracy
        int ret;
        do {
            Assert.IsTrue(WiimoteManager.HasWiimote(), "A Wiimote must be connected");
            
            ret = Wiimote.ReadWiimoteData();
            //not sure re efficiency, this may be v slow and laggy
            transform.rotation *= Quaternion.Euler(
                Wiimote.MotionPlus.PitchSpeed/95, 
                Wiimote.MotionPlus.YawSpeed/95, 
                Wiimote.MotionPlus.RollSpeed/95);
        } while (ret > 0); // ReadWiimoteData() returns 0 when nothing is left to read.  
                // So by doing this we continue to update the Wiimote's attitude until it is "up to date."

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
}
