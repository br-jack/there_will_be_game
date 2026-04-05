using UnityEngine;

namespace Hammer
{
    /*
     VisualHammer: 
        Acts like a spring towards target hammer
     */
    public class VisualHammer : MonoBehaviour
    {
        [SerializeField] private Transform pivotTransform;
        private Rigidbody _rb;
        [SerializeField] private Rigidbody horseRigidBody;
        [Tooltip("This should be from a TargetHammer prefab")]
        [SerializeField] private TargetHammer _targetHammer;
        [SerializeField] private float positionK = 100;
        [SerializeField] private float positionDampingCoeff = 63;
        [SerializeField] private float rotationK = 100;
        [SerializeField] private float rotationDampingCoeff = 63;
        [Tooltip("This is the main one you want to change. Just a multiplier")]
        [SerializeField] private float sensitivity = 20f;
        private Vector3 localOffset;


        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            localOffset = _targetHammer.LocalPosition;
        }

        private void moveToTargetPosition()
        {
            Vector3 toTarget = _targetHammer.LocalPosition - transform.localPosition;
            Vector3 worldToTarget = pivotTransform.TransformDirection(toTarget);

            Vector3 worldTargetVel = pivotTransform.TransformDirection(_targetHammer.Velocity);
            Vector3 velError = _rb.linearVelocity - worldTargetVel;

            Vector3 springForce = positionK * worldToTarget;
            Vector3 dampingForce = -positionDampingCoeff * velError;

            _rb.AddForce(springForce + dampingForce, ForceMode.Force);
        }

        private void moveToTargetRotation()
        {
            Quaternion rotationError = _targetHammer.Rotation * Quaternion.Inverse(transform.rotation);
            rotationError.ToAngleAxis(out float angle, out Vector3 axis);

            if (angle > 180f) angle -= 360f;

            Vector3 torque = axis * (angle * rotationK) - rotationDampingCoeff * _rb.angularVelocity;
            _rb.AddTorque(torque, ForceMode.Force);
        }

        void FixedUpdate()
        {
            _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, horseRigidBody.linearVelocity, 0.8f);
            moveToTargetPosition();
            moveToTargetRotation();
        }

        // this should probably be replaced with damage on the enemies or something (it should also involve force maybe?)
        public void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Enemy"))
            {
                Destroy(collision.gameObject);
            }
        }
    }

}
