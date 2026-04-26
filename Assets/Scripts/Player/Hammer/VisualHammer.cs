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
        // [SerializeField] private CharacterController _horseCC;
        public hammerHead head;
        
        // [SerializeField] private Transform pivotTransform;
        private Rigidbody _rb;
        
        [Tooltip("This should be from a TargetHammer prefab")]
        [SerializeField] private TargetHammer _targetHammer;
        
        /*
         [Header("Spring Settings")] 
        [SerializeField] private bool useSpring = true;
        [SerializeField] private float positionSpringStrength = 2000f;
        [SerializeField] private float positionDamping = 240f;
        [SerializeField] private float rotationSpringStrength = 2000f;
        [SerializeField] private float rotationDamping = 240f;
        */

        /*
        [Header("Collision Distance Settings")]
        [Tooltip("Beyond this distance, collisions are disabled so the hammer can snap back freely")]
        [SerializeField] private float collisionDisableDistance = 2.5f;
        [Tooltip("Must get this close to re-enable collisions")]
        [SerializeField] private float collisionReenableDistance = 0.3f;
        */

        [Header("Dynamic Hitbox")]
        [SerializeField] private bool useDynamicHitbox = true;
        [SerializeField] private Vector3 smallHitboxSize;
        [SerializeField] private Vector3 smallHitboxCenter;
        [SerializeField] private float mediumHitboxThreshold; //currently set to trail threshold, which may be sensible to maintain?
        [SerializeField] private Vector3 mediumHitboxSize;
        [SerializeField] private Vector3 mediumHitboxCenter;
        [SerializeField] private float largeHitboxThreshold; //currently set to ghost effect threshold, which may be sensible to maintain?
        [SerializeField] private Vector3 largeHitboxSize;
        [SerializeField] private Vector3 largeHitboxCenter;

        //private bool _collisionsEnabled = true;
        
        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            Debug.Assert(head.Hitbox != null);
        }

        /*private void MoveToTargetPosition()
        {
            if (!useSpring)
            {
                transform.position = _targetHammer.transform.position;
                return;
            }
            Vector3 toTarget = _targetHammer.transform.position - transform.position;
            float distance = toTarget.magnitude;

            Vector3 springForce = toTarget * positionSpringStrength;

            //// horses are much faster than hammers
            // Vector3 dampingForce = -(_rb.linearVelocity - _horseCC.velocity) * positionDamping;
            Vector3 dampingForce = -_rb.linearVelocity * positionDamping;

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
            
            _rb.AddForce(springForce + dampingForce, ForceMode.Acceleration);
        }

        private void MoveToTargetRotation()
        {
            if (!useSpring)
            {
                transform.rotation = _targetHammer.transform.rotation;
                return;
            }
            Quaternion rotationDiff = _targetHammer.transform.rotation * Quaternion.Inverse(transform.rotation);
            rotationDiff.normalized.ToAngleAxis(out float angle, out Vector3 rotationAxis);

            //map range from [0, 360] to [-180, 180]
            if (angle > 180f) angle -= 360f;

            //prevent infinity vectors
            if (rotationAxis.sqrMagnitude > 0.001f)
            {
                Vector3 springTorque = rotationAxis.normalized * ((angle * Mathf.Deg2Rad) * rotationSpringStrength);
                Vector3 dampingTorque = -_rb.angularVelocity * rotationDamping;
                _rb.AddTorque(springTorque + dampingTorque, ForceMode.Acceleration);
            }
        }*/

        void FixedUpdate()
        {
            
            // Debug.Log($"Tensor position: {_rb.inertiaTensor}, Tensor rotation: {_rb.inertiaTensorRotation}");
            if (useDynamicHitbox)
            {
                if (head.forwardSpeed < mediumHitboxThreshold)
                {
                    head.Hitbox.size = smallHitboxSize;
                    head.Hitbox.center = smallHitboxCenter;
                } else if (head.forwardSpeed < largeHitboxThreshold)
                {
                    head.Hitbox.size = mediumHitboxSize;
                    head.Hitbox.center = mediumHitboxCenter;
                } else
                {
                    head.Hitbox.size = largeHitboxSize;
                    head.Hitbox.center = largeHitboxCenter;
                }
            }
            
            // maybe horse acceleration?
            //_rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, horseRigidBody.linearVelocity, 0.8f);

            // MoveToTargetPosition();
            // MoveToTargetRotation();
        }

        public void OnCollisionEnter(Collision collision)
        {
            _targetHammer.Rumble();
        }
    }
}