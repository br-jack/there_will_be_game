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
    public bool SuppressParticles { get; set; }

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        if (jumpTrail != null)
        {
            jumpTrail.emitting = false;
            jumpTrail.Clear();
        }

        if (runParticles != null)
        {
            var emission = runParticles.emission;
            emission.enabled = false;
            runParticles.Clear();
            runParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        if (jumpParticles != null)
        {
            jumpParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            jumpParticles.Clear();
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
        if (SuppressParticles) yield break;
        jumpParticles.Play();
        
        if (jumpTrail != null)
        {
            jumpTrail.emitting = true;
        }

        //Reduce particle lifetime and size if jump button is released early
        
        yield return new WaitForSeconds(0.1f);

        //just decrease particles always for now, i don't understand how this works because i am foolish!
        DecreaseParticles(jumpParticles);

        /*
        if (!horseMovement.JumpButtonHeld)
        {
            DecreaseParticles(jumpParticles);
        }
        */
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
        if (SuppressParticles) return;
        StartCoroutine(PlayJumpParticles());
    }

    // Update is called once per frame
    void Update()
    {
        if (SuppressParticles)
        {
            StopAllMovementParticles();
            return;
        }
        if (_cc != null) {
            if (_cc.isGrounded)
            {
                // var sizeOverLifetime = jumpParticles.sizeOverLifetime;
                // sizeOverLifetime.sizeMultiplier = 1;

                if (runParticles != null)
                {
                    var emission = runParticles.emission;
                    emission.enabled = true;

                    if (!runParticles.isPlaying)
                    {
                        runParticles.Play();
                    }
                }

                if (jumpTrail != null)
                {
                    jumpTrail.emitting = false;
                }
            }
            else
            {
                if (runParticles != null)
                {
                    var emission = runParticles.emission;
                    emission.enabled = false;
                    runParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }
        }
    }

    public void StopAllMovementParticles()
    {
        if (runParticles != null)
        {
            var emission = runParticles.emission;
            emission.enabled = false;
            runParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            runParticles.Clear();
        }

        if (jumpParticles != null)
        {
            jumpParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            jumpParticles.Clear();
        }

        if (jumpTrail != null)
        {
            jumpTrail.emitting = false;
            jumpTrail.Clear();
        }
    }
}
