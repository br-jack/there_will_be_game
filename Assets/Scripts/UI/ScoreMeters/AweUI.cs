using TMPro;
using UnityEngine;

namespace Score
{
    public class AweUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI aweText;

        private void Start()
        {

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnAweChanged += UpdateAweDisplay;
                UpdateAweDisplay(ScoreManager.Instance.AweScore);
            }
        }

        private void OnDestroy()
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnAweChanged -= UpdateAweDisplay;
            }
        }

        private void UpdateAweDisplay(int awe)
        {
            aweText.text = $"{awe}";
        }

    }
}

