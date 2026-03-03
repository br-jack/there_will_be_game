using System;
using UnityEngine;

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

    private int current;

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
        if (playerLives.IsInvincible) return; // Don't let player take damage while invisible.

        // Don't put negative values for TakeDamage, use Heal() instead
        if (damage <= 0) return;
        if (IsDead) return;

        Current -= damage;
        if (Current < 0) Current = 0;

        OnHealthChanged?.Invoke(Current, Max);

        if (IsDead) OnDeath?.Invoke();
    }

    public void Heal(int amount)
    {
        // Don't call Heal() for a negative amount, use TakeDamage().
        if (amount <= 0) return;
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

}
