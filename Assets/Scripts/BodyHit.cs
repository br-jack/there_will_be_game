using UnityEngine;

public class BodyHit : MonoBehaviour
{
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
        if (enemy.isKnockedback)
        {
            return;
        }
        // If shield was already hit this frame, ignore body hit
        if (enemy.shieldWasJustHit)
        {
            return;
        }

        // Use raycasting to check if shield is blocking
        Vector3 attackPosition = other.transform.position;
        Vector3 enemyPosition = enemy.transform.position;
        Vector3 direction = (enemyPosition - attackPosition).normalized;
        float distance = Vector3.Distance(attackPosition, enemyPosition);


        // Get all the hits, then filter out the attacker
        RaycastHit[] hits = Physics.RaycastAll(attackPosition, direction, distance);

        // Find the FIRST non-attacker hit
        GameObject firstHit = null;
        foreach (RaycastHit h in hits)
        {
            // Skip the attacker's own colliders
            if (h.collider.transform.IsChildOf(other.transform.root))
            {
                continue;
            }
            // Check if the first thing hit was the shield
            if (h.collider.CompareTag("Shield"))
            {
                return;
            }
            break;
        }

        // No shield blocking - kill the enemy
        enemy.Die();
    }
}
