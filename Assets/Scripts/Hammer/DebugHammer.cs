using System.Globalization;
using Hammer;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using WiimoteApi;

namespace Hammer
{
    public class DebugHammer : MonoBehaviour
    {
        public TMP_Text pitchSpeedText;
        public TMP_Text rollSpeedText;
        public TMP_Text yawSpeedText;

        public TMP_Text xAccelText;
        public TMP_Text yAccelText;
        public TMP_Text zAccelText;

        public TMP_Text accelMessagesText;
        public TMP_Text accelButtonText;

        public GameObject backImage;
        public GameObject noseImage;
        public GameObject sideImage;

        public enum calibrationMode { BACK, NOSE, SIDE, NONE }
        private calibrationMode step = calibrationMode.NONE;

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
            sideImage.gameObject.SetActive(false);
            noseImage.gameObject.SetActive(false);
            backImage.gameObject.SetActive(false);


        }

        public void CalibrateAccelStep()
        {
            switch (step)
            {
                case calibrationMode.NONE:
                    // display back image
                    backImage.gameObject.SetActive(true);
                    accelMessagesText.text = $"Place your remote on its back then press next.";
                    accelButtonText.text = "Next";
                    this.step = calibrationMode.BACK;
                    break;
                case calibrationMode.BACK:
                    // display nose image
                    backImage.gameObject.SetActive(false);
                    noseImage.gameObject.SetActive(true);
                    accelMessagesText.text = "Place your remote on its nose then press next.";
                    _wiimote.Accel.CalibrateAccel(AccelCalibrationStep.A_BUTTON_UP);
                    this.step = calibrationMode.NOSE;
                    break;
                case calibrationMode.NOSE:
                    // display side image
                    sideImage.gameObject.SetActive(true);
                    noseImage.gameObject.SetActive(false);
                    accelMessagesText.text = "Place your remote on its right side then press next.";
                    _wiimote.Accel.CalibrateAccel(AccelCalibrationStep.EXPANSION_UP);
                    this.step = calibrationMode.SIDE;
                    break;
                case calibrationMode.SIDE:
                    // display calibrated image
                    sideImage.gameObject.SetActive(false);

                    accelMessagesText.text = "Calibration Complete.";
                    accelButtonText.text = "Calibrate Again";
                    _wiimote.Accel.CalibrateAccel(AccelCalibrationStep.LEFT_SIDE_UP);
                    this.step = calibrationMode.NONE;
                    break;
                
            }
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
            hb.Wiimote.MotionPlus.SetZeroValues();
        }

        // Update is called once per frame
        void Update()
        {
            pitchSpeedText.text = $"Pitch Speed: {_wiimote.MotionPlus.PitchSpeed.ToString(CultureInfo.CurrentCulture)}";
            rollSpeedText.text = $"Roll Speed: {_wiimote.MotionPlus.RollSpeed.ToString(CultureInfo.CurrentCulture)}";
            yawSpeedText.text = $"Yaw Speed: {_wiimote.MotionPlus.YawSpeed.ToString(CultureInfo.CurrentCulture)}";
            xAccelText.text = $"X Accel: {_wiimote.Accel.GetCalibratedAccelData()[0].ToString(CultureInfo.CurrentCulture)}";
            yAccelText.text = $"Y Accel: {_wiimote.Accel.GetCalibratedAccelData()[1].ToString(CultureInfo.CurrentCulture)}";
            zAccelText.text = $"Z Accel: {_wiimote.Accel.GetCalibratedAccelData()[2].ToString(CultureInfo.CurrentCulture)}";
        }
    }
}

