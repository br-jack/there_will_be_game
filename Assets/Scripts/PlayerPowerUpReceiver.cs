using System.Collections;
using UnityEngine;

public class PlayerPowerUpReceiver : MonoBehaviour
{
    public HorseMovement horseMovement;

    public PlayerHealth playerHealth;

    private float defaultMaxSpeed;
    private float defaultAcceleration;
    private float defaultJumpForce;

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

    private void ApplySpeedBoost(float multiplier, float duration)
    {
        if (speedBoostCoroutine != null && horseMovement != null)
        {
            StopCoroutine(speedBoostCoroutine);

            horseMovement.maxSpeed = defaultMaxSpeed;
            horseMovement.acceleration = defaultAcceleration;
        }

        speedBoostCoroutine = StartCoroutine(SpeedBoostRoutine(multiplier, duration));
    }

    private IEnumerator SpeedBoostRoutine(float multiplier, float duration)
    {
        horseMovement.maxSpeed = defaultMaxSpeed * multiplier;
        horseMovement.acceleration = defaultAcceleration * multiplier;

        yield return new WaitForSeconds(duration);

        horseMovement.maxSpeed = defaultMaxSpeed;
        horseMovement.acceleration = defaultAcceleration;
        speedBoostCoroutine = null;
    }

    private void ApplyJumpBoost(float multiplier, float duration)
    {
        if (jumpBoostCoroutine != null && horseMovement != null)
        {
            StopCoroutine(jumpBoostCoroutine);
            horseMovement.jumpForce = defaultJumpForce;
        }

        jumpBoostCoroutine = StartCoroutine(JumpBoostRoutine(multiplier, duration));
    }

    private IEnumerator JumpBoostRoutine(float multiplier, float duration)
    {
        horseMovement.jumpForce = defaultJumpForce * multiplier;

        yield return new WaitForSeconds(duration);

        horseMovement.jumpForce = defaultJumpForce;
        jumpBoostCoroutine = null;
    }

    private void ApplyHeal(float amount)
    {
        if (playerHealth != null){
            playerHealth.Heal(Mathf.RoundToInt(amount));
        }
    }
}