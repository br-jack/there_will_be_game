using System.Globalization;
using Hammer;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
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

        public GameObject backImage;
        public GameObject noseImage;
        public GameObject sideImage;

        private HammerBehaviour hb;

        void Awake()
        {
            hb = GetComponent<HammerBehaviour>();
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            sideImage.gameObject.SetActive(false);
            noseImage.gameObject.SetActive(false);
            backImage.gameObject.SetActive(false);

        }

        public void CalibrateWiiMotionPlus()
        {

            hb.CallibrateHammer();
            print("Callibrated!");

        }

        // Update is called once per frame
        void Update()
        {
            //    pitchSpeedText.text = $"Pitch Speed: {WiimoteGlobal.wiimote.MotionPlus.PitchSpeed.ToString(CultureInfo.CurrentCulture)}";
            //    rollSpeedText.text = $"Roll Speed: {WiimoteGlobal.wiimote.MotionPlus.RollSpeed.ToString(CultureInfo.CurrentCulture)}";
            //    yawSpeedText.text = $"Yaw Speed: {WiimoteGlobal.wiimote.MotionPlus.YawSpeed.ToString(CultureInfo.CurrentCulture)}";
            //    xAccelText.text = $"X Accel: {WiimoteGlobal.wiimote.Accel.GetCalibratedAccelData()[0].ToString(CultureInfo.CurrentCulture)}";
            //    yAccelText.text = $"Y Accel: {WiimoteGlobal.wiimote.Accel.GetCalibratedAccelData()[1].ToString(CultureInfo.CurrentCulture)}";
            //    zAccelText.text = $"Z Accel: {WiimoteGlobal.wiimote.Accel.GetCalibratedAccelData()[2].ToString(CultureInfo.CurrentCulture)}";
        }
    }
}

