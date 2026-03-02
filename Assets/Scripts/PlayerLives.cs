using System;
using UnityEngine;

/*
PlayerLives contains both the discrete and continuous bars (UI).
*/

public class PlayerLives : MonoBehaviour
{
    [SerializeField] private int maxLives = 3;
    [SerializeField] private int startingLives = 3;

    [Header("UI")]
    [SerializeField] private DiscreteHealthBar livesBar;
    [SerializeField] private ContinuousBar healthBar;

    [Header("Dependencies")]
    [SerializeField] private PlayerHealth playerHealth;

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
    }

    private void Start()
    {
        Lives = startingLives;
        UpdateLivesUI(Lives, maxLives);
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
        UpdateLivesUI(Lives, maxLives);
        OnLivesChanged?.Invoke(Lives, maxLives);

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
        UpdateLivesUI(Lives, maxLives);
        OnLivesChanged?.Invoke(Lives, maxLives);
    }

    private void UpdateHealthUI(int current, int max) => healthBar?.DisplayHealth(current, max);
    private void UpdateLivesUI(int current, int max)  => livesBar?.DisplayHealth(current, max);
}
