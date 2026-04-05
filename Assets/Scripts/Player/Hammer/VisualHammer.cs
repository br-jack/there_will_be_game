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
        [Tooltip("This should be from a TargetHammer prefab")]
        [SerializeField] private TargetHammer _targetHammer;
        [SerializeField] private float positionK = 100;
        [SerializeField] private float positionDampingCoeff = 63;
        [Tooltip("This is the main one you want to change. Just a multiplier")]
        [SerializeField] private float sensitivity = 20f;


        private Vector3 lastTargetPosition;
        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
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
            // temporary, should also be springy eventually TODO
            _rb.rotation = _targetHammer.Rotation;
        }
        void FixedUpdate()
        {
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
