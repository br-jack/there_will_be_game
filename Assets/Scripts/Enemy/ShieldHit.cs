using UnityEngine;

public class ShieldHit : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
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

        Debug.Log($"ShieldHit triggered by {other.gameObject.name}");

        enemy.MarkShieldHit();

        float knockbackForce = attack.GetKnockbackForce();
        Vector3 knockbackDirection = enemy.transform.position - other.transform.position;
        float upwardForceRatio = Mathf.Clamp(knockbackForce / 75f, 0.2f, 1.5f);
        knockbackDirection.y = upwardForceRatio;
        knockbackDirection.Normalize();

        enemy.ApplyKnockback(knockbackDirection * knockbackForce);
        enemy.BreakShield();
    }

    private void HandleFormationEnemy(EnemyMovement enemy, Collider other, AttackHitbox attack)
    {
        if (enemy.IsKnockedBack) return;

        Debug.Log($"ShieldHit triggered by {other.gameObject.name}");

        enemy.MarkShieldHit();

        float knockbackForce = attack.GetKnockbackForce();
        Vector3 knockbackDirection = enemy.transform.position - other.transform.position;
        float upwardForceRatio = Mathf.Clamp(knockbackForce / 75f, 0.2f, 1.5f);
        knockbackDirection.y = upwardForceRatio;
        knockbackDirection.Normalize();

        enemy.ApplyKnockback(knockbackDirection * knockbackForce);
        enemy.BreakShield();
    }
}