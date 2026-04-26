using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FireballProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 18f;
    [SerializeField] private float lifetime = 4f;

    [Header("Collision")]
    [SerializeField] private LayerMask hitLayer;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask buildingLayer;

    [Header("Impact Knockback")]
    [SerializeField] private float impactKnockbackForce = 8f;
    [SerializeField] private float impactUpwardForceRatio = 0.25f;

    [Header("Flame Pillar")]
    [SerializeField] private GameObject flamePillarPrefab;
    [SerializeField] private float groundCheckHeight = 5f;
    [SerializeField] private float groundCheckDistance = 20f;
    [SerializeField] private LayerMask groundLayerMask;

    private Vector3 moveDirection;
    private bool initialised;

    public void Initialise(Vector3 direction)
    {
        moveDirection = direction.normalized;
        initialised = true;

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
        int otherLayerBit = 1 << other.gameObject.layer;

        // Ignore anything not in the allowed hit layers
        if ((otherLayerBit & hitLayer.value) == 0)
            return;

        Debug.Log("Fireball hit: " + other.name + " on layer " + LayerMask.LayerToName(other.gameObject.layer));

        // ENEMY HIT
        if ((otherLayerBit & enemyLayer.value) != 0)
        {
            StandardEnemyAI enemyAI = other.GetComponentInParent<StandardEnemyAI>();
            if (enemyAI != null)
            {
                ApplyImpactKnockback(enemyAI);

                EnemyBurnable burnable = enemyAI.GetComponent<EnemyBurnable>();
                if (burnable != null)
                {
                    burnable.ApplyBurn(transform.position);
                }

                SpawnFlamePillarAtGround(enemyAI.transform.position);
                Destroy(gameObject);
                return;
            }
        }

        // BUILDING HIT
        if ((otherLayerBit & buildingLayer.value) != 0)
        {
            Destroy(gameObject);
            return;
        }

        // any other solid thing in hitLayer
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

    private void SpawnFlamePillarAtGround(Vector3 targetPosition)
    {
        if (flamePillarPrefab == null)
            return;

        Vector3 rayStart = targetPosition + Vector3.up * groundCheckHeight;

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayerMask))
        {
            Instantiate(flamePillarPrefab, hit.point, Quaternion.identity);
        }
        else
        {
            Instantiate(flamePillarPrefab, targetPosition, Quaternion.identity);
        }
    }
}