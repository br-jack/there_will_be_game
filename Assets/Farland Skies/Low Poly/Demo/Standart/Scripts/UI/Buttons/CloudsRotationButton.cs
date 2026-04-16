using UnityEngine;

namespace Borodar.FarlandSkies.LowPoly
{
    public class CloudsRotationButton : MonoBehaviour
    {
        public float RotationPerSecond = 1;
        private bool _rotate;

        protected void Update()
        {
            if (_rotate) SkyboxController.Instance.CloudsRotation = Time.time * RotationPerSecond;
        }

        public void ToggleCloudsRotation()
        {
            _rotate = !_rotate;
        }
    }
}