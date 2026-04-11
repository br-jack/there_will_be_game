using UnityEngine;
using Hammer;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifetime = 5f;

    private int damage;
    public void Initialize(int damageAmount, Vector3 direction)
    {
        damage = damageAmount;
        if (direction.sqrMagnitude > 0.0001f)
        {
            transform.forward = direction.normalized;
        }
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Projectile travels at a constant linear speed in target direction.
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {

        // This comes before the player health so calls destroy() before harming the player if the projectile hits the hammer.
        if (other.GetComponentInParent<HammerBehaviour>() != null)
        {
            HandleProjectileHitsHammer();
            return;
        }

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null) return;

        playerHealth.TakeDamage(damage);
        Destroy(gameObject);
    }

    // Called when the projectile is blocked by the player's hammer
    private void HandleProjectileHitsHammer()
    {
        Destroy(gameObject);
    }
}
