using Score;
using System;
using UnityEngine;
using System.Collections;

/*
PlayerHealth:
Manages the player's health, including taking damage, healing, and updating the health bar UI.
- Raises an event when player dies (OnDeath)
- use Heal() or TakeDamage() to heal or take damage.
Connected to PlayerLives (which gives it the healthbar UI).
*/

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private DamageVignetteFlash damageFlash;

    [SerializeField] private PlayerInvulnerabilityFlash invulnerabilityFlash;

    [Header("Respawn")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private float respawnDelay = 1f;
    [SerializeField] private float respawnInvincibilityDuration = 2f;

    [Header("Score Penalty")]
    [SerializeField] private int fearPenaltyOnRespawn = 500;

    [SerializeField] private Renderer[] playerRenderers;

    private int current;
    private bool isRespawning = false;

    public int Current
    {
        get { return current; }
        private set { current = value; }
    }
    public int Max => maxHealth;
    public bool IsDead => Current <= 0;
    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;
    private PlayerLives playerLives;

    private void Start()
    {
        if (playerLives == null) playerLives = GetComponent<PlayerLives>();
        ResetHealthToFull();
    }

    public void TakeDamage(int damage)
    {
        //Debug.Log($"TakeDamage activated.");
        if (playerLives.IsInvincible) return; // Don't let player take damage while invisible.
       
        if (damage <= 0)
        {
            Debug.LogError("Don't put negative values for TakeDamage, use Heal() instead");
            return;
        }
        if (IsDead) return;

        Current -= damage;
        if (Current < 0) Current = 0;

        OnHealthChanged?.Invoke(Current, Max);

        if (damageFlash != null)
        {
            damageFlash.Flash();
        }
        if (!IsDead)
        {
            if (invulnerabilityFlash != null)
            {
                invulnerabilityFlash.PlayFlash();
                playerLives.MakeInvincibleFor(invulnerabilityFlash.FlashDuration);
            }
            else // ?
            {
                playerLives.MakeInvincibleFor(1f);
            }

            return;
        }
        SetPlayerVisible(false);
        //OnDeath?.Invoke();
        StartCoroutine(RespawnAfterDelay());
    }

    public void Heal(int amount)
    {
        // Don't call Heal() for a negative amount, use TakeDamage().
        if (amount <= 0)
        {
            Debug.LogError("Don't put negative values for Heal, use TakeDamage() instead");
            return;
        }
        if (IsDead) return;

        current += amount;

        if (current > Max) current = Max;

        OnHealthChanged?.Invoke(Current, Max);
    }

    // Resets player health to full and updates UI accordingly.
    public void ResetHealthToFull()
    {
        Current = Max;
        OnHealthChanged?.Invoke(Current, Max);
    }

    private IEnumerator RespawnAfterDelay()
    {
        isRespawning = true;
        ScoreManager.Instance.RemoveFear(fearPenaltyOnRespawn);
        yield return new WaitForSeconds(respawnDelay);
        RespawnPlayer();
        isRespawning = false;
    }

    private void RespawnPlayer()
    {
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        transform.position = respawnPoint.position;
        transform.rotation = respawnPoint.rotation;

        if (controller != null)
        {
            controller.enabled = true;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        ResetHealthToFull();
        SetPlayerVisible(true);
        playerLives.MakeInvincibleFor(respawnInvincibilityDuration);

        invulnerabilityFlash.PlayFlash();
    }

    private void SetPlayerVisible(bool visible)
    {
        foreach (Renderer rend in playerRenderers)
        {
            rend.enabled = visible;
        }
    }

}
