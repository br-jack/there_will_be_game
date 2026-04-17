using System.Collections;
using UnityEngine;

public class PlayerParticles : MonoBehaviour
{
    public TrailRenderer jumpTrail;
    public ParticleSystem runParticles;
    public ParticleSystem jumpParticles;
    
    //[SerializeField] private HorseMovement horseMovement;
    [SerializeField] private horseMovementGaits _horseMovementGaits;
    private CharacterController _cc;
    
    private ParticleSystem.Particle[] _particleBuffer;
    
    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        if (jumpTrail != null)
        {
            jumpTrail.emitting = false;
        }

        if (runParticles != null)
        {
            var emission = runParticles.emission;
            emission.enabled = false;
        }

    }
    
    public void OnEnable()
    {
        _horseMovementGaits.jumpStarted += TriggerJumpParticles;
    }

    public void OnDisable()
    {
        _horseMovementGaits.jumpStarted -= TriggerJumpParticles; 
    }

    private IEnumerator PlayJumpParticles()
    {
        jumpParticles.Play();
        
        if (jumpTrail != null)
        {
            jumpTrail.emitting = true;
        }

        ////NOTE: Currently we don't have an analog jump, so we don't use this
        /*
        //Reduce particle lifetime and size if jump button is released early
        
        //yield return new WaitForSeconds(0.1f);

        if (!horseMovement.JumpButtonHeld)
        {
            DecreaseParticles(jumpParticles);
        }
        */

        yield return null;
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
            _particleBuffer[i].remainingLifetime /= 2;
            _particleBuffer[i].startSize -= 2.5f;
        }

        pSystem.SetParticles(_particleBuffer, numAliveParticles);
    }

    private void TriggerJumpParticles()
    {
        StartCoroutine(PlayJumpParticles());
    }

    // Update is called once per frame
    void Update()
    {
        if (_cc != null) {
            if (_cc.isGrounded)
            {
                // var sizeOverLifetime = jumpParticles.sizeOverLifetime;
                // sizeOverLifetime.sizeMultiplier = 1;

                if (runParticles != null && !runParticles.isEmitting)
                {
                    var emission = runParticles.emission;
                    emission.enabled = true;
                }
                
                if (jumpTrail != null)
                {
                    jumpTrail.emitting = false;
                }
            }
            else
            {
                var emission = runParticles.emission;
                emission.enabled = false;
            }
        }
    }
}
