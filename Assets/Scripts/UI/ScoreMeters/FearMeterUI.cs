using UnityEngine;

namespace Score
{
    public class FearMeterUI : IMeterUI
    {
        override protected void Start()
        {
            
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnFearChanged += HandleScoreChanged;
                UpdateScoreUI(ScoreManager.Instance.FearScore);
            }
            else
            {
                Debug.LogWarning("FearMeterUI: No ScoreManager instance found in scene.");
                UpdateScoreUI(0);
            }
        }

        override protected void OnDestroy()
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnFearChanged -= HandleScoreChanged;
            }
        }

    }
}

