using UnityEngine;

public class GoalTriggerHome : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Football")) return;
        Football ball = other.GetComponent<Football>();
        
        if (ball != null)
        {
            ball.StartCoroutine(ball.ResetBall(1.5f));
            Debug.Log("GOAL CONCEDED!");
        }
    }
}