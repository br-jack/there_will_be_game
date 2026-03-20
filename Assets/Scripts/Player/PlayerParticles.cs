using UnityEngine;

public class PlayerParticles : MonoBehaviour
{
    [SerializeField] private TrailRenderer jumpTrail;
    [SerializeField] private ParticleSystem runParticles;
    [SerializeField] private ParticleSystem jumpParticles;
    
    [SerializeField] private HorseMovement horseMovement;
    
    void Awake()
    {
        if (jumpTrail == null)
        {
            jumpTrail.emitting = false;
        }
    }
    
    public void OnEnable()
    {
        horseMovement.jumpStarted += TriggerJumpParticles;
    }

    public void OnDisable()
    {
        horseMovement.jumpStarted -= TriggerJumpParticles;
    }

    private void TriggerJumpParticles()
    {
        jumpParticles.Play();

        if (jumpTrail != null)
        {
            jumpTrail.emitting = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (horseMovement.IsGrounded)
        {
            runParticles.Play();
        }
        else
        {
            runParticles.Stop();
        }
    }
}
