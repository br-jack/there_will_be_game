using UnityEngine;

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

        AwardScore();
     
        // No shield blocking - kill the enemy
        enemy.KilledBy(other);
    }

    private void AwardScore()
    {   
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;
        
        HorseMovement horseMovement = player.GetComponent<HorseMovement>();
        if (horseMovement == null) return;

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        
        int scoreToAdd = baseScore;
        
        if (horseMovement.CurrentSpeed >= speedThreshold)
        {
            scoreToAdd += speedBonusScore;
        }
        if (playerHealth != null)
        {
            float healthPercent = (float)playerHealth.Current / playerHealth.Max * 100f;
            if (healthPercent <= lowHealthThreshold)
            {
                scoreToAdd += lowHealthBonusScore;
            }
        }
        if (!horseMovement.IsGrounded)
        {
            scoreToAdd += airBonusScore;
        }
        
        ScoreManager.Instance.AddScore(scoreToAdd);
    }
}
