using UnityEngine;

public class ShieldHit : MonoBehaviour
{
    public float knockbackForce = 20f;
    public AudioSource audioSource;
    void OnTriggerEnter(Collider other)
    {
           audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogError("man there's no audio source");
        }
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