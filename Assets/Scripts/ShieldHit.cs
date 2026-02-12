using UnityEngine;

public class ShieldHit : MonoBehaviour
{
    public float knockbackForce = 20f;

    void OnTriggerEnter(Collider other)
    {
        AttackHitbox attack = other.GetComponent<AttackHitbox>();
        if (attack == null) return;

        EnemyMovement enemy = GetComponentInParent<EnemyMovement>();
        if (enemy == null) return;

        Vector3 direction = enemy.transform.position - other.transform.position;

        direction.y = 0.5f;
        direction.Normalize();

        enemy.ApplyKnockback(direction * knockbackForce);
        enemy.BreakShield();
    }
}
