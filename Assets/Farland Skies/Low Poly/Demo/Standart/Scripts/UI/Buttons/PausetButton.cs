using UnityEngine;

namespace Borodar.FarlandSkies.LowPoly
{
    public class PausetButton : MonoBehaviour
    {
        public void OnClick()
        {
            var cycleManager = SkyboxCycleManager.Instance;
            cycleManager.Paused = !cycleManager.Paused;
        }
    }
}