using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TextCore.Text;

public class AttackHitbox : MonoBehaviour
{
    public float baseKnockbackForce = 12f;
    public float velocityKnockbackMultiplier = 1f;
    
    // private Rigidbody _rb;
    //private HorseMovement _horseMovement;
    private CharacterController _cc;
    private horseMovementGaits _horseMovementGaits;
    
    public UnityEvent killEvent;
    public UnityEvent breakShieldEvent;
    public UnityEvent destroyBuildingEvent;

    private void Awake()
    {
        // _rb = GetComponent<Rigidbody>();
        //_horseMovement = GetComponentInParent<HorseMovement>();
        _cc = GetComponentInParent<CharacterController>();
        _horseMovementGaits = GetComponentInParent<horseMovementGaits>();

    }

    public void KilledEnemy()
    {
        killEvent.Invoke();
    }

    public void BrokeShield()
    {
        breakShieldEvent.Invoke();
    }

    public void DestroyedBuilding()
    {
        destroyBuildingEvent.Invoke();
    }

    

    public float GetKnockbackForce()
    {
        float speed = 0f;
        
        // Try to get speed from HorseMovement first (more accurate)
        //horseMovement currently doesn't exist, so this won't work!
        /*
        if (_horseMovement != null)
        {
            speed = _horseMovement.GetCurrentSpeed();
        }
        
        // If can't then use Rigidbody speed <-- we will just do this i guess!
        else if (_rb != null)
        {
            speed = _rb.velocity.magnitude;
        }
        //same thing but gaits, but I want to leave the old code in there too!
        */

        if (_horseMovementGaits != null)
        {
            speed = _horseMovementGaits.currentSpeed;
        }
        else if (_cc != null)
        {
            speed = _cc.velocity.magnitude;
        }
        
        // Now calculates knockback based on the speed of approach
        float knockback = baseKnockbackForce + (speed * velocityKnockbackMultiplier);
        Debug.Log($"Hit at speed {speed:F2} -> Knockback force: {knockback:F2}");

        return knockback;
    }
}
