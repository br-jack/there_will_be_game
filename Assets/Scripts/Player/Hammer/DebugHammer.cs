using System.Globalization;
using TMPro;
using UnityEngine;

namespace Hammer
{
/*
DebugHammer:
Shows some data about hammer (kinda outdated) and calls calibration function.
Basically all the DebugHammer scene functions.
*/
    public class DebugHammer : MonoBehaviour
    {
        [SerializeField] private TMP_Text pitchSpeedText;
        [SerializeField] private TMP_Text rollSpeedText;
        [SerializeField] private TMP_Text yawSpeedText;

        [SerializeField] private TMP_Text xAccelText;
        [SerializeField] private TMP_Text yAccelText;
        [SerializeField] private TMP_Text zAccelText;

        [SerializeField] private TMP_Text accelButtonText;
        [SerializeField] private TargetHammer hammerBehaviour;

        public void CalibrateHammer()
        {
            hammerBehaviour.CalibrateHammer();
            accelButtonText.text = $"Calibrated!";

        }
        void Update()
        {
            pitchSpeedText.text = $"X Rotation: {hammerBehaviour.transform.eulerAngles.x.ToString(CultureInfo.CurrentCulture)}";
            rollSpeedText.text = $"Y Rotation: {hammerBehaviour.transform.eulerAngles.y.ToString(CultureInfo.CurrentCulture)}";
            yawSpeedText.text = $"Z Rotation: {hammerBehaviour.transform.transform.eulerAngles.z.ToString(CultureInfo.CurrentCulture)}";
        }
    }
}

