using System.Collections;
using UnityEngine;

public class PlayerParticles : MonoBehaviour
{
    public TrailRenderer jumpTrail;
    public ParticleSystem runParticles;
    public ParticleSystem jumpParticles;
    
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
    }

    public void OnDisable()
    {
        horseMovement.jumpStarted -= TriggerJumpParticles; 
    }

    private IEnumerator SetJumpParticles()
    {
        jumpParticles.Play();
        
        if (jumpTrail != null)
        {
            jumpTrail.emitting = true;
        }

        yield return new WaitForSeconds(0.5f);

        if (!horseMovement.JumpButtonHeld)
        {
            jumpParticles.Pause();
            var sizeOverLifetime = jumpParticles.sizeOverLifetime;
            sizeOverLifetime.sizeMultiplier = 0.25f;
            jumpParticles.Play();
        }
    }

    private void TriggerJumpParticles()
    {
        StartCoroutine(SetJumpParticles());
    }

    // Update is called once per frame
    void Update()
    {
        if (horseMovement.IsGrounded)
        {
            var sizeOverLifetime = jumpParticles.sizeOverLifetime;
            sizeOverLifetime.sizeMultiplier = 1;
            
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
