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
        [SerializeField] private float positionK = 25;
        [SerializeField] private float positionDampingCoeff = 0.9f;
        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }
        private void moveToTargetPosition()
        {
            Vector3 toTarget = _targetHammer.Position - _rb.position;
            Vector3 springForce = toTarget * positionK;
            Vector3 dampingForce = -_rb.linearVelocity * positionDampingCoeff;

            _rb.AddForce(springForce + dampingForce, ForceMode.Acceleration);
        }
        private void moveToTargetRotation()
        {
            // temporary, should also be springy eventually TODO
            _rb.rotation = _targetHammer.Rotation;
        }
        void Update()
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
