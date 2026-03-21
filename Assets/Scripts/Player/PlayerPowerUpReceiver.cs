using System.Collections;
using UnityEngine;

public class PlayerPowerUpReceiver : MonoBehaviour
{
    public HorseMovement horseMovement;
    
    public PlayerParticles playerParticles;

    public PlayerHealth playerHealth;

    private float defaultMaxSpeed;
    private float defaultAcceleration;
    private float defaultJumpForce;
    
    
    public ParticleSystem jumpBoostParticles;
    public TrailRenderer jumpBoostTrail;
    private ParticleSystem defaultJumpParticles;
    private TrailRenderer defaultJumpTrail;

    private Coroutine speedBoostCoroutine;
    private Coroutine jumpBoostCoroutine;

    private void Start()
    {
        if (horseMovement != null)
        {
            defaultMaxSpeed = horseMovement.maxSpeed;
            defaultAcceleration = horseMovement.acceleration;
            defaultJumpForce = horseMovement.jumpForce;
        }

        if (playerParticles != null)
        {
            defaultJumpParticles = playerParticles.jumpParticles;
            defaultJumpTrail = playerParticles.jumpTrail;
        }
    }

    public void ApplyPowerUp(PowerUpType powerUpType, float amount, float duration)
    {
        if (powerUpType == PowerUpType.SpeedBoost)
        {
            ApplySpeedBoost(amount, duration);
        }
        else if (powerUpType == PowerUpType.JumpBoost)
        {
            ApplyJumpBoost(amount, duration);
        }
        else if (powerUpType == PowerUpType.Heal)
        {
            ApplyHeal(amount);
        }
    }

    private void StartSpeedBoostEffects(float multiplier)
    {
        horseMovement.maxSpeed = defaultMaxSpeed;
        horseMovement.acceleration = defaultAcceleration;
    }

    private void EndSpeedBoostEffects()
    {
        horseMovement.maxSpeed = defaultMaxSpeed;
        horseMovement.acceleration = defaultAcceleration;
        speedBoostCoroutine = null;
    }

    private void ApplySpeedBoost(float multiplier, float duration)
    {
        if (speedBoostCoroutine != null && horseMovement != null)
        {
            StopCoroutine(speedBoostCoroutine);
            EndSpeedBoostEffects();
        }

        speedBoostCoroutine = StartCoroutine(SpeedBoostRoutine(multiplier, duration));
    }

    private IEnumerator SpeedBoostRoutine(float multiplier, float duration)
    {
        StartSpeedBoostEffects(multiplier);

        yield return new WaitForSeconds(duration);

        EndSpeedBoostEffects();
    }

    private void StartJumpBoostEffects(float multiplier)
    {
        horseMovement.jumpForce = defaultJumpForce * multiplier;

        if (playerParticles != null)
        {
            playerParticles.jumpTrail.emitting = false;
            playerParticles.jumpParticles = jumpBoostParticles;
            playerParticles.jumpTrail = jumpBoostTrail;
        }
    }

    private void EndJumpBoostEffects()
    {
        horseMovement.jumpForce = defaultJumpForce;
        if (playerParticles != null)
        {
            playerParticles.jumpTrail.emitting = false;
            playerParticles.jumpParticles = defaultJumpParticles;
            playerParticles.jumpTrail = defaultJumpTrail;
        }

        jumpBoostCoroutine = null;
    }

    private void ApplyJumpBoost(float multiplier, float duration)
    {
        if (jumpBoostCoroutine != null && horseMovement != null)
        {
            StopCoroutine(jumpBoostCoroutine);
            EndJumpBoostEffects();
        }

        jumpBoostCoroutine = StartCoroutine(JumpBoostRoutine(multiplier, duration));
    }

    private IEnumerator JumpBoostRoutine(float multiplier, float duration)
    {
        StartJumpBoostEffects(multiplier);

        yield return new WaitForSeconds(duration);
        
        EndJumpBoostEffects();
    }

    private void ApplyHeal(float amount)
    {
        if (playerHealth != null){
            playerHealth.Heal(Mathf.RoundToInt(amount));
        }
    }
}