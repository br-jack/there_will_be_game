using UnityEngine;

namespace Hammer
{
    /*
     VisualHammer: 
        Acts like a spring towards target hammer.
        - Springs toward target position/rotation each FixedUpdate
        - Disables collisions when too far from target (so it can snap back)
        - (TODO) Horse acceleration lag
     */
    public class VisualHammer : MonoBehaviour
    {
        public hammerHead head;
        public Vector3 smallHitboxSize;
        public Vector3 smallHitboxCenter;
        public float mediumHitboxThreshold; //currently set to trail threshold, which may be sensible to maintain?
        public Vector3 mediumHitboxSize;
        public Vector3 mediumHitboxCenter;
        public float largeHitboxThreshold; //currently set to ghost effect threshold, which may be sensible to maintain?
        public Vector3 largeHitboxSize;
        public Vector3 largeHitboxCenter;

        [SerializeField] private Transform pivotTransform;
        [SerializeFrield] private Rigidbody _rb;

        //[SerializeField] private Rigidbody horseRigidBody;

        [Tooltip("This should be from a TargetHammer prefab")]
        [SerializeField] private TargetHammer _targetHammer;

        [Header("Spring Settings")]
        //[SerializeField] private float positionSpringStrength = 2000f;
        //[SerializeField] private float positionDamping = 240f;
        //[SerializeField] private float rotationSpringStrength = 2000f;
        //[SerializeField] private float rotationDamping = 240f;

        /*
        [Header("Collision Distance Settings")]
        [Tooltip("Beyond this distance, collisions are disabled so the hammer can snap back freely")]
        [SerializeField] private float collisionDisableDistance = 2.5f;
        [Tooltip("Must get this close to re-enable collisions")]
        [SerializeField] private float collisionReenableDistance = 0.3f;
        */

        //private bool _collisionsEnabled = true;

        private BoxCollider _hitbox;

        void Awake()
        {
            //_rb = GetComponent<Rigidbody>();
            _hitbox = GetComponent<BoxCollider>();
            Debug.Assert(_hitbox != null);
        }

        private void MoveToTargetPosition()
        {
            transform.position = _targetHammer.transform.position;
            //Vector3 toTarget = _targetHammer.transform.position - transform.position;
            //float distance = toTarget.magnitude;

            //Vector3 springForce = toTarget * positionSpringStrength;

            //// horses are much faster than hammers
            //Vector3 dampingForce = -(_rb.linearVelocity - horseRigidBody.linearVelocity) * positionDamping;

            //_rb.AddForce(springForce + dampingForce, ForceMode.Acceleration);

            //if (_collisionsEnabled && distance > collisionDisableDistance)
            //{
            //    _collisionsEnabled = false;
            //    if (_hitbox != null) _hitbox.enabled = false;
            //}
            //else if (!_collisionsEnabled && distance < collisionReenableDistance)
            //{
            //    _collisionsEnabled = true;
            //    if (_hitbox != null) _hitbox.enabled = true;
            //}
        }

        private void MoveToTargetRotation()
        {
            transform.rotation = _targetHammer.transform.rotation;
            //Quaternion rotationDiff = _targetHammer.transform.rotation * Quaternion.Inverse(transform.rotation);
            //rotationDiff.ToAngleAxis(out float angle, out Vector3 rotationAxis);

            //if (angle > 180f) angle -= 360f;

            //if (rotationAxis.sqrMagnitude > 0.001f)
            //{
            //    Vector3 springTorque = rotationAxis.normalized * (angle * Mathf.Deg2Rad * rotationSpringStrength);
            //    Vector3 dampingTorque = -_rb.angularVelocity * rotationDamping;
            //    _rb.AddTorque(springTorque + dampingTorque, ForceMode.Acceleration);
            //}
        }

        void FixedUpdate()
        {
            /*
            Thoughts and notes:
            - There should be a separate mode which makes the hammer more powerful and shiny and stuff.
                - Maybe this is with awe maybe not
                - This should have to be active to destroy buildings
                - It should do an explosion in an area
                - It should fling you upwards
            - If the hammer has significant velocity (maybe rotational too) the horse should be effected
                (like a dash)
            - The target hammer should be update. maybe both are smaller which would let us use the 
                bungeeing more and would maybe be more fun (though maybe we can scrap it considering how massive
                the hammers gonne be anyways)
                - We should deffo use a small spring system instead of teleporting. This would reduce the jitter
                    and allow us to add animations
                - Yeah yeah the way I did it didn't work we can do it though cause now were not bothing with full]
                    collisions
            - The hitbox size should be continuious with whatever velocity we use
                - I dont even know if were going to use the linear acceleration data from the IMU

            Order of operations (TODO):
                1. Decide on the final size of the hammer/ update target hammer movement
                2. Get some actual velocity value that we use (probably a combination of rotation and acceleration)
                3. Use that velocity for the hammer rigid body (even if we teleport it otherwise)
                4. Use velocity for collider size
                5. Use spring system to make visual hammer movement a little nicer
                6. Make a powered mode
                7. Disable building destruction outside powered mode
                8. Slams....
                -. Implement velocity dash

            Actually had a thought about the visual hammer bungeeing: we should just to all the bungee calculations (specifically)
            the force stuff) ourselves then teleport it still. Trust me.

            */

            // TODO this should probably be continuous
            if (head.forwardSpeed < mediumHitboxThreshold)
            {
                _hitbox.size = smallHitboxSize;
                _hitbox.center = smallHitboxCenter;
            }
            else if (head.forwardSpeed < largeHitboxThreshold)
            {
                _hitbox.size = mediumHitboxSize;
                _hitbox.center = mediumHitboxCenter;
            }
            else
            {
                _hitbox.size = largeHitboxSize;
                _hitbox.center = largeHitboxCenter;
            }

            // maybe horse acceleration?
            //_rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, horseRigidBody.linearVelocity, 0.8f);

            _rb.linearVelocity = _targetHammer.Velocity;
            MoveToTargetPosition();
            MoveToTargetRotation();
        }

    }
}