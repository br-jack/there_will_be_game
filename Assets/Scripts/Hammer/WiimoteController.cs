using UnityEngine;
using UnityEngine.Assertions;
using WiimoteApi;

namespace Hammer
{
    public class WiimoteController : IController
    {

        private Wiimote _wiimote;
        
        public void Connect()
        {
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
                        Debug.Log("Connected with Wii Motion Plus Extension.");
                    }
                    else
                    {
                        Debug.LogWarning("Wii remote doesn't have motion plus :(");
                    }
                    _wiimote.ActivateWiiMotionPlus();
                }
                
                //Default input mode only sends button data, so for accelerometer / gyro data 
                //we need to request a mode with extension bytes
                _wiimote.SendDataReportMode(InputDataType.REPORT_BUTTONS_ACCEL_EXT16);
            }
        }

        public void Update()
        {
            Assert.IsTrue(WiimoteManager.HasWiimote(), "A Wiimote must be connected");
        }

        public Quaternion GetAttitude()
        {
            throw new System.NotImplementedException();
        }

        public Vector3 GetAcceleration()
        {
            throw new System.NotImplementedException();
        }

        public void Cleanup()
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
    }
}