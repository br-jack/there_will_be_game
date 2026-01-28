using UnityEngine;

public class Health : MonoBehaviour
{
    //TODO make setter private but still serialize
    public int lives = 4;
    
    public void LoseLife()
    {
        lives--;
        if (lives == 0)
        {
            Destroy(gameObject);
        }
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
}
