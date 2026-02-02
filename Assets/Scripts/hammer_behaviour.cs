using System;
using UnityEditor;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine;
using WiimoteApi;

public class hammer_behaviour : MonoBehaviour
{
    //probs should be an enum - can be either "Unity Remote", "Wiimote", or "None"
    private string hammerRemote;


    public Wiimote wiimote;

    
    
    static public Quaternion startingRotation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    //maybe should have this script give the wiimote script a wiimote object, and the same for the sensor script. 
    
    //Prioritises wii remotes if both are connected
    void ConnectController()

    //most of this (at least for wii remotes) should probs just be done every frame
    {
        //clunky ifs and elses but not sure how to make logic clearer!
        if (EditorApplication.isRemoteConnected) {
            Input.gyro.enabled = true;
            hammerRemote = "Unity Remote";
        } else {

            WiimoteManager.FindWiimotes(); // Poll native bluetooth drivers to find Wiimotes

            if(WiimoteManager.HasWiimote()) {
                wiimote = WiimoteManager.Wiimotes[0];
                if (wiimote.RequestIdentifyWiiMotionPlus())
                {
                    wiimote.ActivateWiiMotionPlus();
                    hammerRemote = "Wiimote";
                } else print("Wii remote doesn't have motion plus :(");

            } else {
                hammerRemote = "None";
                print("Couldn't find any controllers :(");
            }

        }
    }

    void Start()
    {
        hammerRemote = "None";

        //Change this to pressing a button on the controller
        print("Press Space to set up controllers");

        //Hammer should start flat with Pitch = 0
        startingRotation = Quaternion.Euler(new Vector3(270,0,0));

        //ConnectController();

        
    }

    // Update is called once per frame
    void Update()
    {   
        switch(hammerRemote)
        {
            case "Wiimote":

                WiimoteManager.FindWiimotes(); // Poll native bluetooth drivers to find Wiimotes

                if(WiimoteManager.HasWiimote()) {
                    //Use the first wiimote: others ignored
                    wiimote = WiimoteManager.Wiimotes[0];
                    hammerRemote = "Wiimote";
                    if (wiimote.RequestIdentifyWiiMotionPlus())
                        {
                            wiimote.ActivateWiiMotionPlus();
                            
                        } else print("Wii remote doesn't have motion plus :((");
                }

            
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
                break;
            case "Unity Remote":
                transform.rotation = Quaternion.Inverse(Input.gyro.attitude * startingRotation);
                break;
            default: 
                if (Input.GetKeyDown(KeyCode.Space)) {
                    print("Looking for connected controllers");
                    ConnectController();
                    }
                break;
        }
        
    }
}
