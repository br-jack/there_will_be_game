using System;
using UnityEngine;
using UnityEngine.UI;

namespace Borodar.FarlandSkies.LowPoly
{
    public class TimeText : MonoBehaviour
    {
        private Text _text;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected void Awake()
        {
            _text = GetComponent<Text>();
        }

        protected void Update()
        {
            var timeSpan = TimeSpan.FromHours(SkyboxCycleManager.Instance.CycleProgress * 0.24f);
            _text.text = string.Format("{0:D2}:{1:D2}", timeSpan.Hours, timeSpan.Minutes);
        }
    }
}