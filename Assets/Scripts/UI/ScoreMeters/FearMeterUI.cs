using UnityEngine;

namespace Score
{
    public class FearMeterUI : IMeterUI
    {
        override protected void Start()
        {
            max = ScoreManager.Instance.MaxFearScore;
            ScoreManager.Instance.OnFearChanged += HandleScoreChanged;
            UpdateScoreUI(ScoreManager.Instance.FearScore);
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

