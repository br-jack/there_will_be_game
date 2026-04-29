using UnityEngine;
using UnityEngine.Events;

public class slamHitbox : MonoBehaviour
{
    //the slamHitbox should only be enabled when the hammer is charged, so it needs to be updated
    private BoxCollider _collider;
    public UnityEvent<Vector3> slamHitboxTrigger;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _collider = GetComponent<BoxCollider>();
    }

    public void updateHitbox(hammerChargeState state)
    {
        if (state == hammerChargeState.charged) _collider.enabled = true;
        else _collider.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        slamHitboxTrigger.Invoke(transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
