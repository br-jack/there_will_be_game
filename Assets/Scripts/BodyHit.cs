using UnityEngine;
using System.Collections.Generic;

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

    void OnTriggerEnter(Collider other)
    {
        // Check if hit by attack
        AttackHitbox attack = other.GetComponent<AttackHitbox>();
        if (attack == null) return;

        // Try StandardEnemyAI first, then fall back to EnemyMovement
        StandardEnemyAI standardEnemy = GetComponentInParent<StandardEnemyAI>();
        if (standardEnemy != null)
        {
            HandleStandardEnemy(standardEnemy, other, attack);
            return;
        }

        EnemyMovement enemy = GetComponentInParent<EnemyMovement>();
        if (enemy != null)
        {
            HandleFormationEnemy(enemy, other, attack);
        }
    }

    private void HandleStandardEnemy(StandardEnemyAI enemy, Collider other, AttackHitbox attack)
    {
        if (enemy.IsKnockedBack) return;
        if (enemy.ShieldWasJustHit) return;

        Vector3 attackPosition = other.transform.position;
        Vector3 enemyPosition = enemy.transform.position;
        Vector3 direction = (enemyPosition - attackPosition).normalized;
        float distance = Vector3.Distance(attackPosition, enemyPosition);

        if (Physics.Raycast(attackPosition, direction, distance, shieldMask)) return;

        hitSounds = GameObject.Find("KillSound").GetComponent<hitSound>();
        hitSounds.PlaySFX();

        AwardScore(enemy.HasShield());
        enemy.KilledBy(other, attack);
    }

    private void HandleFormationEnemy(EnemyMovement enemy, Collider other, AttackHitbox attack)
    {
        if (enemy.IsKnockedBack) return;
        if (enemy.ShieldWasJustHit) return;

        Vector3 attackPosition = other.transform.position;
        Vector3 enemyPosition = enemy.transform.position;
        Vector3 direction = (enemyPosition - attackPosition).normalized;
        float distance = Vector3.Distance(attackPosition, enemyPosition);

        if (Physics.Raycast(attackPosition, direction, distance, shieldMask)) return;

        hitSounds = GameObject.Find("KillSound").GetComponent<hitSound>();
        hitSounds.PlaySFX();

        AwardScore(enemy.HasShield());
        enemy.KilledBy(other, attack);
    }

    private void AwardScore(bool hasShield)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        HorseMovement horseMovement = player.GetComponent<HorseMovement>();
        if (horseMovement == null) return;

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

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
        if (hasShield)
        {
            scoreComponents.Add(new ScoreComponent(shieldBypassBonusScore, ScoreType.ShieldBypass));
        }

        ScoreManager.Instance.AddScore(scoreComponents);
    }
}
