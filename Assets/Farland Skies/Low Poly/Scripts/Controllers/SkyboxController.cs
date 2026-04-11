using Borodar.FarlandSkies.Core.Helpers;
using UnityEngine;

namespace Borodar.FarlandSkies.LowPoly
{
    [ExecuteInEditMode]
    [HelpURL("http://www.borodar.com/stuff/farlandskies/lowpoly/docs/QuickStart_v2.5.2.pdf")]
    public class SkyboxController : Singleton<SkyboxController>
    {
        public Material SkyboxMaterial;

        // Sky

        [SerializeField]
        [Tooltip("Color at the top pole of skybox sphere")]
        private Color _topColor = Color.gray;

        [SerializeField]
        [Tooltip("Color on equator of skybox sphere")]
        private Color _middleColor = Color.gray;

        [SerializeField]
        [Tooltip("Color at the bottom pole of skybox sphere")]
        private Color _bottomColor = Color.gray;

        [SerializeField]
        [Range(0.01f, 5f)]
        [Tooltip("Color interpolation coefficient between top pole and equator")]
        private float _topExponent = 1f;

        [SerializeField]
        [Range(0.01f, 5f)]
        [Tooltip("Color interpolation coefficient between bottom pole and equator")]
        private float _bottomExponent = 1f;

        // Stars

        [SerializeField]
        private bool _starsEnabled = true;

        [SerializeField]
        private Cubemap _starsCubemap;

        [SerializeField]
        private Color _starsTint = Color.gray;

        [SerializeField]
        [Range(0f, 10f)]
        [Tooltip("Reduction in stars apparent brightness closer to the horizon")]
        private float _starsExtinction = 2f;

        [SerializeField]
        [Range(0f, 25f)]
        [Tooltip("Variation in stars apparent brightness caused by the atmospheric turbulence")]
        private float _starsTwinklingSpeed = 10f;

        // Sun

        [SerializeField]
        private bool _sunEnabled = true;

        [SerializeField]
        private Texture2D _sunTexture;

        [SerializeField]
        private Light _sunLight;

        [SerializeField]
        private Color _sunTint = Color.gray;

        [SerializeField]
        [Range(0.1f, 3f)]
        private float _sunSize = 1f;
        
        [SerializeField]
        [Range(0f, 2f)]
        private float _sunHalo = 1f;

        [SerializeField]
        private bool _sunFlare = true;

        [SerializeField]
        [Range(0.01f, 2f)]
        [Tooltip("Actual flare brightness depends on sun tint alpha, and this property is just a coefficient for that value")]
        private float _sunFlareBrightness = 0.3f;

        // Moon

        [SerializeField]
        private bool _moonEnabled = true;

        [SerializeField]
        private Texture2D _moonTexture;

        [SerializeField]
        private Light _moonLight;

        [SerializeField]
        private Color _moonTint = Color.gray;

        [SerializeField]
        [Range(0.1f, 3f)]
        private float _moonSize = 1f;
        
        [SerializeField]
        [Range(0f, 2f)]
        private float _moonHalo = 1f;

        [SerializeField]
        private bool _moonFlare = true;

        [SerializeField]
        [Range(0.01f, 2f)]
        [Tooltip("Actual flare brightness depends on moon tint alpha, and this property is just a coefficient for that value")]
        private float _moonFlareBrightness = 0.3f;

        // Clouds

        [SerializeField]
        private bool _cloudsEnabled = true;

        [SerializeField]
        private Cubemap _cloudsCubemap;

        [SerializeField]
        private Color _cloudsTint = Color.gray;        

        [SerializeField]
        [Range(-0.75f, 0.75f)]
        [Tooltip("Height of the clouds relative to the horizon.")]
        private float _cloudsHeight;

        [SerializeField]
        [Range(0, 360f)]
        [Tooltip("Rotation of the clouds around the positive y axis.")]
        private float _cloudsRotation;

        // General

        [SerializeField]
        [Range(0, 8f)]
        private float _exposure = 1f;        

        [SerializeField]
        [Tooltip("Keep fog color in sync with the sky middle color automatically")]
        private bool _adjustFogColor;

        // Private

        private LensFlare _sunFlareComponent;
        private LensFlare _moonFlareComponent;

        //---------------------------------------------------------------------
        // Shader Properties
        //---------------------------------------------------------------------

        private static readonly int TOP_COLOR = Shader.PropertyToID("_TopColor");
        private static readonly int MIDDLE_COLOR = Shader.PropertyToID("_MiddleColor");
        private static readonly int BOTTOM_COLOR = Shader.PropertyToID("_BottomColor");
        private static readonly int TOP_EXPONENT = Shader.PropertyToID("_TopExponent");
        private static readonly int BOTTOM_EXPONENT = Shader.PropertyToID("_BottomExponent");

        private static readonly int STARS_TINT = Shader.PropertyToID("_StarsTint");
        private static readonly int STARS_TEX = Shader.PropertyToID("_StarsTex");
        private static readonly int STARS_EXTINCTION = Shader.PropertyToID("_StarsExtinction");
        private static readonly int STARS_TWINKLING_SPEED = Shader.PropertyToID("_StarsTwinklingSpeed");

        private static readonly int SUN_TEX = Shader.PropertyToID("_SunTex");
        private static readonly int SUN_TINT = Shader.PropertyToID("_SunTint");
        private static readonly int SUN_SIZE = Shader.PropertyToID("_SunSize");
        private static readonly int SUN_HALO = Shader.PropertyToID("_SunHalo");
        private static readonly int SUN_MATRIX = Shader.PropertyToID("sunMatrix");

        private static readonly int MOON_TEX = Shader.PropertyToID("_MoonTex");
        private static readonly int MOON_SIZE = Shader.PropertyToID("_MoonSize");
        private static readonly int MOON_HALO = Shader.PropertyToID("_MoonHalo");
        private static readonly int MOON_TINT = Shader.PropertyToID("_MoonTint");
        private static readonly int MOON_MATRIX = Shader.PropertyToID("moonMatrix");

        private static readonly int CLOUDS_TEX = Shader.PropertyToID("_CloudsTex");
        private static readonly int CLOUDS_TINT = Shader.PropertyToID("_CloudsTint");
        private static readonly int CLOUDS_ROTATION = Shader.PropertyToID("_CloudsRotation");
        private static readonly int CLOUDS_HEIGHT = Shader.PropertyToID("_CloudsHeight");

        private static readonly int EXPOSURE = Shader.PropertyToID("_Exposure");

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        // Sky

        public Color TopColor
        {
            get => _topColor;
            set
            {
                _topColor = value;
                SkyboxMaterial.SetColor(TOP_COLOR, _topColor);
            }
        }

        public Color MiddleColor
        {
            get => _middleColor;
            set
            {
                _middleColor = value;
                SkyboxMaterial.SetColor(MIDDLE_COLOR, _middleColor);
            }
        }

        public Color BottomColor
        {
            get => _bottomColor;
            set
            {
                _bottomColor = value;
                SkyboxMaterial.SetColor(BOTTOM_COLOR, _bottomColor);
            }
        }

        public float TopExponent
        {
            get => _topExponent;
            set
            {
                _topExponent = value;
                SkyboxMaterial.SetFloat(TOP_EXPONENT, _topExponent);
            }
        }

        public float BottomExponent
        {
            get => _bottomExponent;
            set
            {
                _bottomExponent = value;
                SkyboxMaterial.SetFloat(BOTTOM_EXPONENT, _bottomExponent);
            }
        }

        // Stars

        public bool StarsEnabled => _starsEnabled;

        public Color StarsTint
        {
            get => _starsTint;
            set
            {
                _starsTint = value;
                SkyboxMaterial.SetColor(STARS_TINT, _starsTint);
            }
        }

        public Cubemap StarsCubemap
        {
            get => _starsCubemap;
            set
            {
                _starsCubemap = value;
                SkyboxMaterial.SetTexture(STARS_TEX, _starsCubemap);
            }
        }

        public float StarsExtinction
        {
            get => _starsExtinction;
            set
            {
                _starsExtinction = value;
                SkyboxMaterial.SetFloat(STARS_EXTINCTION, _starsExtinction);
            }
        }

        public float StarsTwinklingSpeed
        {
            get => _starsTwinklingSpeed;
            set
            {
                _starsTwinklingSpeed = value;
                SkyboxMaterial.SetFloat(STARS_TWINKLING_SPEED, _starsTwinklingSpeed);
            }
        }

        // Sun

        public bool SunEnabled => _sunEnabled;

        public Light SunLight
        {
            get => _sunLight;
            set => _sunLight = value;
        }

        public Texture2D SunTexture
        {
            get => _sunTexture;
            set
            {
                _sunTexture = value;
                SkyboxMaterial.SetTexture(SUN_TEX, _sunTexture);
            }
        }

        public Color SunTint
        {
            get => _sunTint;
            set
            {
                _sunTint = value;
                SkyboxMaterial.SetColor(SUN_TINT, _sunTint);
            }
        }

        public float SunSize
        {
            get => _sunSize;
            set
            {
                _sunSize = value;
                SkyboxMaterial.SetFloat(SUN_SIZE, _sunSize);
            }
        }
        
        public float SunHalo
        {
            get => _sunHalo;
            set
            {
                _sunHalo = value;
                SkyboxMaterial.SetFloat(SUN_HALO, _sunHalo);
            }
        }

        public bool SunFlare
        {
            get => _sunFlare;
            set
            {
                _sunFlare = value;
                if (_sunFlareComponent) _sunFlareComponent.enabled = value;
            }
        }

        public float SunFlareBrightness
        {
            get => _sunFlareBrightness;
            set => _sunFlareBrightness = value;
        }

        // Moon

        public bool MoonEnabled => _moonEnabled;

        public Texture2D MoonTexture
        {
            get => _moonTexture;
            set
            {
                _moonTexture = value;
                SkyboxMaterial.SetTexture(MOON_TEX, _moonTexture);
            }
        }

        public float MoonSize
        {
            get => _moonSize;
            set
            {
                _moonSize = value;
                SkyboxMaterial.SetFloat(MOON_SIZE, _moonSize);
            }
        }
        
        public float MoonHalo
        {
            get => _moonHalo;
            set
            {
                _moonHalo = value;
                SkyboxMaterial.SetFloat(MOON_HALO, _moonHalo);
            }
        }

        public Color MoonTint
        {
            get => _moonTint;
            set
            {
                _moonTint = value;
                SkyboxMaterial.SetColor(MOON_TINT, _moonTint);
            }
        }

        public Light MoonLight
        {
            get => _moonLight;
            set => _moonLight = value;
        }

        public bool MoonFlare
        {
            get => _moonFlare;
            set
            {
                _moonFlare = value;
                if (_moonFlareComponent) _moonFlareComponent.enabled = value;
            }
        }

        public float MoonFlareBrightness
        {
            get => _moonFlareBrightness;
            set => _moonFlareBrightness = value;
        }

        // Clouds

        public bool CloudsEnabled => _cloudsEnabled;

        public Cubemap CloudsCubemap
        {
            get => _cloudsCubemap;
            set
            {
                _cloudsCubemap = value;
                SkyboxMaterial.SetTexture(CLOUDS_TEX, _cloudsCubemap);
            }
        }

        public Color CloudsTint
        {
            get => _cloudsTint;
            set
            {
                _cloudsTint = value;
                SkyboxMaterial.SetColor(CLOUDS_TINT, _cloudsTint);
            }
        }

        public float CloudsRotation
        {
            get => _cloudsRotation;
            set
            {
                _cloudsRotation = value;
                SkyboxMaterial.SetFloat(CLOUDS_ROTATION, _cloudsRotation);
            }
        }

        public float CloudsHeight
        {
            get => _cloudsHeight;
            set
            {
                _cloudsHeight = value;
                SkyboxMaterial.SetFloat(CLOUDS_HEIGHT, _cloudsHeight);
            }
        }

        // General

        public float Exposure
        {
            get => _exposure;
            set
            {
                _exposure = value;
                SkyboxMaterial.SetFloat(EXPOSURE, _exposure);
            }
        }

        public bool AdjustFogColor
        {
            get => _adjustFogColor;
            set
            {
                _adjustFogColor = value;
                if (_adjustFogColor) RenderSettings.fogColor = MiddleColor;
            }
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected void Awake()
        {
            UpdateSkyboxProperties();
        }

        protected void OnValidate()
        {
            UpdateSkyboxProperties();
        }

        protected void Update()
        {
            if (SkyboxMaterial == null) return;
            
            if (_adjustFogColor) RenderSettings.fogColor = MiddleColor;

            if (_sunEnabled)
            {
                SkyboxMaterial.SetMatrix(SUN_MATRIX, _sunLight.transform.worldToLocalMatrix);
                if (_sunFlare && _sunFlareComponent) _sunFlareComponent.brightness = _sunTint.a * _sunFlareBrightness;
            }

            if (_moonEnabled)
            {
                SkyboxMaterial.SetMatrix(MOON_MATRIX, _moonLight.transform.worldToLocalMatrix);
                if (_moonFlare && _moonFlareComponent) _moonFlareComponent.brightness = _moonTint.a * _moonFlareBrightness;
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void UpdateSkyboxProperties()
        {
            if (SkyboxMaterial == null)
            {
                Debug.LogWarning("SkyboxController: Skybox material is not assigned.");
                return;
            }            

            // Sky

            SkyboxMaterial.SetColor(TOP_COLOR, _topColor);
            SkyboxMaterial.SetColor(MIDDLE_COLOR, _middleColor);
            SkyboxMaterial.SetColor(BOTTOM_COLOR, _bottomColor);
            SkyboxMaterial.SetFloat(TOP_EXPONENT, _topExponent);
            SkyboxMaterial.SetFloat(BOTTOM_EXPONENT, _bottomExponent);

            // Stars

            if (_starsEnabled)
            {
                SkyboxMaterial.DisableKeyword("STARS_OFF");
                SkyboxMaterial.SetTexture(STARS_TEX, _starsCubemap);
                SkyboxMaterial.SetColor(STARS_TINT, _starsTint);
                SkyboxMaterial.SetFloat(STARS_EXTINCTION, _starsExtinction);
                SkyboxMaterial.SetFloat(STARS_TWINKLING_SPEED, _starsTwinklingSpeed);
            }
            else
            {
                SkyboxMaterial.EnableKeyword("STARS_OFF");
            }

            // Sun

            if (_sunEnabled)
            {
                SkyboxMaterial.DisableKeyword("SUN_OFF");
                SkyboxMaterial.SetTexture(SUN_TEX, _sunTexture);
                SkyboxMaterial.SetFloat(SUN_SIZE, _sunSize);
                SkyboxMaterial.SetFloat(SUN_HALO, _sunHalo);
                SkyboxMaterial.SetColor(SUN_TINT, _sunTint);

                if (_sunLight)
                {
                    _sunLight.gameObject.SetActive(true);
                    _sunFlareComponent = _sunLight.GetComponent<LensFlare>();
                }
                else
                {
                    Debug.LogWarning("SkyboxController: Sun light object is not assigned.");
                }                    

                if (_sunFlareComponent) _sunFlareComponent.enabled = _sunFlare;
            }
            else
            {
                SkyboxMaterial.EnableKeyword("SUN_OFF");
                if (_sunLight) _sunLight.gameObject.SetActive(false);
            }

            // Moon

            if (_moonEnabled)
            {
                SkyboxMaterial.DisableKeyword("MOON_OFF");
                SkyboxMaterial.SetTexture(MOON_TEX, _moonTexture);
                SkyboxMaterial.SetFloat(MOON_SIZE, _moonSize);
                SkyboxMaterial.SetFloat(MOON_HALO, _moonHalo);
                SkyboxMaterial.SetColor(MOON_TINT, _moonTint);

                if (_moonLight)
                {
                    _moonLight.gameObject.SetActive(true);
                    _moonFlareComponent = _moonLight.GetComponent<LensFlare>();
                }
                else
                {
                    Debug.LogWarning("SkyboxController: Moon light object is not assigned.");
                }

                if (_moonFlareComponent) _moonFlareComponent.enabled = _moonFlare;
            }
            else
            {
                SkyboxMaterial.EnableKeyword("MOON_OFF");
                if (_moonLight) _moonLight.gameObject.SetActive(false);
            }

            // Clouds

            if (_cloudsEnabled)
            {
                SkyboxMaterial.DisableKeyword("CLOUDS_OFF");
                SkyboxMaterial.SetTexture(CLOUDS_TEX, _cloudsCubemap);
                SkyboxMaterial.SetColor(CLOUDS_TINT, _cloudsTint);
                SkyboxMaterial.SetFloat(CLOUDS_ROTATION, _cloudsRotation);
                SkyboxMaterial.SetFloat(CLOUDS_HEIGHT, _cloudsHeight);
            }
            else
            {
                SkyboxMaterial.EnableKeyword("CLOUDS_OFF");
            }

            // General

            SkyboxMaterial.SetFloat(EXPOSURE, _exposure);

            // Update skybox

            RenderSettings.skybox = SkyboxMaterial;
        }
    }
}