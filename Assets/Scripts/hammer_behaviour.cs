using System;
using UnityEditor;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine;
using WiimoteApi;

public class hammer_behaviour : MonoBehaviour
{

    public Wiimote wiimote;

    static void ConnectWiimote(Wiimote wiimote) {
         WiimoteManager.FindWiimotes(); // Poll native bluetooth drivers to find Wiimotes

        if(WiimoteManager.HasWiimote()) {
            //Use the first wiimote: others ignored
            wiimote = WiimoteManager.Wiimotes[0];

            if (wiimote.RequestIdentifyWiiMotionPlus())
                {
                    wiimote.ActivateWiiMotionPlus();
                    print("connected with wiimotionplus");
                } else print("Wii remote doesn't have motion plus :((");
        }
    }
    
    static public Quaternion startingRotation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Hammer should start flat with Pitch = 0
        startingRotation = Quaternion.Euler(new Vector3(270,0,0));

        ConnectWiimote(wiimote);

        
    }

    // Update is called once per frame
    void Update()
    {   
       


        //As well as making this more efficient, we can probs use the "slow mode" booleans to improve accuracy
        int ret;
        do {
            ret = wiimote.ReadWiimoteData();
            //not sure re efficiency, this may be v slow and laggy
            transform.rotation *= Quaternion.Euler(
                wiimote.MotionPlus.PitchSpeed/95, 
                wiimote.MotionPlus.YawSpeed/95, 
                wiimote.MotionPlus.RollSpeed/95);
        } while (ret > 0); // ReadWiimoteData() returns 0 when nothing is left to read.  
                // So by doing this we continue to update the Wiimote's attitude until it is "up to date."


        transform.rotation = Quaternion.Inverse(Input.gyro.attitude * startingRotation);


        /*
        also doesn't work because of input system
        if (Input.GetKeyDown(KeyCode.Space)) {
            print("Looking for connected controllers");
            ConnectController();
            }
        */
        
        
        
    }
}
