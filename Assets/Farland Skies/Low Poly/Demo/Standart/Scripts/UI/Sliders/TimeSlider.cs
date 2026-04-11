using UnityEngine;
using UnityEngine.UI;

namespace Borodar.FarlandSkies.LowPoly
{
    public class TimeSlider : MonoBehaviour
    {
        private Slider _slider;
        private SkyboxCycleManager _cycleManager;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected void Awake()
        {
            _slider = GetComponent<Slider>();
        }

        protected void Start()
        {
            _cycleManager = SkyboxCycleManager.Instance;
        }

        protected void Update()
        {
            _slider.value = _cycleManager.CycleProgress;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void OnValueChanged(float value)
        {
            _cycleManager.CycleProgress = value;
        }
    }
}