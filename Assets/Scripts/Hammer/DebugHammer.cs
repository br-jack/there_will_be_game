using System.Globalization;
using Hammer;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using WiimoteApi;

namespace Hammer
{
    public class DebugHammer : MonoBehaviour
    {
        public TMP_Text pitchSpeedText;
        public TMP_Text rollSpeedText;
        public TMP_Text yawSpeedText;

        private Wiimote _wiimote;

        private HammerBehaviour hb;
    
        void Awake()
        {
            hb = GetComponent<HammerBehaviour>();
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _wiimote = hb.Wiimote;
        }


        public void CalibrateWiiMotionPlus()
        {
            print("Calibrating Wiimote! Detected gyro speeds at moment of calibration: \nPitch: " 
                + hb.Wiimote.MotionPlus.PitchSpeed + 
                "\nRoll: "+hb.Wiimote.MotionPlus.RollSpeed+
                "\nYaw: "+hb.Wiimote.MotionPlus.YawSpeed);
            if(!hb.Wiimote.MotionPlus.PitchSlow) print("Also, wiimote is Pitching fast!");
            if(!hb.Wiimote.MotionPlus.YawSlow) print("Also, wiimote is Yawing fast!");
            if(!hb.Wiimote.MotionPlus.RollSlow) print("Also, wiimote is Rolling fast!");

            transform.SetPositionAndRotation(transform.position, hb.StartingRotation);
            hb.wiimoteAttitude = hb.StartingRotation;
            hb.Wiimote.MotionPlus.SetZeroValues();
            hb.Wiimote.Accel.CalibrateAccel(0);
        }

        // Update is called once per frame
        void Update()
        {
            pitchSpeedText.text = $"Pitch Speed: {_wiimote.MotionPlus.PitchSpeed.ToString(CultureInfo.CurrentCulture)}";
            rollSpeedText.text = $"Roll Speed: {_wiimote.MotionPlus.RollSpeed.ToString(CultureInfo.CurrentCulture)}";
            yawSpeedText.text = $"Yaw Speed: {_wiimote.MotionPlus.YawSpeed.ToString(CultureInfo.CurrentCulture)}";
        }
    }
}

