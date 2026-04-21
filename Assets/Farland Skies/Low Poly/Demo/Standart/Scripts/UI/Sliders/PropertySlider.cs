using System;
using UnityEngine;
using UnityEngine.UI;

namespace Borodar.FarlandSkies.LowPoly
{
    public class PropertySlider : MonoBehaviour
    {
        public Type SliderType;
        private Slider _slider;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected void Awake()
        {
            _slider = GetComponent<Slider>();
        }

        protected void Start()
        {
            switch (SliderType)
            {
                case Type.SkyBottomExponent:
                    _slider.value = SkyboxController.Instance.BottomExponent;
                    break;
                case Type.SkyTopExponent:
                    _slider.value = SkyboxController.Instance.TopExponent;
                    break;
                case Type.StarsExtinction:
                    _slider.value = SkyboxController.Instance.StarsExtinction;
                    break;
                case Type.StarsTwinkling:
                    _slider.value = SkyboxController.Instance.StarsTwinklingSpeed;
                    break;
                case Type.SunAlpha:
                    _slider.value = SkyboxController.Instance.SunTint.a;
                    break;
                case Type.SunSize:
                    _slider.value = SkyboxController.Instance.SunSize;
                    break;
                case Type.SunFlareBrightness:
                    _slider.value = SkyboxController.Instance.SunFlareBrightness;
                    break;
                case Type.MoonAlpha:
                    _slider.value = SkyboxController.Instance.MoonTint.a;
                    break;
                case Type.MoonSize:
                    _slider.value = SkyboxController.Instance.MoonSize;
                    break;
                case Type.MoonFlareBrightness:
                    _slider.value = SkyboxController.Instance.MoonFlareBrightness;
                    break;
                case Type.CloudsRotation:
                    _slider.value = SkyboxController.Instance.CloudsRotation;
                    break;
                case Type.CloudsHeight:
                    _slider.value = SkyboxController.Instance.CloudsHeight;
                    break;
                case Type.Exposure:
                    _slider.value = SkyboxController.Instance.Exposure;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void OnValueChanged(float value)
        {
            switch (SliderType)
            {
                case Type.SkyBottomExponent:
                    SkyboxController.Instance.BottomExponent = value;
                    break;
                case Type.SkyTopExponent:
                    SkyboxController.Instance.TopExponent = value;
                    break;
                case Type.StarsExtinction:
                    SkyboxController.Instance.StarsExtinction = value;
                    break;
                case Type.StarsTwinkling:
                    SkyboxController.Instance.StarsTwinklingSpeed = value;
                    break;
                case Type.SunAlpha:
                    var sunTint = SkyboxController.Instance.SunTint;
                    sunTint.a = value;
                    SkyboxController.Instance.SunTint = sunTint;
                    break;
                case Type.SunSize:
                    SkyboxController.Instance.SunSize = value;
                    break;
                case Type.SunFlareBrightness:
                    SkyboxController.Instance.SunFlareBrightness = value;
                    break;
                case Type.MoonAlpha:
                    var moonTint = SkyboxController.Instance.MoonTint;
                    moonTint.a = value;
                    SkyboxController.Instance.MoonTint = moonTint;
                    break;
                case Type.MoonSize:
                    SkyboxController.Instance.MoonSize = value;
                    break;
                case Type.MoonFlareBrightness:
                    SkyboxController.Instance.MoonFlareBrightness = value;
                    break;
                case Type.CloudsRotation:
                    SkyboxController.Instance.CloudsRotation = value;
                    break;
                case Type.CloudsHeight:
                    SkyboxController.Instance.CloudsHeight = value;
                    break;
                case Type.Exposure:
                    SkyboxController.Instance.Exposure = value;
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
            SkyTopExponent,
            SkyBottomExponent,
            StarsExtinction,
            StarsTwinkling,
            SunAlpha,
            SunSize,
            SunFlareBrightness,
            MoonAlpha,
            MoonSize,
            MoonFlareBrightness,
            CloudsRotation,
            CloudsHeight,
            Exposure
        }
    }
}