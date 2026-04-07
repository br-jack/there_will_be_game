using UnityEngine;
using System.Collections.Generic;

public class PitchManager : MonoBehaviour
{
    public List<FootballPlayer> footballEnemies;
    public GameObject invisibleWallForNormalEnemies;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ActivatePitch(true);
        }
    }

    public void ActivatePitch(bool status)
    {
        foreach (var enemy in footballEnemies)
        {
            enemy.SetPitchActivity(status);
        }
        
        // Disable the wall so normal enemies can enter once the goal is scored/task done
        if (!status) invisibleWallForNormalEnemies.SetActive(false);
    }
}