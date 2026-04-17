using Score;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverLow;
    [SerializeField] private GameObject gameOverMid;
    [SerializeField] private GameObject gameOverHigh;

    public void Show(ReportCard tier)
    {
        // First, make sure this whole panel is active
        gameObject.SetActive(true);

        // Hide all three sub-panels
        gameOverLow.SetActive(false);
        gameOverMid.SetActive(false);
        gameOverHigh.SetActive(false);

        // Show only the one that matches the player's score
        switch (tier)
        {
            case ReportCard.OneStar:
                gameOverLow.SetActive(true);
                break;
            case ReportCard.TwoStars:
                gameOverMid.SetActive(true);
                break;
            case ReportCard.ThreeStars:
                gameOverHigh.SetActive(true);
                break;
        }
    }
}
