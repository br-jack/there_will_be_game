using Score;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverLow;
    [SerializeField] private GameObject gameOverMid;
    [SerializeField] private GameObject gameOverHigh;

    public void Show(ReportCard tier)
    {
        gameObject.SetActive(true);

        gameOverLow.SetActive(false);
        gameOverMid.SetActive(false);
        gameOverHigh.SetActive(false);

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
