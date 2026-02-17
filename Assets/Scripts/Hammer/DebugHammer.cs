using System.Globalization;
using Hammer;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using WiimoteApi;
using UnityEngine.SceneManagement;

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

        public TMP_Text accelPitchText;
        public TMP_Text accelRollText;

        public GameObject backImage;
        public GameObject noseImage;
        public GameObject sideImage;



        public int[,] savedAccelCalib;
        public bool saveAccelCalib;
        public bool useAccelCalib;


        public enum calibrationMode { BACK, NOSE, SIDE, NONE }
        private calibrationMode step = calibrationMode.NONE;

        


        private HammerBehaviour hb;

        void Awake()
        {
            hb = GetComponent<HammerBehaviour>();
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {   
            if (useAccelCalib) WiimoteGlobal.wiimote.Accel.accel_calib = savedAccelCalib;
            sideImage.gameObject.SetActive(false);
            noseImage.gameObject.SetActive(false);
            backImage.gameObject.SetActive(false);

        }
        public void SceneSwitch()
        {
            SceneManager.LoadScene("MainScene");
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
                    WiimoteGlobal.wiimote.Accel.CalibrateAccel(AccelCalibrationStep.A_BUTTON_UP);
                    this.step = calibrationMode.NOSE;
                    break;
                case calibrationMode.NOSE:
                    // display side image
                    sideImage.gameObject.SetActive(true);
                    noseImage.gameObject.SetActive(false);
                    accelMessagesText.text = "Place your remote on its right side then press next.";
                    WiimoteGlobal.wiimote.Accel.CalibrateAccel(AccelCalibrationStep.EXPANSION_UP);
                    this.step = calibrationMode.SIDE;
                    break;
                case calibrationMode.SIDE:
                    // display calibrated image
                    sideImage.gameObject.SetActive(false);

                    accelMessagesText.text = "Calibration Complete.";
                    accelButtonText.text = "Calibrate Again";
                    WiimoteGlobal.wiimote.Accel.CalibrateAccel(AccelCalibrationStep.LEFT_SIDE_UP);
                    if (saveAccelCalib) savedAccelCalib = WiimoteGlobal.wiimote.Accel.accel_calib; //this doesn't work, you have to manually copy and then paste back into public field
                    this.step = calibrationMode.NONE;
                    break;
                
            }
        }
    
        

        public void CalibrateWiiMotionPlus()
        {
            print("Calibrating Wiimote! Detected gyro speeds at moment of calibration: \nPitch: " 
                + WiimoteGlobal.wiimote.MotionPlus.PitchSpeed + 
                "\nRoll: "+WiimoteGlobal.wiimote.MotionPlus.RollSpeed+
                "\nYaw: "+WiimoteGlobal.wiimote.MotionPlus.YawSpeed);
            if(!WiimoteGlobal.wiimote.MotionPlus.PitchSlow) print("Also, wiimote is Pitching fast!");
            if(!WiimoteGlobal.wiimote.MotionPlus.YawSlow) print("Also, wiimote is Yawing fast!");
            if(!WiimoteGlobal.wiimote.MotionPlus.RollSlow) print("Also, wiimote is Rolling fast!");

            hb.transform.SetPositionAndRotation(hb.transform.position, Quaternion.identity);
            WiimoteGlobal.wiimote.MotionPlus.SetZeroValues();
            hb.wiimoteAttitude = Vector3.zero;
        }

        // Update is called once per frame
        void Update()
        {
            pitchSpeedText.text = $"Pitch Speed: {WiimoteGlobal.wiimote.MotionPlus.PitchSpeed.ToString(CultureInfo.CurrentCulture)}";
            rollSpeedText.text = $"Roll Speed: {WiimoteGlobal.wiimote.MotionPlus.RollSpeed.ToString(CultureInfo.CurrentCulture)}";
            yawSpeedText.text = $"Yaw Speed: {WiimoteGlobal.wiimote.MotionPlus.YawSpeed.ToString(CultureInfo.CurrentCulture)}";
            xAccelText.text = $"X Accel: {WiimoteGlobal.wiimote.Accel.GetCalibratedAccelData()[0].ToString(CultureInfo.CurrentCulture)}";
            yAccelText.text = $"Y Accel: {WiimoteGlobal.wiimote.Accel.GetCalibratedAccelData()[1].ToString(CultureInfo.CurrentCulture)}";
            zAccelText.text = $"Z Accel: {WiimoteGlobal.wiimote.Accel.GetCalibratedAccelData()[2].ToString(CultureInfo.CurrentCulture)}";
            accelPitchText.text = $"Accel Pitch: {hb.accelPitch}";
            accelRollText.text = $"Accel Roll: {hb.accelRoll}";
        }
        }
    }


