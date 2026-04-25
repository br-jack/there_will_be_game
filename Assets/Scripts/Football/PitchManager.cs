using UnityEngine;
using System.Collections.Generic;

public class PitchManager : MonoBehaviour
{
    public List<FootballPlayer> footballEnemies;
    private void OnTriggerEnter(Collider other)
    
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player detected! Activating defenders...");
        }
    }

    public void Start()
    {
        foreach (var enemy in footballEnemies)
        {
            enemy.SetPitchActivity(true);
        }
    }

    public void EndFootballMiniGame()
    {
        foreach (var enemy in footballEnemies)
        {   
            enemy.tag = "Enemy";
            enemy.SwitchToNormalAI();
        }
    }
}