using UnityEngine;
using System;
using System.Collections.Generic;

namespace Score
{
    public enum ScoreType
    {
        Base,
        Speed,
        LowHealth,
        Air,
        ShieldBypass
    }

    public class ScoreComponent
    {
        public int amount;
        public ScoreType type;

        public ScoreComponent(int amount, ScoreType type)
        {
            this.amount = amount;
            this.type = type;
        }
    }
    // TODO potentially add constant decay to awe
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        private int fearScore = 0;
        public int FearScore => fearScore;
        private int aweScore = 0;
        public int AweScore => aweScore;

        public event Action<int> OnScoreChanged;
        public event Action<List<ScoreComponent>> OnScoreAdded;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void AddScore(List<ScoreComponent> components)
        {
            int bonusAmount = 0;
            int baseAmount = 0;
            foreach (var component in components)
            {
                if (component.type.Equals(ScoreType.Base))
                    baseAmount += component.amount;
                else
                    bonusAmount += component.amount;
            }

            fearScore += baseAmount + bonusAmount;
            aweScore += bonusAmount;

            OnScoreChanged?.Invoke(fearScore);
            OnScoreAdded?.Invoke(components);
        }

        public void ResetScore()
        {
            fearScore = 0;
            OnScoreChanged?.Invoke(fearScore);
        }
    }

}
