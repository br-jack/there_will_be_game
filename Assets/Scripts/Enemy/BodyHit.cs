using UnityEngine;
using System.Collections.Generic;
using Score;

public class BodyHit : MonoBehaviour
{
    public LayerMask shieldMask;
    public hitSound hitSounds;

    //[SerializeField] private float speedThreshold = 5f;
    [SerializeField] private int baseScore = 10;
    //[SerializeField] private int speedBonusScore = 30;
    [SerializeField] private int lowHealthBonusScore = 20;
    [SerializeField] private int lowHealthThreshold = 30;
    [SerializeField] private int airBonusScore = 25;
    [SerializeField] private int shieldBypassBonusScore = 40;
    [SerializeField] private int atATrotBonusScore = 5;
    [SerializeField] private int atACanterBonusScore = 20;
    [SerializeField] private int atAGallopBonusScore = 50;

    [SerializeField] private int fireBonusScore = 50;

    void OnTriggerEnter(Collider other)
    {
        AttackHitbox attack = other.GetComponent<AttackHitbox>();
        if (attack == null) return;

        StandardEnemyAI enemy = GetComponentInParent<StandardEnemyAI>();
        if (enemy == null) return;
        if (enemy.IsKnockedBack) return;
        if (enemy.ShieldWasJustHit) return;
        if (!enemy.CanBeKilled) return;
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
            new ScoreComponent(baseScore, ScoreType.Base)
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
                    scoreComponents.Add(new ScoreComponent(atATrotBonusScore, ScoreType.atATrot));
                    break;
                case gait.cantering:
                    scoreComponents.Add(new ScoreComponent(atACanterBonusScore, ScoreType.atACanter));
                    break;
                case gait.galloping:
                    scoreComponents.Add(new ScoreComponent(atAGallopBonusScore, ScoreType.atAGallop));
                    break;
            }
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
        if (!characterController.isGrounded)
        {
            scoreComponents.Add(new ScoreComponent(airBonusScore, ScoreType.Air));
        }

        // Shield bypass bonus
        if (hasShield)
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
