using Enemy;
using UnityEngine;
using Hammer;

public class FireballShooter : MonoBehaviour
{
    [SerializeField] private HammerFireController hammerFireController;
    [SerializeField] private Transform fireballSpawnPoint;
    [SerializeField] private GameObject fireballPrefab;

    [Header("Targeting")]
    [SerializeField] private float targetSearchRadius = 12f;
    [SerializeField] private LayerMask enemyLayerMask;
    [SerializeField] private float spawnForwardOffset = 0.5f;

    [Header("Cone Targeting")]
    [SerializeField] private Transform aimForwardSource;
    [SerializeField, Range(0f, 180f)] private float targetConeAngle = 90f;

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

    private void HandleHammerSwing()
    {
        if (!hammerFireController.HasEternalFireBoonActive)
        {
            return;
        }

        if (Time.time < lastFireTime + minTimeBetweenFireballs)
        {
            return;
        }

        if (Random.value > fireballChance)
        {
            return;
        }

        IDeathState nearestEnemy = FindNearestEnemyInCone();
        if (nearestEnemy == null)
        {
            return;
        }

        Vector3 targetPoint = nearestEnemy.Transform.position + Vector3.up * 1.0f;
        Vector3 direction = targetPoint - fireballSpawnPoint.position;

        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        direction.Normalize();

        Vector3 spawnPosition = fireballSpawnPoint.position + direction * spawnForwardOffset;

        GameObject spawnedFireball = Instantiate(fireballPrefab, spawnPosition, Quaternion.LookRotation(direction));

        FireballProjectile projectile = spawnedFireball.GetComponent<FireballProjectile>();
        projectile.Initialise(direction);
        lastFireTime = Time.time;
    }

    private IDeathState FindNearestEnemyInCone()
    {
        Collider[] hits = Physics.OverlapSphere(fireballSpawnPoint.position, targetSearchRadius, enemyLayerMask);

        IDeathState nearestEnemy = null;
        float nearestDistance = Mathf.Infinity;

        Vector3 forward = aimForwardSource.forward;
        forward.y = 0f;
        forward.Normalize();

        float halfConeAngle = targetConeAngle * 0.5f;
        float minDot = Mathf.Cos(halfConeAngle * Mathf.Deg2Rad);

        foreach (Collider hit in hits)
        {
            IDeathState enemy = hit.GetComponentInParent<IDeathState>();
            if (enemy == null) continue;
            if (enemy.IsDying) continue;
            if (enemy.KnockbackState.IsKnockedBack) continue;
            if (!enemy.CanBeKilled) continue;

            Vector3 toEnemy = enemy.Transform.position - aimForwardSource.position;
            toEnemy.y = 0f;

            if (toEnemy.sqrMagnitude < 0.0001f)
                continue;

            Vector3 toEnemyDir = toEnemy.normalized;
            float dot = Vector3.Dot(forward, toEnemyDir);

            // Reject enemies outside the cone
            if (dot < minDot)
                continue;

            float dist = (enemy.Transform.position - fireballSpawnPoint.position).sqrMagnitude;
            if (dist < nearestDistance)
            {
                nearestDistance = dist;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }
}