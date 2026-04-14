using UnityEngine;
using System.Collections.Generic;

public class PitchManager : MonoBehaviour
{
    public List<FootballPlayer> footballEnemies;
    private void OnTriggerEnter(Collider other)
    
    {
        //Debug.Log("Something entered the pitch: " + other.name + " with tag: " + other.tag); //this happens a lot so commented out!
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player detected! Activating defenders...");
            ActivatePitch(true);
        }
    }

    public void ActivatePitch(bool status)
    {
        foreach (var enemy in footballEnemies)
        {
            enemy.SetPitchActivity(status);
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