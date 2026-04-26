using UnityEngine;

namespace Hammer
{
    [RequireComponent(typeof(BoxCollider))]
    public class hammerHead : MonoBehaviour
    {
        public float forwardSpeed;
        public BoxCollider Hitbox { get; private set; }
        public Transform getSpeedRelativeTo;
        private Transform _tf;
        private Vector3 posPrevFrame;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            _tf = GetComponent<Transform>();
            Hitbox = GetComponent<BoxCollider>();
            posPrevFrame = _tf.position;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            //find scalar speed of hammer head in the forwards direction
            Vector3 positionChange = _tf.position - posPrevFrame;
            Vector3 velocityGlobal = positionChange/Time.deltaTime;
            Vector3 velocityLocal =  getSpeedRelativeTo.InverseTransformDirection(velocityGlobal);
            forwardSpeed = velocityLocal.z;

            posPrevFrame = _tf.position;
        }
    }
}
