using UnityEngine;

public class ShieldHit : MonoBehaviour
{
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
        
        // Calculate knockback based on attack velocity
        float knockbackForce = attack.GetKnockbackForce();
        
        //Stagger enemy
        Vector3 knockbackDirection = enemy.transform.position - other.transform.position;
        
        // Faster hits launch enemies higher so they travel farther
        float upwardForceRatio = Mathf.Clamp(knockbackForce / 75f, 0.2f, 1.5f);
        knockbackDirection.y = upwardForceRatio;
        knockbackDirection.Normalize();

        enemy.ApplyKnockback(knockbackDirection * knockbackForce);
        enemy.BreakShield();
    }
}