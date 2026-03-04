using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public float baseKnockbackForce = 20f;
    public float velocityKnockbackMultiplier = 2f;
    
    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponentInParent<Rigidbody>();
    }

    public float GetKnockbackForce()
    {
        if (_rb == null)
        {
            return baseKnockbackForce;
        }

        float speed = _rb.linearVelocity.magnitude;
        
        // Now calculates knockback based on the speed of approach
        float knockback = baseKnockbackForce + (speed * velocityKnockbackMultiplier);
        
        return knockback;
    }
}
