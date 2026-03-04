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
            accelButtonText.text = $"Calibrated!";

        }

        // Update is called once per frame
        void Update()
        {
            pitchSpeedText.text = $"X Rotation: {HammerBehaviour.Instance.transform.eulerAngles.x.ToString(CultureInfo.CurrentCulture)}";
            rollSpeedText.text = $"Y Rotation: {HammerBehaviour.Instance.transform.eulerAngles.y.ToString(CultureInfo.CurrentCulture)}";
            yawSpeedText.text = $"Z Rotation: {HammerBehaviour.Instance.transform.eulerAngles.z.ToString(CultureInfo.CurrentCulture)}";
        }
    }
}

