using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Football")) return;
        Football ball = other.GetComponent<Football>();
        
        if (ball != null)
        {
            FootballTask task = FindFirstObjectByType<FootballTask>();
            if (task != null) task.goalScored();
            ball.StartCoroutine(ball.ResetBall(1.5f));
            PitchManager pitchManager = FindFirstObjectByType<PitchManager>();
            pitchManager.EndFootballMiniGame();
            Debug.Log("GOAL SCORED!");
        }
    }
}