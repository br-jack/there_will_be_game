using Hammer;
using System.Globalization;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEngine.Analytics.IAnalytic;


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

        [SerializeField] private TMP_Text accelButtonText;
        [SerializeField] private HammerBehaviour hammerBehaviour;

        void Awake()
        {
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
   

        }
        
        public void CalibrateHammer()
        {
            hammerBehaviour.CalibrateHammer();
            accelButtonText.text = $"Calibrated!";

        }

        // Update is called once per frame
        void Update()
        {
            pitchSpeedText.text = $"X Rotation: {hammerBehaviour.transform.eulerAngles.x.ToString(CultureInfo.CurrentCulture)}";
            rollSpeedText.text = $"Y Rotation: {hammerBehaviour.transform.eulerAngles.y.ToString(CultureInfo.CurrentCulture)}";
            yawSpeedText.text = $"Z Rotation: {hammerBehaviour.transform.transform.eulerAngles.z.ToString(CultureInfo.CurrentCulture)}";
        }
    }
}

