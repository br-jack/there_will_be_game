using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FireballProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 18f;
    [SerializeField] private float lifetime = 4f;

    [Header("Collision")]
    [SerializeField] private LayerMask hitLayer;

    [Header("Impact Knockback")]
    [SerializeField] private float impactKnockbackForce = 8f;
    [SerializeField] private float impactUpwardForceRatio = 0.25f;

    private Vector3 moveDirection;
    private bool initialised;

    public void Initialise(Vector3 direction)
    {
        moveDirection = direction.normalized;
        initialised = true;

        Debug.Log("FireballProjectile initialised with direction: " + moveDirection);

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!initialised)
            return;

        transform.position += moveDirection * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & hitLayer) == 0)
            return;

        Debug.Log("FireballProjectile hit: " + other.name);

        StandardEnemyAI enemyAI = other.GetComponentInParent<StandardEnemyAI>();
        if (enemyAI != null)
        {
            ApplyImpactKnockback(enemyAI);

            EnemyBurnable burnable = enemyAI.GetComponent<EnemyBurnable>();
            if (burnable != null)
            {
                burnable.ApplyBurn(transform.position);
            }

            Destroy(gameObject);
            return;
        }

        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }

    private void ApplyImpactKnockback(StandardEnemyAI enemyAI)
    {
        if (enemyAI == null) return;
        if (enemyAI.IsDying) return;

        Vector3 knockbackDirection = enemyAI.transform.position - transform.position;
        knockbackDirection.y = impactUpwardForceRatio;
        knockbackDirection.Normalize();

        enemyAI.ApplyKnockback(knockbackDirection * impactKnockbackForce);
    }
}