using Score;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverLow;
    [SerializeField] private GameObject gameOverMid;
    [SerializeField] private GameObject gameOverHigh;
    musicManager musicManager;

    public void Show(ReportCard tier)
    {
        gameObject.SetActive(true);

        gameOverLow.SetActive(false);
        gameOverMid.SetActive(false);
        gameOverHigh.SetActive(false);
        musicManager = GameObject.Find("MusicManager").GetComponent<musicManager>();
        musicManager.PauseMusic();
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
