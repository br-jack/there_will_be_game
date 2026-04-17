using UnityEngine;
using Hammer;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 20.0f;
    public float Speed => speed;
    [SerializeField] private float lifetime = 5f;

    // Change this variable depending on if you want the hammer to deflect or destroy the projectile.
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
        if (direction.sqrMagnitude > 0.0001f)
        {
            rb.linearVelocity = direction.normalized * speed;
        }
    }
    void FixedUpdate()
    {
        rb.linearVelocity += Physics.gravity * gravityScale * Time.fixedDeltaTime;
        if (rb.linearVelocity.sqrMagnitude > 0.0001f)
        {

            transform.forward = rb.linearVelocity.normalized;
        }
    }
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
    }
    void Start()
    {
        Invoke(nameof(DestroyWrapper), lifetime);
    }
    void OnTriggerEnter(Collider other)
    {
        if (owner != null && other.transform.IsChildOf(owner.transform)) return;

        // This comes before the player health so calls destroy() before harming the player if the projectile hits the hammer.
        VisualHammer hammer = other.GetComponentInParent<VisualHammer>();
        if (hammer != null)
        {
            // Don't handle again if already hit hammer.
            if (hasHitHammer) return;
            HandleProjectileHitsHammer(hammer);
            return;
        }

        // Hits player.
        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth != null && hasHitHammer == false){
            playerHealth.TakeDamage(damage);
            DestroyWrapper();
            return;
        }

        // If it hits the enemy.
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
                    enemy.KilledBy(collider, null);
                }
            }
            DestroyWrapper();
            return;
        }
        
        // Hits something that's not the player.
        if (!other.isTrigger)
        {
            DestroyWrapper();
        }
    }

    // Called when the projectile is blocked by the player's hammer
    private void HandleProjectileHitsHammer(VisualHammer hammer)
    {
        hasHitHammer = true;
        // If deflection setting is off, the object is destroyed.
        if (!deflectUponHammerHit)
        {
            DestroyWrapper();
            return;
        }
        // DEFLECT
        Vector3 normal = transform.position - hammer.transform.position;
        if (normal.sqrMagnitude < 0.001f)
        {
            normal = -rb.linearVelocity;
        }
        normal.Normalize();

        rb.linearVelocity = Vector3.Reflect(rb.linearVelocity, normal);

        // Reset the lifetime of the projectile upon deflection.
        CancelInvoke(nameof(DestroyWrapper));
        Invoke(nameof(DestroyWrapper), lifetime);
    }
    private void DestroyWrapper()
    {
        // When we add particle effects upon destruction, we can add it here!
        Destroy(gameObject);
    }
}
