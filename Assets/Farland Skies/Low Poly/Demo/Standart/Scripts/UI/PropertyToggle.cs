using System;
using UnityEngine;
using UnityEngine.UI;

namespace Borodar.FarlandSkies.LowPoly
{
    public class PropertyToggle : MonoBehaviour
    {
        public Type ToggleType;

        private Toggle _toggle;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected void Awake()
        {
            _toggle = GetComponent<Toggle>();
        }

        protected void Start()
        {
            switch (ToggleType)
            {
                case Type.SunFlare:
                    _toggle.isOn = SkyboxController.Instance.SunFlare;
                    break;
                case Type.MoonFlare:
                    _toggle.isOn = SkyboxController.Instance.MoonFlare;
                    break;
                case Type.AdjustFog:
                    _toggle.isOn = SkyboxController.Instance.AdjustFogColor;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void OnValueChanged(bool value)
        {
            switch (ToggleType)
            {
                case Type.SunFlare:
                    SkyboxController.Instance.SunFlare = value;
                    break;
                case Type.MoonFlare:
                    SkyboxController.Instance.MoonFlare = value;
                    break;
                case Type.AdjustFog:
                    SkyboxController.Instance.AdjustFogColor = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //---------------------------------------------------------------------
        // Nested
        //---------------------------------------------------------------------

        public enum Type
        {
            SunFlare,
            MoonFlare,
            AdjustFog
        }
    }
}