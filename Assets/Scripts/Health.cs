using UnityEngine;

public class Health : MonoBehaviour
{
    //TODO make setter private but still serialize
    public int lives = 4;
    public int maxLives = 4;
    public HealthBar healthbar;

    public void UpdateHealthDisplay()
    {
        float health = lives/maxLives;
        healthbar.DisplayHealth(health);
    }
    
    public void LoseLife()
    {
        lives--;

        UpdateHealthDisplay();
        
        if (lives == 0)
        {
            if (CompareTag("Enemy"))
            {
                
            }
            gameObject.SetActive(false);
        }
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lives = maxLives;
        healthbar.DisplayMaxHealth();
    }
}
