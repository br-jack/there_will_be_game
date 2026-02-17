using UnityEngine;

public class ShieldHit : MonoBehaviour
{
    public float knockbackForce = 20f;

    void OnTriggerEnter(Collider other)
    {

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

        enemy.MarkShieldHit();
        Vector3 knockbackDirection = enemy.transform.position - other.transform.position;
        knockbackDirection.y = 0.5f;
        knockbackDirection.Normalize();

        enemy.ApplyKnockback(knockbackDirection * knockbackForce);
        enemy.BreakShield();
    }
}