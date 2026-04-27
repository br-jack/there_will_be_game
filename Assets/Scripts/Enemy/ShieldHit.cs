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
        if (!enemy.HasShield()) return;

        enemy.BreakShieldFromAttack(other, attack);
    }
}
