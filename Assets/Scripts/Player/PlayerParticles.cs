using System.Collections;
using UnityEngine;

public class PlayerParticles : MonoBehaviour
{
    public TrailRenderer jumpTrail;
    public ParticleSystem runParticles;
    public ParticleSystem jumpParticles;
    
    [SerializeField] private HorseMovement horseMovement;

    private ParticleSystem.Particle[] _particleBuffer;
    
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
        horseMovement.jumpReleased += LowerJumpParticles;
    }

    public void OnDisable()
    {
        horseMovement.jumpStarted -= TriggerJumpParticles; 
        horseMovement.jumpReleased -= LowerJumpParticles;
    }

    private void DecreaseParticles(ParticleSystem pSystem)
    {
        if (_particleBuffer == null || _particleBuffer.Length < pSystem.main.maxParticles)
        {
            _particleBuffer = new ParticleSystem.Particle[pSystem.main.maxParticles];
        }

        // GetParticles is allocation free because we reuse the m_Particles buffer between updates
        int numAliveParticles = pSystem.GetParticles(_particleBuffer);
        
        // Change only the particles that are alive
        for (int i = 0; i < numAliveParticles; i++)
        {
            _particleBuffer[i].remainingLifetime /= 2f;
        }

        pSystem.SetParticles(_particleBuffer, numAliveParticles);
    }

    private void LowerJumpParticles()
    {
        DecreaseParticles(jumpParticles);
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
            // var sizeOverLifetime = jumpParticles.sizeOverLifetime;
            // sizeOverLifetime.sizeMultiplier = 1;
            
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
