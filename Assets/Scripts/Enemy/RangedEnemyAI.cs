using UnityEngine;

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
        Vector3 playerCenter = _playerTransformRef.position + Vector3.up * 1.2f;
        Vector3 direction = (playerCenter - spawnPosition).normalized;

        // Horizontal spread to simulate archers won't be completely accurate.
        float spread = Random.Range(-3.0f, 3.0f);
        direction = Quaternion.AngleAxis(spread, Vector3.up) * direction;

        Quaternion spawnRotation = direction.sqrMagnitude > 0.0001f
            ? Quaternion.LookRotation(direction)
            : Quaternion.identity;

        Projectile newProjectile = Instantiate(this.projectile, spawnPosition, spawnRotation);
        newProjectile.Initialize(attack.damage, direction, gameObject);
    }
}
