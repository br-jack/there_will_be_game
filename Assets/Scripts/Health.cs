using UnityEngine;

public class Health : MonoBehaviour
{
    //TODO make setter private but still serialize
    public int lives = 4;
    public int maxLives = 4;
    public DiscreteHealthBar discreteBar;
    public ContinuousBar continuousBar;

    public void UpdateHealthDisplay()
    {
        if (discreteBar != null) discreteBar.DisplayHealth(lives, maxLives);
        /*
        One of these 2 lines should be used to display the health on either the discrete or continuos bar:
        1. if (discreteBar != null) discreteBar.DisplayHealth(lives, maxLives);
        2. if (continuousBar != null) continuousBar.DisplayHealth(lives, maxLives);
        */
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
        UpdateHealthDisplay();
    }
}
