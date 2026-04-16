using UnityEngine;

public class ShieldHit : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        AttackHitbox attack = other.GetComponent<AttackHitbox>();
        if (attack == null) return;

        StandardEnemyAI enemy = GetComponentInParent<StandardEnemyAI>();
        if (enemy == null) return;
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
