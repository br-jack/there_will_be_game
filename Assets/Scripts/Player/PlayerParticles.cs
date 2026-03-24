using System.Collections;
using UnityEngine;

public class PlayerParticles : MonoBehaviour
{
    public TrailRenderer jumpTrail;
    public ParticleSystem runParticles;
    public ParticleSystem jumpParticles;
    
    [SerializeField] private HorseMovement horseMovement;

    [SerializeField] private ParticleSystem.MinMaxCurve lowJumpCurve;

    private ParticleSystem.MinMaxCurve _normalCurve;
    
    void Awake()
    {
        if (jumpTrail != null)
        {
            jumpTrail.emitting = false;
        }

        _normalCurve = jumpParticles.sizeOverLifetime.size;
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
            var sizeOverLifetime = jumpParticles.sizeOverLifetime;
            sizeOverLifetime.size = lowJumpCurve;
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
            sizeOverLifetime.size = _normalCurve;
            
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
