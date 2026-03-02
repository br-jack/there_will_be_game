using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;

    private int current;

    public int Current // Use `Current` NOT `current` to change
    {
        get { return current; }
        private set { current = value; }
    }
    public int Max => maxHealth;
    public bool IsDead => Current <= 0;
    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    private void Start()
    {
        ResetHealthToFull();
    }
    public void TakeDamage(int damage)
    {
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

    public void ResetHealthToFull()
    {
        Current = Max;
        OnHealthChanged?.Invoke(Current, Max);
    }

}
