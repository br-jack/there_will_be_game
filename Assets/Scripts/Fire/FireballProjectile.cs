using Enemy;
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

        // ENEMY HIT
        if ((otherLayerBit & enemyLayer.value) != 0)
        {
            IDeathState deathState = other.GetComponent<IDeathState>();
            
            if (deathState != null)
            {
                Debug.Assert(deathState.KnockbackState != null);
                if (deathState.KnockbackState != null && deathState.IsDying)
                {
                    ApplyImpactKnockback(deathState.KnockbackState, other.transform.position);
                }

                EnemyBurnable burnable = other.GetComponent<EnemyBurnable>();
                if (burnable != null)
                {
                    burnable.ApplyBurn(transform.position);
                }

                SpawnFlamePillarAtGround(other.transform.position);
                Destroy(gameObject);
                return;
            }
        }

        // BUILDING HIT
        if ((otherLayerBit & buildingLayer.value) != 0)
        {
            SpawnFlamePillarAtGround(gameObject.transform.position);
            Destroy(gameObject);
            return;
        }

        // any other solid thing in hitLayer
        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }

    private void ApplyImpactKnockback(IKnockbackState enemyKB, Vector3 enemyPosition)
    {
        if (enemyKB == null) return;

        Vector3 knockbackDirection = enemyPosition - transform.position;
        knockbackDirection.y = impactUpwardForceRatio;
        knockbackDirection.Normalize();

        enemyKB.ApplyKnockback(knockbackDirection * impactKnockbackForce);
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