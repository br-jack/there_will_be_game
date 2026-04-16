using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class hammerHead : MonoBehaviour
{
    public float forwardSpeed;
    public Transform getSpeedRelativeTo;
    private Transform _tf;
    private Vector3 posPrevFrame;
    public float slamRadius;
    public float slamKnockbackAmount;

    InputAction temporarySlamActivate;

    private Collider _temporaryColliderDebug; //CHANGE THIS!

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _tf = GetComponent<Transform>();
        posPrevFrame = _tf.position;
        _temporaryColliderDebug = GetComponent<BoxCollider>();
        temporarySlamActivate = InputSystem.actions.FindAction("Slam");
    }

    // Update is called once per frame
    void Update()
    {
        //find scalar speed of hammer head in the forwards direction
        Vector3 positionChange = _tf.position - posPrevFrame;
        Vector3 velocityGlobal = positionChange / Time.deltaTime;
        Vector3 velocityLocal = getSpeedRelativeTo.InverseTransformDirection(velocityGlobal);
        forwardSpeed = velocityLocal.z;

        posPrevFrame = _tf.position;

        if (temporarySlamActivate.WasPerformedThisFrame()) //temporary
        {
            killAllInRadius();
            Debug.Log("boom!");
        }

        
    }

    private void killEnemyOnHit()
    {
        
    }

    private void killAllInRadius()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position,slamRadius);
        foreach(Collider c in colliders)
        {
            if(c.GetComponentInParent<StandardEnemyAI>())
            {
                Vector3 knockbackDirection = c.ClosestPoint(transform.position) - transform.position; //knock away
                c.GetComponentInParent<StandardEnemyAI>().getKilledBasic(knockbackDirection * slamKnockbackAmount);
            }
        } 
    }
}
