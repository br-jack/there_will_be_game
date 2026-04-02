using UnityEngine;

namespace Score
{
    public class AweMeterUI : IMeterUI
    {
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

