using UnityEngine;

// Instead of dealing damage directly, spawns a projectile.
public class RangedEnemyAI : StandardEnemyAI
{
    [SerializeField] private Projectile projectile;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0.25f, 0.8f);
    protected override void DoDamage()
    {
        if (IsDying || _playerTransformRef == null) return;

        if (projectile == null)
        {
            Debug.LogWarning("RangedEnemyAI: projectilePrefab is not assigned in the Inspector.", this);
            return;
        }

        Vector3 spawnPosition = transform.TransformPoint(spawnOffset);
        Vector3 direction = (_playerTransformRef.position - spawnPosition).normalized;

        // Horizontal spread to simulate archers won't be completely accurate.
        float spread = Random.Range(-3.0f, 3.0f);
        direction = Quaternion.AngleAxis(spread, Vector3.up) * direction;

        Projectile newProjectile = Instantiate(this.projectile, spawnPosition, Quaternion.identity);
        newProjectile.Initialize(attack.damage, direction, gameObject);
    }
}
