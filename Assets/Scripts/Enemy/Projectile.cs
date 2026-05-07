using UnityEngine;
using Hammer;
using System.Collections.Generic;
using Enemy;

public class Projectile : MonoBehaviour
{
    private const float DirectionEpsilonSqr = 0.0001f;
    private static readonly List<Collider> ActiveProjectileColliders = new List<Collider>();

    [SerializeField] private float speed = 20.0f;
    [SerializeField] private float lifetime = 5f;

    // note for tuning: change this variable depending on if you want the hammer to deflect or destroy the projectile.
    [SerializeField] private bool deflectUponHammerHit = true;
    [SerializeField] private float gravityScale = 0f;
    private bool hasHitHammer = false;
    private Rigidbody rb;
    private int damage;
    private new Collider collider;
    private GameObject owner;
    public void Initialize(int damageAmount, Vector3 direction, GameObject owner)
    {
        this.owner = owner;
        damage = damageAmount;
        if (direction.sqrMagnitude > DirectionEpsilonSqr)
        {
            Vector3 normalizedDirection = direction.normalized;
            AlignToDirection(normalizedDirection);
            rb.linearVelocity = normalizedDirection * speed;
        }
    }
    void FixedUpdate()
    {
        rb.linearVelocity += Physics.gravity * gravityScale * Time.fixedDeltaTime;
        if (rb.linearVelocity.sqrMagnitude > DirectionEpsilonSqr)
        {
            AlignToDirection(rb.linearVelocity);
        }
    }
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
    }
    void OnEnable()
    {
        RegisterProjectileCollisionIgnores();
    }
    void OnDisable()
    {
        UnregisterProjectileCollider();
    }
    void Start()
    {
        Invoke(nameof(DestroyWrapper), lifetime);
    }
    void OnTriggerEnter(Collider other)
    {
        if (owner != null && other.transform.IsChildOf(owner.transform)) return;

        // collisions between projectiles are ignored (caused bugs earlier with projectiles dissapearing and stuff...)
        Projectile otherProjectile = other.GetComponentInParent<Projectile>();
        if (otherProjectile != null && otherProjectile != this)
        {
            Physics.IgnoreCollision(collider, other, true);
            return;
        }

        // note: PlEASE make sure this stays below the hammer check because otherwise it will deal player damage when it hits the hammer
        VisualHammer hammer = other.GetComponentInParent<VisualHammer>();
        if (hammer != null)
        {
            if (hasHitHammer) return;
            HandleProjectileHitsHammer(hammer);
            return;
        }

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth != null && hasHitHammer == false){
            playerHealth.TakeDamage(damage);
            DestroyWrapper();
            return;
        }

        StandardEnemyAI enemy = other.GetComponentInParent<StandardEnemyAI>();
        if (enemy != null)
        {
            if (hasHitHammer)
            {
                if (enemy.HasShield())
                {
                    enemy.BreakShield();
                }
                else
                {
                    enemy.DeathHandler.KilledBy(collider, null);
                }
                DestroyWrapper();
            }
            // undeflected projectiles should pass through enemies (not having this also caused issues with enemies looking like they're randomly dying in crowds)
            return;
        }
        
        // get rid of projectile if it hits anything that's not the player or the hammer (performance optimisation and aesthetic choice)
        if (!other.isTrigger)
        {
            DestroyWrapper();
        }
    }

    private void HandleProjectileHitsHammer(VisualHammer hammer)
    {
        hasHitHammer = true;
        if (!deflectUponHammerHit)
        {
            DestroyWrapper();
            return;
        }
        Vector3 normal = transform.position - hammer.transform.position;
        if (normal.sqrMagnitude < 0.001f)
        {
            normal = -rb.linearVelocity;
        }
        normal.Normalize();

        rb.linearVelocity = Vector3.Reflect(rb.linearVelocity, normal);
        if (rb.linearVelocity.sqrMagnitude > DirectionEpsilonSqr)
        {
            AlignToDirection(rb.linearVelocity);
        }

        // reset the lifetime of the projectile after being deflected
        CancelInvoke(nameof(DestroyWrapper));
        Invoke(nameof(DestroyWrapper), lifetime);
    }
    private void DestroyWrapper()
    {
        // When we add particle effects upon destruction, we can add it here!!!
        // If we don't, it's still easier than the other way of tracking time to automatically delete it after a set period
        Destroy(gameObject);
    }

    // fix for the 1-frame snap that happens immediately after the arrow spawns in that Shay pointed out
    private void AlignToDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude <= DirectionEpsilonSqr) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
        transform.rotation = targetRotation;

        rb.rotation = targetRotation;
    }

    private void RegisterProjectileCollisionIgnores()
    {
        for (int i = ActiveProjectileColliders.Count - 1; i >= 0; i--)
        {
            Collider otherCollider = ActiveProjectileColliders[i];
            if (otherCollider == null)
            {
                ActiveProjectileColliders.RemoveAt(i);
                continue;
            }

            if (otherCollider == collider) continue;
            Physics.IgnoreCollision(collider, otherCollider, true);
        }

        if (!ActiveProjectileColliders.Contains(collider))
        {
            ActiveProjectileColliders.Add(collider);
        }
    }

    private void UnregisterProjectileCollider()
    {
        ActiveProjectileColliders.Remove(collider);
    }
}
