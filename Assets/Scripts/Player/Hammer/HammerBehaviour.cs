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
        public static Action OnHammerSwing;

        [SerializeField] private float extension;
        private float extensionVelocity;
        [SerializeField] private float k = 20f;
        [SerializeField] private float dampingCoef = 3f;
        [SerializeField] private float restLength = 1;
        [SerializeField] private float maxLength = 20;
        [SerializeField] private float sensitivity = 2;
        [SerializeField] private float momentumDecay = 0.92f;

        private float momentum = 0;

        [SerializeField] private Transform pivotTransform;

        [Header("Swing Detection")]
        [SerializeField] private float swingAccelerationThreshold = 2.5f;
        [SerializeField] private float swingCooldown = 0.6f;

        private float lastSwingTime = -999f;

        private Rigidbody _rb;

        private Quaternion attitude;
        private Vector3 frameAcceleration;

        private IController _controllerRef;

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        void Start()
        {
            _controllerRef = HammerManager.Instance.hammerController;
        }

        public void CalibrateHammer()
        {
            _controllerRef.Update();
            attitude = _controllerRef.GetAttitude();
            HammerManager.Instance.CalibrationQuaternion = Quaternion.Inverse(attitude);
        }

       
        void UpdateRotation()
        {
            transform.localRotation = HammerManager.Instance.CalibrationQuaternion * attitude;
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
            _controllerRef.Update();
            attitude = _controllerRef.GetAttitude();
            frameAcceleration = _controllerRef.GetAcceleration();
            
            UpdateRotation();
            UpdatePosition();
            CheckForSwing();
        }

        void CheckForSwing()
        {
            Vector3 worldForward = transform.rotation * Vector3.forward;
            float radialAcceleration = Vector3.Dot(frameAcceleration, worldForward);

            if (Mathf.Abs(radialAcceleration) >= swingAccelerationThreshold && Time.time >= lastSwingTime + swingCooldown)
            {
                lastSwingTime = Time.time;
                OnHammerSwing?.Invoke();
                Debug.Log("Hammer swing detected");
            }
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Enemy"))
            {
                Destroy(collision.gameObject);
            }
        }
    }

}
