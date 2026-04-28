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
        // gameOverLow.SetActive(false);
        // gameOverMid.SetActive(false);
        // gameOverHigh.SetActive(false);
        switch (tier)
        {
            case ReportCard.OneStar:
                SceneController.LoadEnd1();
                break;
            case ReportCard.TwoStars:
                SceneController.LoadEnd2();
                break;
            case ReportCard.ThreeStars:
                SceneController.LoadEnd3();
                break;
        }

    }
}
