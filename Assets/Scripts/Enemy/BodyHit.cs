using UnityEngine;
using System.Collections.Generic;
using Score;

public class BodyHit : MonoBehaviour
{
    public LayerMask shieldMask;
    public hitSound hitSounds;

    private ScoreSettings scoreSettings;

    void Awake()
    {
        scoreSettings = Resources.Load<ScoreSettings>("ScoreSettings");
    }

    void OnTriggerEnter(Collider other)
    {
        AttackHitbox attack = other.GetComponent<AttackHitbox>();
        if (attack == null) return;

        StandardEnemyAI enemy = GetComponentInParent<StandardEnemyAI>();
        if (enemy == null) return;
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
        //could perhaps be done with events, with enemies broadcasting when they're hit,
        //and a script on the player awarding the score.

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        /*
        HorseMovement horseMovement = player.GetComponent<HorseMovement>();
        if (horseMovement == null) return;
        */

        HammerFireController hammerFireController = FindFirstObjectByType<HammerFireController>();

        //hopefully the player has these! should probs do null checks

        CharacterController characterController = player.GetComponent<CharacterController>();

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

        horseMovementGaits horseMovementGaits = player.GetComponent<horseMovementGaits>();

        if (characterController == null) return;

        List<ScoreComponent> scoreComponents = new List<ScoreComponent>
        {
            // Base score
            new ScoreComponent(scoreSettings.baseScore, ScoreType.Base)
        };

        // Speed bonus. Commented out for now as I think gait bonus is more intuitive.
        /*
        if (characterController.velocity.magnitude >= speedThreshold)
        {
            scoreComponents.Add(new ScoreComponent(speedBonusScore, ScoreType.Speed));
        }
        */

        //gait bonus
        if (horseMovementGaits != null)
        {
            switch (horseMovementGaits.getCurrentGait())
            {
                case gait.walking:
                    // no bonus
                    break;
                case gait.trotting:
                    scoreComponents.Add(new ScoreComponent(scoreSettings.atATrotBonusScore, ScoreType.atATrot));
                    break;
                case gait.cantering:
                    scoreComponents.Add(new ScoreComponent(scoreSettings.atACanterBonusScore, ScoreType.atACanter));
                    break;
                case gait.galloping:
                    scoreComponents.Add(new ScoreComponent(scoreSettings.atAGallopBonusScore, ScoreType.atAGallop));
                    break;
            }
        }

        // Low health bonus
        if (playerHealth != null)
        {
            float healthPercent = (float)playerHealth.Current / playerHealth.Max * 100f;
            if (healthPercent <= scoreSettings.lowHealthThreshold)
            {
                scoreComponents.Add(new ScoreComponent(scoreSettings.lowHealthBonusScore, ScoreType.LowHealth));
            }
        }

        // Air bonus
        if (!characterController.isGrounded)
        {
            scoreComponents.Add(new ScoreComponent(scoreSettings.airBonusScore, ScoreType.Air));
        }

        // Shield bypass bonus
        if (hasShield)
        {
            scoreComponents.Add(new ScoreComponent(scoreSettings.shieldBypassBonusScore, ScoreType.ShieldBypass));
        }

        // On fire bonus
        if (hammerFireController != null && hammerFireController.IsOnFire)
        {
            scoreComponents.Add(new ScoreComponent(scoreSettings.fireBonusScore, ScoreType.OnFire));
        }

        ScoreManager.Instance.AddScore(scoreComponents);
    }
}
