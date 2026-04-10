using UnityEngine;

// Instead of dealing damage directly, spawns a projectile.
public class RangedEnemyAI : StandardEnemyAI
{
    [SerializeField] private Projectile projectile;

    [Tooltip("Offset where projectile spawns.")]
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 1.5f, 0.8f);

    protected override void DoDamage()
    {
        if (IsDying || _playerTransformRef == null) return;
        if (projectile == null)
        {
            Debug.LogWarning("RangedEnemyAI: projectilePrefab is not assigned in the Inspector.", this);
            return;
        }

        // Position the projectile should spawn from.
        Vector3 spawnPos = transform.TransformPoint(spawnOffset);

        // Projectile shots are aimed at the player's position.
        Vector3 direction = _playerTransformRef.position - spawnPos;
        direction.y = 0f;

        Projectile proj = Instantiate(projectile, spawnPos, Quaternion.identity);
        proj.Initialize(attack.damage, direction);
    }
}
