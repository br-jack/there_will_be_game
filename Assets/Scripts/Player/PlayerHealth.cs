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
    [SerializeField] private PlayerDamageRedFlash damageRedFlash;

    [SerializeField] private PlayerInvulnerabilityFlash invulnerabilityFlash;

    [Header("Respawn")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private float respawnDelay = 1f;
    [SerializeField] private float respawnInvincibilityDuration = 2f;

    [Header("Score Penalty")]
    [SerializeField] private int fearPenaltyOnRespawn = 500;

    [Header("Damage Reduction After Hit")]
    [SerializeField] private float damageReductionDuration = 1f;
    [SerializeField, Range(0f, 1f)] private float reducedDamageMultiplier = 0.25f;
    private float damageReductionEndTime;

    [SerializeField] private Renderer[] playerRenderers;
    [SerializeField] private DeathTextUI deathTextUI;
    private PlayerParticles playerParticles;

    private horseMovementGaits horseMovement;
    private Hammer.TargetHammer targetHammer;

    [Header("Hammer VFX")]
    [SerializeField] private GameObject hammerParticlesRoot;
    [SerializeField] private GameObject hammerFireVisual;
    [SerializeField] private HammerFireController hammerFireController;

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
    private AudioSource audioSource;
    public AudioClip fleshClip;
    public AudioClip fleshClip1;
    private AudioClip[] fleshSounds;


    private void Awake()
    {
        if (damageRedFlash == null)
        {
            damageRedFlash = GetComponent<PlayerDamageRedFlash>();
            if (damageRedFlash == null)
            {
                damageRedFlash = gameObject.AddComponent<PlayerDamageRedFlash>();
            }
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        audioSource.volume = 1f;
        audioSource.playOnAwake = false;

        fleshSounds = new AudioClip[]
        {
            fleshClip,
            fleshClip1
        };
    }

    private void Start()
    {
        if (horseMovement == null) horseMovement = GetComponent<horseMovementGaits>();
        if (targetHammer == null) targetHammer = FindFirstObjectByType<Hammer.TargetHammer>();
        if (playerLives == null) playerLives = GetComponent<PlayerLives>();
        if (playerParticles == null) playerParticles = GetComponent<PlayerParticles>();
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

        if (Time.time < damageReductionEndTime)
        {
            damage = Mathf.RoundToInt(damage * reducedDamageMultiplier);
        }

        Current -= damage;
        if (Current < 0) Current = 0;

        OnHealthChanged?.Invoke(Current, Max);

        if (damageFlash != null)
        {
            damageFlash.Flash();
        }

        AudioClip clip = fleshSounds[UnityEngine.Random.Range(0, fleshSounds.Length)];
        if (clip != null)
        {
            audioSource.PlayOneShot(clip, 1f);
        }

        if (!IsDead)
        {
            damageReductionEndTime = Time.time + damageReductionDuration;
            if (damageRedFlash != null)
            {
                damageRedFlash.PlayFlash();
            }
            else if (invulnerabilityFlash != null)
            {
                invulnerabilityFlash.PlayFlash();
            }
            else
            {
                playerLives.MakeInvincibleFor(1f);
            }

            return;
        }
        SetPlayerVisible(false);
        SetControlEnabled(false);
        HideHammerEffects();
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
        playerParticles.SuppressParticles = true; // Suppress particles during respawn to avoid weird effects.
        playerParticles.StopAllMovementParticles();
        ScoreManager.Instance.RemoveFear(fearPenaltyOnRespawn);
        ScoreManager.Instance.ResetAwe();
        if (deathTextUI != null)
        {
            deathTextUI.ShowDeathText();
        }
        yield return new WaitForSeconds(respawnDelay);
        RespawnPlayer();
        yield return null;
        playerParticles.StopAllMovementParticles();
        playerParticles.SuppressParticles = false;
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
        SetControlEnabled(true);
        RestoreHammerEffectsAfterRespawn();
        playerLives.MakeInvincibleFor(respawnInvincibilityDuration);

        if (invulnerabilityFlash != null)
        {
            invulnerabilityFlash.PlayFlash();
        }
    }

    private void SetPlayerVisible(bool visible)
    {
        foreach (Renderer rend in playerRenderers)
        {
            rend.enabled = visible;
        }
    }

    private void SetControlEnabled(bool enabled)
    {
        horseMovement.canControl = enabled;
        targetHammer.canControl = enabled;
    }

    private void HideHammerEffects()
    {
        hammerParticlesRoot.SetActive(false);
        hammerFireVisual.SetActive(false);
    }

    private void RestoreHammerEffectsAfterRespawn()
    {
        hammerParticlesRoot.SetActive(true);

        bool shouldShowFire = hammerFireController != null && hammerFireController.InfiniteFireUnlocked;
        hammerFireVisual.SetActive(shouldShowFire);
    }

}
