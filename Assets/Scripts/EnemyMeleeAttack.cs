using UnityEngine;

/// Melee attack component (Requires the EnemyMovement on the same GameObject).
public class EnemyMeleeAttack : MonoBehaviour
{
    [Header("Melee Attack Settings")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private string playerTag = "Player";

    // Requires {enemyMovement, playerHealth, playerTransform} to function.
    private EnemyMovement enemyMovement;
    private PlayerHealth playerHealth;
    private Transform playerTransform;
    private float nextAttackTime;
    private bool hasWarnedMissingPlayerRefs;

    private void Start()
    {
        ResolveEnemyMovement();
        ResolvePlayerRefs();
    }

    private void ResolveEnemyMovement()
    {
        enemyMovement = GetComponent<EnemyMovement>();
        if (enemyMovement == null) Debug.LogError("EnemyMeleeAttack.cs: missing `enemyMovement`.");
    }

    private void ResolvePlayerRefs()
    {
        if (enemyMovement != null)
        {
            playerHealth = enemyMovement._playerHealthRef;
            playerTransform = enemyMovement._playerTransformRef;
        }

        // There was some problems with missing playerRefs, hence this code.
        if (playerHealth == null || playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag(playerTag);
            if (player != null)
            {
                if (playerHealth == null) playerHealth = player.GetComponent<PlayerHealth>();
                if (playerTransform == null) playerTransform = player.transform;
            }
            if (playerHealth != null && playerTransform != null && enemyMovement != null)
            {
                enemyMovement._playerHealthRef = playerHealth;
                enemyMovement._playerTransformRef = playerTransform;
            }
        }
    }

    private void Update()
    {
        // There was some problems with missing playerRefs, hence this if loop.
        if (playerHealth == null || playerTransform == null)
        {
            ResolvePlayerRefs();
            WarnIfMissingPlayerRefs();
        }

        if (!CanAttack()) return;
        if (Time.time < nextAttackTime) return;

        // Checks whether player is close enough to attack
        float sqrDistance = (playerTransform.position - transform.position).sqrMagnitude;
        if (sqrDistance > attackRange * attackRange) return;

        PerformAttack();
    }

    private bool CanAttack()
    {
        if (playerHealth == null || playerTransform == null) return false;
        if (playerHealth.IsDead) return false;
        return true;
    }

    private void PerformAttack()
    {
        // Logs attack in console, performs attack, sets cooldown
        Debug.Log($"EnemyMeleeAttack: the enemy deals {damage} damage to {playerHealth?.name} at this time: {Time.time}");
        playerHealth.TakeDamage(damage);
        nextAttackTime = Time.time + attackCooldown;
    }

    private void WarnIfMissingPlayerRefs()
    {
        if (hasWarnedMissingPlayerRefs) return;
        if (Time.time < 2.0f) return;

        hasWarnedMissingPlayerRefs = true;
        Debug.LogWarning(
            $"EnemyMeleeAttack on {gameObject.name} still cannot find Player refs after 2.0s. " + $"Check that the Player exists, is active, has tag '{playerTag}', and has PlayerHealth."
        );
    }
}
