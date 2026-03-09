using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public float baseKnockbackForce = 20f;
    public float velocityKnockbackMultiplier = 2f;
    
    private Rigidbody _rb;
    private HorseMovement _horseMovement;

    private void Awake()
    {
        _rb = GetComponentInParent<Rigidbody>();
        _horseMovement = GetComponentInParent<HorseMovement>();
    }

    public float GetKnockbackForce()
    {
        float speed = 0f;
        
        // Try to get speed from HorseMovement first (more accurate)
        if (_horseMovement != null)
        {
            speed = _horseMovement.GetCurrentSpeed();
        }
        // If can't then use Rigidbody speed
        else if (_rb != null)
        {
            speed = _rb.linearVelocity.magnitude;
        }
        
        // Now calculates knockback based on the speed of approach
        float knockback = baseKnockbackForce + (speed * velocityKnockbackMultiplier);
        
        return knockback;
    }
}
