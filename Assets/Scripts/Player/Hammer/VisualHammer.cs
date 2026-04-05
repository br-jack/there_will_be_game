using UnityEngine;
using UnityEngine.UIElements;

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

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }
        private void moveToTargetPosition()
        {

        }
        private void moveToTargetAttitude()
        {
            // temporary, should also be springy eventually TODO
            _rb.rotation = _targetHammer.Attitude;
        }
        void Update()
        {

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
