using UnityEngine;

public class PlayerParticles : MonoBehaviour
{
    public TrailRenderer jumpTrail;
    public ParticleSystem runParticles;
    public ParticleSystem jumpParticles;
    
    [SerializeField] private HorseMovement horseMovement;

    [SerializeField] private ParticleSystem.MinMaxCurve lowJumpCurve;
    
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
