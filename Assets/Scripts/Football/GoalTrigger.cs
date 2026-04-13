using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    public ParticleSystem confetti;
    public ParticleSystem fireworks;
    public AudioClip fireworksSFX;
    [Range(0, 1)] public float volume = 1.0f;

    void Awake()
    {
       fireworksSFX = Resources.Load<AudioClip>("fireworksSFX"); 
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Football")) return;
        Football ball = other.GetComponent<Football>();
        
        if (ball != null)
        {
            if (confetti != null)
            {
                confetti.Play();
            }

            if (fireworks != null && fireworksSFX != null)
            {
                AudioSource.PlayClipAtPoint(fireworksSFX, transform.position, volume);
                fireworks.Play();
            }

            FootballTask task = FindFirstObjectByType<FootballTask>();
            if (task != null) task.goalScored();

            ball.StartCoroutine(ball.ResetBall(1.5f));

            PitchManager pitchManager = FindFirstObjectByType<PitchManager>();
            pitchManager.EndFootballMiniGame();

            Debug.Log("GOAL SCORED!");
        }
    }
}