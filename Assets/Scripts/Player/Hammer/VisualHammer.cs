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
        [SerializeField] private Transform pivotTransform;
        private Rigidbody _rb;
        private Collider _collider;

        [SerializeField] private Rigidbody horseRigidBody;

        [Tooltip("This should be from a TargetHammer prefab")]
        [SerializeField] private TargetHammer _targetHammer;

        [Header("Spring Settings")]
        [SerializeField] private float positionSpringStrength = 2000f;
        [SerializeField] private float positionDamping = 240f;
        [SerializeField] private float rotationSpringStrength = 2000f;
        [SerializeField] private float rotationDamping = 240f;

        [Header("Collision Distance Settings")]
        [Tooltip("Beyond this distance, collisions are disabled so the hammer can snap back freely")]
        [SerializeField] private float collisionDisableDistance = 2.5f;
        [Tooltip("Must get this close to re-enable collisions")]
        [SerializeField] private float collisionReenableDistance = 0.3f;

        private bool _collisionsEnabled = true;

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
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
            //    if (_collider != null) _collider.enabled = false;
            //}
            //else if (!_collisionsEnabled && distance < collisionReenableDistance)
            //{
            //    _collisionsEnabled = true;
            //    if (_collider != null) _collider.enabled = true;
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
            // maybe horse acceleration?
            //_rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, horseRigidBody.linearVelocity, 0.8f);

            MoveToTargetPosition();
            MoveToTargetRotation();
        }

    }
}