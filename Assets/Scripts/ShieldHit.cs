using UnityEngine;

public class ShieldHit : MonoBehaviour
{
    public float knockbackForce = 40f;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        EnemyMovement enemyRb = GetComponentInParent<EnemyMovement>();
        if (enemyRb == null) return;

        Vector3 direction =
            enemyRb.transform.position - other.transform.position;

        direction.y = 0.5f;
        direction.Normalize();

        enemyRb.ApplyKnockback(direction * knockbackForce);
    }
}
