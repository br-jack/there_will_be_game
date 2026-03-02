using System;
using System.Globalization;
using Hammer;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using WiimoteApi;
using UnityEngine.SceneManagement;

public class WiimoteGlobal : IRotatable
{
    private Wiimote _wiimote;
    
    private bool ConnectWiimote() {
        if (WiimoteManager.HasWiimote())
        {
            Debug.LogWarning("Attempting to find a Wiimote even though one is already connected!");
        }
        
        WiimoteManager.FindWiimotes(); // Poll native bluetooth drivers to find Wiimotes

        if (WiimoteManager.HasWiimote())
        {
            Assert.IsTrue(WiimoteManager.Wiimotes.Count == 1, "Only one Wiimote should be connected at a time");
            //Use the first wiimote: others ignored
            _wiimote = WiimoteManager.Wiimotes[0];

            //If the wiimote wasn't connected through dolphin, it may still have blinking lights
            //even though it actually is still connected
            _wiimote.SendPlayerLED(true, false, false, true);

            if (_wiimote.Type == WiimoteType.WIIMOTEPLUS)
            {
                //Running RequestIdentifyWiiMotionPlus() on a Wiimote Plus unfortunately fails,
                //so we have to skip that check.
                Debug.Log("Wii Remote Plus detected, skipping Motion Plus check.");
                _wiimote.ActivateWiiMotionPlus();
            }
            else
            {
                _wiimote.RequestIdentifyWiiMotionPlus();

                if (_wiimote.wmp_attached)
                {
                    _wiimote.ActivateWiiMotionPlus();
                    Debug.Log("Connected with Wii Motion Plus Extension.");
                }
                else
                {
                    Debug.LogWarning("Wii remote doesn't have motion plus :(");
                }
            }
            
            //Default input mode only sends button data, so for accelerometer / gyro data 
            //we need to request a mode with extension bytes
            _wiimote.SendDataReportMode(InputDataType.REPORT_BUTTONS_ACCEL_EXT16);

            return true;
        }

        return false;
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
        _wiimote = null;
    }
    
    private Vector3 UseWiimoteData()
    {
        Assert.IsTrue(WiimoteManager.HasWiimote(), "A Wiimote must be connected");

        //TODO As well as making this more efficient, we can probs use the "slow mode" booleans to improve accuracy
        int ret;
        Vector3 gyroOffset = Vector3.zero;
        Vector3 accelOffset = Vector3.zero;

        do {
            ret = _wiimote.ReadWiimoteData();

            //ACCELEROMETER
            
            Vector3 accelDataForFrameTest = new Vector3(
                _wiimote.Accel.GetCalibratedAccelData()[0],
                _wiimote.Accel.GetCalibratedAccelData()[1],
                _wiimote.Accel.GetCalibratedAccelData()[2]);
            Debug.Log("Accel data: "+accelDataForFrameTest);
            accelOffset += accelDataForFrameTest;
            
            //this is a test basically

            //GYROSCOPE
            //add all detected rotations throughout the frame to gyroOffset
            //we should integrate this! would give accurate total rotation
            if (ret > 0 && _wiimote.current_ext == ExtensionController.MOTIONPLUS) {
                gyroOffset += new Vector3(  -_wiimote.MotionPlus.PitchSpeed,
                                                -_wiimote.MotionPlus.RollSpeed,
                                                -_wiimote.MotionPlus.YawSpeed);
            }
        } while (ret > 0);

        gyroOffset /= 95f; //divide by 95 because of the average rate of sending messages of the wiimote is 95Hz
                        //and speeds of rotations are sent in degrees per second (i think!)
                        //would be cool to actually count the number of updates per second but I'm not sure how. 
                        //i think that this is completely wrong. nothing like 95 messages sent per frame. but idk
                        //oh wait yeah ofc its wrong, we are not 1 frame per second!! 



        // ReadWiimoteData() returns 0 when nothing is left to read.
        // So by doing this we continue to update the Wiimote's attitude until it is "up to date."

        return gyroOffset;
        
        //print("Total accel offset for frame: "+accelOffset);
        //transform.Translate(accelOffset/95f, Space.Self);
        //just using accel values/100 for translation - makes no sense but testing!
    }

    public void Connect()
    {
        ConnectWiimote();
    }

    public Vector3 GetRotationOffset()
    {
        return UseWiimoteData();
    }

    public void Cleanup()
    {
        CleanupWiimotes();
    }
}