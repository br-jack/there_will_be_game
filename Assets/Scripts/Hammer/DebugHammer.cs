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
        [SerializeField] private TMP_Text pitchSpeedText;
        [SerializeField] private TMP_Text rollSpeedText;
        [SerializeField] private TMP_Text yawSpeedText;

        [SerializeField] private TMP_Text xAccelText;
        [SerializeField] private TMP_Text yAccelText;
        [SerializeField] private TMP_Text zAccelText;

        [SerializeField] private TMP_Text accelMessagesText;
        [SerializeField] private TMP_Text accelButtonText;


        void Awake()
        {
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
   

        }
        
        public void CalibrateHammer()
        {
            HammerBehaviour.Instance.CalibrateHammer();
            print("Calibrated!");
            accelButtonText.text = $"Calibrated!";

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

