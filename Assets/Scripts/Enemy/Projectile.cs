using UnityEngine;
using Hammer;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifetime = 5f;

    // Change this variable depending on if you want the hammer to deflect or destroy the projectile.
    [SerializeField] private bool deflectUponHammerHit = true;
    private bool hasHitHammer = false;
    private Rigidbody rb;
    private int damage;
    public void Initialize(int damageAmount, Vector3 direction)
    {
        damage = damageAmount;
        if (direction.sqrMagnitude > 0.0001f)
        {
            rb.linearVelocity = direction.normalized * speed;
        }
    }
    void FixedUpdate()
    {
        if (rb.linearVelocity.sqrMagnitude > 0.0001f)
        {
            transform.forward = rb.linearVelocity.normalized;
        }
    }
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Start()
    {
        Destroy(gameObject, lifetime);
    }
    void OnTriggerEnter(Collider other)
    {
        // Stop checking for hitting hammer or player after it's already been deflected by the hammer.
        if (hasHitHammer) return;

        // This comes before the player health so calls destroy() before harming the player if the projectile hits the hammer.
        HammerBehaviour hammer = other.GetComponentInParent<HammerBehaviour>();
        if (hammer != null)
        {
            HandleProjectileHitsHammer(hammer);
            return;
        }

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null) return;

        playerHealth.TakeDamage(damage);
        Destroy(gameObject);
    }

    // Called when the projectile is blocked by the player's hammer
    private void HandleProjectileHitsHammer(HammerBehaviour hammer)
    {
        hasHitHammer = true;
        // If deflection setting is off, the object is destroyed.
        if (!deflectUponHammerHit)
        {
            Destroy(gameObject);
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
    }
}
