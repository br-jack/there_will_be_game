using System;
using Borodar.FarlandSkies.Core.Demo;
using UnityEngine;

namespace Borodar.FarlandSkies.LowPoly
{
    public class ColorButton : BaseColorButton
    {
        public ColorType SkyColorType;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected void Start()
        {
            switch (SkyColorType)
            {
                case ColorType.Top:
                    ColorImage.color = SkyboxController.Instance.TopColor;
                    break;
                case ColorType.Middle:
                    ColorImage.color = SkyboxController.Instance.MiddleColor;
                    break;
                case ColorType.Bottom:
                    ColorImage.color = SkyboxController.Instance.BottomColor;
                    break;
                case ColorType.StarsTint:
                    ColorImage.color = SkyboxController.Instance.StarsTint;
                    break;
                case ColorType.SunTint:
                    ColorImage.color = SkyboxController.Instance.SunTint;
                    break;
                case ColorType.MoonTint:
                    ColorImage.color = SkyboxController.Instance.MoonTint;
                    break;
                case ColorType.CloudTint:
                    ColorImage.color = SkyboxController.Instance.CloudsTint;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override void ChangeColor(Color color)
        {
            base.ChangeColor(color);

            switch (SkyColorType)
            {
                case ColorType.Top:
                    SkyboxController.Instance.TopColor = color;
                    break;
                case ColorType.Middle:
                    SkyboxController.Instance.MiddleColor = color;
                    break;
                case ColorType.Bottom:
                    SkyboxController.Instance.BottomColor = color;
                    break;
                case ColorType.StarsTint:
                    SkyboxController.Instance.StarsTint = color;
                    break;
                case ColorType.SunTint:
                    color.a = SkyboxController.Instance.SunTint.a;
                    SkyboxController.Instance.SunTint = color;
                    break;
                case ColorType.MoonTint:
                    color.a = SkyboxController.Instance.MoonTint.a;
                    SkyboxController.Instance.MoonTint = color;
                    break;
                case ColorType.CloudTint:
                    SkyboxController.Instance.CloudsTint = color;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //---------------------------------------------------------------------
        // Nested
        //---------------------------------------------------------------------

        public enum ColorType
        {
            Top,
            Middle,
            Bottom,
            StarsTint,
            SunTint,
            MoonTint,
            CloudTint
        }
    }
}