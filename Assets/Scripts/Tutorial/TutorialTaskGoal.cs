using UnityEngine;

public class TutorialTaskGoal : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private TutorialReachPointTask tutorialTask;

    private void OnTriggerEnter(Collider other)
    {
        if (tutorialTask != null && other.CompareTag(playerTag))
        {
            tutorialTask.MarkGoalReached();
        }
    }
}
