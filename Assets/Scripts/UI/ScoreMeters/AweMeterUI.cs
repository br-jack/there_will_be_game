using UnityEngine;

namespace Score
{
    public class AweMeterUI : IMeterUI
    {
        [SerializeField] protected new int max = 5000;
        [SerializeField] protected new float fullBarWidth = 280.0f;

        [SerializeField] protected new Color lowScoreColor = new Color(1.0f, 0.72f, 0.15f, 1.0f);
        [SerializeField] protected new Color midScoreColor = new Color(1.0f, 0.45f, 0.10f, 1.0f);
        [SerializeField] protected new Color highScoreColor = new Color(0.90f, 0.15f, 0.15f, 1.0f);
        override protected void Start()
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnAweChanged += HandleScoreChanged;
                UpdateScoreUI(ScoreManager.Instance.AweScore);
            }
            else
            {
                Debug.LogWarning("AweMeterUI: No ScoreManager instance found in scene.");
                UpdateScoreUI(0);
            }
        }

        override protected void OnDestroy()
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnAweChanged -= HandleScoreChanged;
            }
        }

    }
}

