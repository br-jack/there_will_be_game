using UnityEngine;
using System.Collections.Generic;

public class FootballManager : MonoBehaviour
{
    public List<FootballPlayer> players;
    public Transform ball;
    public Transform playerGoal; 

    void Update()
    {
        FootballPlayer closestPlayer = null;
        float minDistance = float.MaxValue;

        foreach (var player in players)
        {
            if (!player.gameObject.activeInHierarchy) continue;
            
            float dist = Vector3.Distance(player.transform.position, ball.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestPlayer = player;
            }
            player.isChasing = false;
        }

        if (closestPlayer != null)
        {
            closestPlayer.isChasing = true;
            closestPlayer.targetGoal = playerGoal;
        }
    }
}