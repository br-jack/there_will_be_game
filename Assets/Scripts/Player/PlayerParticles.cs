using UnityEngine;

public class PlayerParticles : MonoBehaviour
{
    public TrailRenderer jumpTrail;
    public ParticleSystem runParticles;
    public ParticleSystem jumpParticles;
    public ParticleSystem maxSpeedParticles;
    
    [SerializeField] private HorseMovement horseMovement;
    
    void Awake()
    {
        if (jumpTrail != null)
        {
            jumpTrail.emitting = false;
        }
    }
    
    public void OnEnable()
    {
        horseMovement.jumpStarted += TriggerJumpParticles;
        horseMovement.reachedMaxSpeed += TriggerMaxSpeedParticles;
    }

    public void OnDisable()
    {
        horseMovement.jumpStarted -= TriggerJumpParticles; 
        horseMovement.reachedMaxSpeed -= TriggerMaxSpeedParticles;
    }

    private void TriggerMaxSpeedParticles()
    {
        maxSpeedParticles.Play();
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
            if (jumpTrail != null)
            {
                jumpTrail.emitting = false;
            }
        }
        else
        {
            runParticles.Stop();
        }
    }
}
