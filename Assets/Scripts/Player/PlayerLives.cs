using System;
using UnityEngine;

/*
PlayerLives contains the continuous bar (UI).
It's connected to both UIs and PlayerHealth.
Invincibility is controlled here, PlayerHealth can use the getter IsInvincible to see whether to deal damage.
*/

public class PlayerLives : MonoBehaviour
{
    [SerializeField] private int maxLives = 1;

    [Header("UI")]
    [SerializeField] private ContinuousBar healthBar;

    [Header("Dependencies")]
    [SerializeField] private PlayerHealth playerHealth;

    [SerializeField] private float invincibilityTime = 3.0f;
    private float invincibilityTimer = 0.0f;
    private bool _isInvincible;
    public bool IsInvincible => _isInvincible;

    private int lives;

    public int Lives
    {
        get { return lives; }
        private set { lives = value; }
    }
    public bool IsGameOver => Lives <= 0;

    public event Action<int, int> OnLivesChanged;
    public event Action OnGameOver;

    private void Awake()
    {
        if (playerHealth == null) playerHealth = GetComponent<PlayerHealth>();
        if (healthBar == null) healthBar = FindObjectOfType<ContinuousBar>();
    }

    private void Start()
    {
        Lives = 1;
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += UpdateHealthUI;
            playerHealth.OnDeath += LoseLife;
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthUI;
            playerHealth.OnDeath -= LoseLife;
        }
    }

    public void LoseLife()
    {
        if (IsGameOver) return;

        Lives--;
        OnLivesChanged?.Invoke(Lives, maxLives);

        MakeInvincibleFor(invincibilityTime);

        if (IsGameOver)
        {
            OnGameOver?.Invoke();
            gameObject.SetActive(false);
        }
        else
        {
            playerHealth?.ResetHealthToFull();
        }
    }

    public void GainLife(int amount = 1)
    {
        if (amount < 0) return; // Use LoseLife() to reduce lives
        if (IsGameOver) return;
        Lives = Lives + amount;
        if (Lives > maxLives) Lives = maxLives;
        OnLivesChanged?.Invoke(Lives, maxLives);
    }

    public void MakeInvincibleFor(float time)
    {
        _isInvincible = true;
        invincibilityTimer = time;
    }

    private void Update()
    {
        if (_isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0)
            {
                _isInvincible = false;
            }
        }
    }

    private void UpdateHealthUI(int current, int max)
    {
        healthBar?.DisplayHealth(current, max);
    }
}
