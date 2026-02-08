using UnityEngine;

public class ShieldHit : MonoBehaviour
{
    public float knockbackForce = 40f;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        Rigidbody enemyRb = GetComponentInParent<Rigidbody>();
        if (enemyRb == null) return;

        Vector3 direction =
            enemyRb.position - other.transform.position;

        direction.y = 0.5f;
        direction.Normalize();

        enemyRb.AddForce(direction * knockbackForce, ForceMode.Impulse);
    }
}
