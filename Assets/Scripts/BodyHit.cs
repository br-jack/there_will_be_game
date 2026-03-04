using UnityEngine;

public class BodyHit : MonoBehaviour
{
    public LayerMask shieldMask;
    public hitSound hitSounds;
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
     
        // No shield blocking - kill the enemy
        enemy.KilledBy(other, attack);
    }
}
