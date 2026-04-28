using System.Collections;
using UnityEngine;

public class PlayerPowerUpReceiver : MonoBehaviour
{
    //public HorseMovement horseMovement;

    public horseMovementGaits horseMovementGaits;
    
    public PlayerParticles playerParticles;

    public PlayerHealth playerHealth;

    //private float _defaultMaxSpeedMultiplier;
    //private float _defaultAccelerationMultiplier;
    //private float _defaultJumpForce;
    private float _defaultAcceleration;
    private float _defaultGallopSpeed;
    private float _defaultCanterSpeed;
    private float _defaultTrotSpeed;
    private float _defaultJumpHeight;
    
    public HammerFireController hammerFireController;
    
    public ParticleSystem jumpBoostParticles;
    public TrailRenderer jumpBoostTrail;
    private ParticleSystem _defaultJumpParticles;
    private TrailRenderer _defaultJumpTrail;

    public ParticleSystem speedBoostParticles;
    private ParticleSystem _defaultSpeedParticles;
    

    private Coroutine speedBoostCoroutine;
    private Coroutine jumpBoostCoroutine;

    public bool canRamBuildings = false;
    private void Start()
    {
        
        if (horseMovementGaits != null)
        {
            //_defaultMaxSpeedMultiplier = horseMovement.maxSpeedMultiplier;
            //_defaultAccelerationMultiplier = horseMovement.accelerationMultiplier;
            //_defaultJumpForce = horseMovement.jumpForce;
            _defaultAcceleration = horseMovementGaits.acceleration;
            _defaultGallopSpeed = horseMovementGaits.gallopSpeed;
            _defaultCanterSpeed = horseMovementGaits.canterSpeed;
            _defaultTrotSpeed = horseMovementGaits.trotSpeed;
            _defaultJumpHeight = horseMovementGaits.jumpHeight;

        }
        

        if (playerParticles != null)
        {
            //_defaultSpeedParticles = playerParticles.runParticles;
            
            _defaultJumpParticles = playerParticles.jumpParticles;
            _defaultJumpTrail = playerParticles.jumpTrail;
            
            speedBoostParticles.gameObject.SetActive(false);
            jumpBoostParticles.gameObject.SetActive(false);
            jumpBoostTrail.gameObject.SetActive(false);
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
        else if (powerUpType == PowerUpType.InfiniteFire)
        {
            ApplyInfiniteFire();
        }
        else if (powerUpType == PowerUpType.HorseRam)
        {
            canRamBuildings = true;
        }
    }

    private void StartSpeedBoostEffects(float multiplier)
    {
        /*
        horseMovement.maxSpeedMultiplier = multiplier;
        horseMovement.accelerationMultiplier = multiplier;
        */
        horseMovementGaits.acceleration *= multiplier;
        horseMovementGaits.trotSpeed *= multiplier;
        horseMovementGaits.canterSpeed *= multiplier;
        horseMovementGaits.gallopSpeed *= multiplier;
        
        
        
        if (playerParticles != null)
        {
            //playerParticles.runParticles.Stop();
            speedBoostParticles.gameObject.SetActive(true);
            //playerParticles.runParticles = speedBoostParticles;
        }
    }

    private void EndSpeedBoostEffects()
    {
        /*
        horseMovement.maxSpeedMultiplier = _defaultMaxSpeedMultiplier;
        horseMovement.accelerationMultiplier = _defaultAccelerationMultiplier;
        */
        horseMovementGaits.trotSpeed = _defaultTrotSpeed;
        horseMovementGaits.canterSpeed = _defaultCanterSpeed;
        horseMovementGaits.gallopSpeed = _defaultGallopSpeed;
        horseMovementGaits.acceleration = _defaultAcceleration;

        if (playerParticles != null)
        {
            //playerParticles.runParticles = _defaultSpeedParticles;
            speedBoostParticles.gameObject.SetActive(false);
        }

        speedBoostCoroutine = null;
    }

    private void ApplySpeedBoost(float multiplier, float duration)
    {
        

        if (speedBoostCoroutine != null && horseMovementGaits != null)
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
        //horseMovement.jumpForce = _defaultJumpForce * multiplier;
        horseMovementGaits.jumpHeight *= multiplier;

        if (playerParticles != null)
        {
            jumpBoostParticles.gameObject.SetActive(true);
            jumpBoostTrail.gameObject.SetActive(true);
            
            playerParticles.jumpParticles = jumpBoostParticles;
            playerParticles.jumpTrail = jumpBoostTrail;
        }
    }

    private void EndJumpBoostEffects()
    {
        //horseMovement.jumpForce = _defaultJumpForce;
        horseMovementGaits.jumpHeight = _defaultJumpHeight;
        if (playerParticles != null)
        {
            playerParticles.jumpTrail.emitting = false;
            playerParticles.jumpParticles = _defaultJumpParticles;
            playerParticles.jumpTrail = _defaultJumpTrail;
            
            jumpBoostParticles.gameObject.SetActive(false);
            jumpBoostTrail.gameObject.SetActive(false);
        }

        jumpBoostCoroutine = null;
    }

    private void ApplyJumpBoost(float multiplier, float duration)
    {
        if (jumpBoostCoroutine != null && horseMovementGaits != null)
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

    private void ApplyInfiniteFire()
    {
        hammerFireController.UnlockInfiniteFire();
        hammerFireController.IgniteHammer();
    }
}