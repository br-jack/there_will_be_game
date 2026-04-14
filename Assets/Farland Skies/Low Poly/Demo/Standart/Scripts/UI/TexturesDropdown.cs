using System;
using UnityEngine;
using UnityEngine.UI;

namespace Borodar.FarlandSkies.LowPoly
{
    public class TexturesDropdown : MonoBehaviour
    {

        [SerializeField]
        protected TextureType Type;

        [SerializeField]
        protected Texture[] Textures;

        private Dropdown _dropdown;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected void Awake()
        {
            _dropdown = GetComponent<Dropdown>();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void OnValueChanged()
        {
            var value = _dropdown.value;
            switch (Type)
            {
                case TextureType.Stars:
                    SkyboxController.Instance.StarsCubemap = Textures[value] as Cubemap;
                    break;
                case TextureType.Sun:
                    SkyboxController.Instance.SunTexture = Textures[value] as Texture2D;
                    break;
                case TextureType.Moon:
                    SkyboxController.Instance.MoonTexture = Textures[value] as Texture2D;
                    break;
                case TextureType.Clouds:
                    SkyboxController.Instance.CloudsCubemap = Textures[value] as Cubemap;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //---------------------------------------------------------------------
        // Nested
        //---------------------------------------------------------------------

        public enum TextureType
        {
            Stars,
            Sun,
            Moon,
            Clouds
        }
    }
}