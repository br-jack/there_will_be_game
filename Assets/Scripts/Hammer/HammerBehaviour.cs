using System;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

namespace Hammer
{
    public class HammerBehaviour : MonoBehaviour
    {

        [SerializeField] float extension;
        float extensionVelocity;
        [SerializeField] float k = 20f;
        [SerializeField] float dampingCoef = 3f;
        [SerializeField] float restLength = 1;
        [SerializeField] float maxLength = 20;
        [SerializeField] float sensitivity = 2;
        [SerializeField] float momentumDecay = 0.92f;

        private float momentum = 0;

        [SerializeField] Transform pivotTransform;

        public Rigidbody rigidBody;

        void Start()
        {
            rigidBody = GetComponent<Rigidbody>();
        }

        public void CalibrateHammer()
        {
            GlobalManager.Instance.CalibrationQuaternion = Quaternion.Inverse(gameRotationVector);

        }

        void UpdateRotation()
        {
            transform.localRotation = gameRotationVector * GlobalManager.Instance.CalibrationQuaternion;;
        }

        void UpdatePosition()
        {
            Vector3 worldForward = transform.rotation * Vector3.forward;
            float radialAcceleration = Vector3.Dot(frameAcceleration, worldForward);
            float force = Mathf.Abs(radialAcceleration) < 0.1f ? 0f : radialAcceleration;

            momentum += force * Time.deltaTime;
            momentum *= momentumDecay;


            float spring = -k * (extension - restLength);
            float damping = -dampingCoef * extensionVelocity;
            float acceleration = spring + damping + momentum * sensitivity;

            extensionVelocity += acceleration * Time.deltaTime;
            extension += extensionVelocity * Time.deltaTime;
            extension = Mathf.Clamp(extension, 0, maxLength);

            transform.position = pivotTransform.position + transform.rotation * Vector3.forward * extension;
        }

        void Update()
        {

            ParseStream();
            UpdateRotation();
            UpdatePosition();

            // this completely breaks momentum but whatever
            frameAcceleration = new Vector3(0, 0, 0);
        }

        public void OnCollisionEnter(Collision collision)
        {

            if (collision.gameObject.CompareTag("Enemy"))
            {
                Destroy(collision.gameObject);
            }
        }

        void OnDisable()
        {
            
        }
    }

}
