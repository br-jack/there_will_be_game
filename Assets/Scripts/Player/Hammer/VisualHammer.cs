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
        //private Rigidbody _rb;

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
            if (head.forwardSpeed < mediumHitboxThreshold)
            {
                _hitbox.size = smallHitboxSize;
                _hitbox.center = smallHitboxCenter;
            } else if (head.forwardSpeed < largeHitboxThreshold)
            {
                _hitbox.size = mediumHitboxSize;
                _hitbox.center = mediumHitboxCenter;
            } else
            {
                _hitbox.size = largeHitboxSize;
                _hitbox.center = largeHitboxCenter;
            }
            
            // maybe horse acceleration?
            //_rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, horseRigidBody.linearVelocity, 0.8f);

            MoveToTargetPosition();
            MoveToTargetRotation();
        }

    }
}