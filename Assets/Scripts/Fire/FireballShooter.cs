using UnityEngine;
using Hammer;

public class FireballShooter : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private HammerFireController hammerFireController;
    [SerializeField] private Transform fireballSpawnPoint;
    [SerializeField] private GameObject fireballPrefab;

    [Header("Targeting")]
    [SerializeField] private float targetSearchRadius = 12f;
    [SerializeField] private LayerMask enemyLayerMask;
    [SerializeField] private float spawnForwardOffset = 0.5f;

    [Header("Chance")]
    [Range(0f, 1f)]
    [SerializeField] private float fireballChance = 1f;

    [Header("Optional Limits")]
    [SerializeField] private float minTimeBetweenFireballs = 0.35f;

    private float lastFireTime = -999f;

    private void OnEnable()
    {
        TargetHammer.OnHammerSwing += HandleHammerSwing;
    }

    private void OnDisable()
    {
        TargetHammer.OnHammerSwing -= HandleHammerSwing;
    }

    // spam debugs cause i think this aint working
    private void HandleHammerSwing()
    {
        Debug.Log("Hammer swing event received by FireballShooter");

        if (hammerFireController == null)
        {
            Debug.LogWarning("FireballShooter: hammerFireController is null");
            return;
        }

        if (fireballSpawnPoint == null)
        {
            Debug.LogWarning("FireballShooter: fireballSpawnPoint is null");
            return;
        }

        if (fireballPrefab == null)
        {
            Debug.LogWarning("FireballShooter: fireballPrefab is null");
            return;
        }

        if (!hammerFireController.HasEternalFireBoonActive)
        {
            Debug.Log("FireballShooter: eternal fire boon not active");
            return;
        }

        if (Time.time < lastFireTime + minTimeBetweenFireballs)
        {
            Debug.Log("FireballShooter: blocked by cooldown");
            return;
        }

        if (Random.value > fireballChance)
        {
            Debug.Log("FireballShooter: chance roll failed");
            return;
        }

        StandardEnemyAI nearestEnemy = FindNearestEnemy();
        if (nearestEnemy == null)
        {
            Debug.Log("FireballShooter: no enemy in range");
            return;
        }

        Vector3 targetPoint = nearestEnemy.transform.position + Vector3.up * 1.0f;
        Vector3 direction = targetPoint - fireballSpawnPoint.position;

        if (direction.sqrMagnitude < 0.0001f)
        {
            Debug.LogWarning("FireballShooter: direction too small");
            return;
        }

        direction.Normalize();

        Vector3 spawnPosition = fireballSpawnPoint.position + direction * spawnForwardOffset;

        Debug.Log($"FireballShooter: firing at {nearestEnemy.name} from {spawnPosition} with direction {direction}");

        GameObject spawnedFireball = Instantiate(
            fireballPrefab,
            spawnPosition,
            Quaternion.LookRotation(direction)
        );

        FireballProjectile projectile = spawnedFireball.GetComponent<FireballProjectile>();
        if (projectile == null)
        {
            Debug.LogWarning("FireballShooter: spawned fireball has no FireballProjectile on root");
            return;
        }

        projectile.Initialise(direction);
        lastFireTime = Time.time;
    }

    private StandardEnemyAI FindNearestEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(fireballSpawnPoint.position, targetSearchRadius, enemyLayerMask);

        StandardEnemyAI nearestEnemy = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            StandardEnemyAI enemy = hit.GetComponentInParent<StandardEnemyAI>();
            if (enemy == null) continue;
            if (enemy.IsDying) continue;
            if (enemy.IsKnockedBack) continue;
            if (!enemy.CanBeKilled) continue;

            float dist = (enemy.transform.position - fireballSpawnPoint.position).sqrMagnitude;
            if (dist < nearestDistance)
            {
                nearestDistance = dist;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }

    private void OnDrawGizmosSelected()
    {
        if (fireballSpawnPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(fireballSpawnPoint.position, targetSearchRadius);
    }
}