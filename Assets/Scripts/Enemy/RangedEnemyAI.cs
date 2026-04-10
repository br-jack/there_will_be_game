using UnityEngine;

// Instead of dealing damage directly, spawns a projectile.
public class RangedEnemyAI : StandardEnemyAI
{
    [SerializeField] private Projectile projectile;

    [Tooltip("Offset where projectile spawns.")]
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 1.5f, 0.8f);

    [Tooltip("Horizontal aim inaccuracy in degrees. Each shot is rotated by a random angle within +/- this value. 0 = perfect aim.")]
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

        // Make them less accurate with a random offset.
        float spread = Random.Range(-3.0f, 3.0f);
        direction = Quaternion.AngleAxis(spread, Vector3.up) * direction;

        Projectile proj = Instantiate(projectile, spawnPos, Quaternion.identity);
        proj.Initialize(attack.damage, direction);
    }
}
