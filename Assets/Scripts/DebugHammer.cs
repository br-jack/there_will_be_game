using System.Globalization;
using TMPro;
using UnityEngine;
using WiimoteApi;

public class DebugHammer : MonoBehaviour
{
    public TMP_Text pitchSpeedText;
    public TMP_Text rollSpeedText;
    public TMP_Text yawSpeedText;

    private Wiimote _wiimote;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _wiimote = GetComponent<hammer_behaviour>().Wiimote;
    }

    public void CalibrateWiiMotionPlus()
    {
        Debug.Log("Calibrating WMP values");
        _wiimote.MotionPlus.SetZeroValues();
    }

    // Update is called once per frame
    void Update()
    {
        pitchSpeedText.text = $"Pitch Speed: {_wiimote.MotionPlus.PitchSpeed.ToString(CultureInfo.CurrentCulture)}";
        rollSpeedText.text = $"Roll Speed: {_wiimote.MotionPlus.RollSpeed.ToString(CultureInfo.CurrentCulture)}";
        yawSpeedText.text = $"Yaw Speed: {_wiimote.MotionPlus.YawSpeed.ToString(CultureInfo.CurrentCulture)}";
    }
}
