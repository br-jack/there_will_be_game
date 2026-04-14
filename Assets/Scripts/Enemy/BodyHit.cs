using UnityEngine;
using System.Collections.Generic;
using Score;

public class BodyHit : MonoBehaviour
{
    public LayerMask shieldMask;
    public hitSound hitSounds;

    [SerializeField] private float speedThreshold = 5f;
    [SerializeField] private int baseScore = 10;
    [SerializeField] private int speedBonusScore = 30;
    [SerializeField] private int lowHealthBonusScore = 20;
    [SerializeField] private int lowHealthThreshold = 30;
    [SerializeField] private int airBonusScore = 25;
    [SerializeField] private int shieldBypassBonusScore = 40;
    [SerializeField] private int fireBonusScore = 50;

    void OnTriggerEnter(Collider other)
    {
        // Check if hit by attack
        AttackHitbox attack = other.GetComponent<AttackHitbox>();
        if (attack == null)
        {
            return;
        }
        EnemyMovement enemy = GetComponentInParent<EnemyMovement>();
        if (enemy == null)
        {
            return;
        }
        if (enemy.IsKnockedBack)
        {
            return;
        }
        // If shield was already hit this frame, ignore body hit
        if (enemy.ShieldWasJustHit)
        {
            return;
        }
        // Use raycasting to check if shield is blocking
        Vector3 attackPosition = other.transform.position;
        Vector3 enemyPosition = enemy.transform.position;
        Vector3 direction = (enemyPosition - attackPosition).normalized;
        float distance = Vector3.Distance(attackPosition, enemyPosition);


        if (Physics.Raycast(attackPosition, direction, distance, shieldMask))
        {
            // Shield is blocking
            return;
        }
        hitSounds = GameObject.Find("KillSound").GetComponent<hitSound>();
        hitSounds.PlaySFX();

        AwardScore(enemy);
     
        // No shield blocking - kill the enemy
        enemy.KilledBy(other, attack);
    }

    private void AwardScore(EnemyMovement enemy)
    {   
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;
        
        HorseMovement horseMovement = player.GetComponent<HorseMovement>();
        if (horseMovement == null) return;

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

        HammerFireController hammerFireController = FindObjectOfType<HammerFireController>();

        List<ScoreComponent> scoreComponents = new List<ScoreComponent>();
        
        // Base score
        scoreComponents.Add(new ScoreComponent(baseScore, ScoreType.Base));
        
        // Speed bonus
        if (horseMovement.CurrentSpeed >= speedThreshold)
        {
            scoreComponents.Add(new ScoreComponent(speedBonusScore, ScoreType.Speed));
        }
        
        // Low health bonus
        if (playerHealth != null)
        {
            float healthPercent = (float)playerHealth.Current / playerHealth.Max * 100f;
            if (healthPercent <= lowHealthThreshold)
            {
                scoreComponents.Add(new ScoreComponent(lowHealthBonusScore, ScoreType.LowHealth));
            }
        }
        
        // Air bonus
        if (!horseMovement.IsGrounded)
        {
            scoreComponents.Add(new ScoreComponent(airBonusScore, ScoreType.Air));
        }

        // Shield bypass bonus
        if (enemy != null && enemy.HasShield())
        {
            scoreComponents.Add(new ScoreComponent(shieldBypassBonusScore, ScoreType.ShieldBypass));
        }

        // On fire bonus
        if (hammerFireController != null && hammerFireController.IsOnFire)
        {
            scoreComponents.Add(new ScoreComponent(fireBonusScore, ScoreType.OnFire));
        }
        
        ScoreManager.Instance.AddScore(scoreComponents);
    }
}
