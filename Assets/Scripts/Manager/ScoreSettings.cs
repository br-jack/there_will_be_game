using UnityEngine;

namespace Score
{
    [CreateAssetMenu(fileName = "ScoreSettings", menuName = "Score/Score Settings")]
    public class ScoreSettings : ScriptableObject
    {
        public int baseScore = 10;
        public int lowHealthBonusScore = 20;
        public int lowHealthThreshold = 30;
        public int airBonusScore = 25;
        public int shieldBypassBonusScore = 40;
        public int atATrotBonusScore = 5;
        public int atACanterBonusScore = 20;
        public int atAGallopBonusScore = 50;
        public int fireBonusScore = 50;
    }
}
