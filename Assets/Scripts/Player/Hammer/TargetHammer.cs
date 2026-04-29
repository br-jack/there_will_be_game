using UnityEngine;
using System;

namespace Hammer
{
    /*
    TargetHammer:
        Uses IMU position and velocity to simulate hammer. 
            - rotation is 1:1 with IMU
            - position is calculated by simulating a spring using the velocity values
            - feel free to adjust serialised fields however the changes may be obscured by
                the visual hammer behaviour (which uses its own spring). Be sure to disable 
                that spring if you want clearer feedback.
    */
    public class TargetHammer : MonoBehaviour
    {
        public static Action OnHammerSwing;

        [Header("Spring Settings")]
        [Tooltip("This is the main one you want to change. Just a multiplier")]
        [SerializeField] private float sensitivity = 2;
        [Tooltip("Should probably be between like 0.85-0.95, again this is one of the main ones you want to change.")]
        [SerializeField] private float momentumDecay = 0.92f;
        [Tooltip("Do not adjust this this is just serialised so you can see if its actually doing anything.")]
        [SerializeField] private float extension;
        [Tooltip("Spring constant: higher = stiffer")]
        [SerializeField] private float k = 20f;
        [Tooltip("Damping: how quickly it returns to rest, higher = returns to rest faster (have to think abt critical damping though)")]
        [SerializeField] private float dampingCoef = 3f;
        [Tooltip("Shortest length of spring: should be at least one or it causes maths issues")]
        [SerializeField] private float restLength = 1;
        [Tooltip("Max spring length: this can be whatever as long as its bigger than rest length obvs")]
        [SerializeField] private float maxLength = 20;
        private float extensionVelocity;
        private float momentum = 0;
        public float radialAcceleration { get; set; } = 0;

        public bool canControl { get; set; } = true;

        [SerializeField] private Transform pivotTransform;

        [Header("Swing Detection")]
        [SerializeField] private float swingAccelerationThreshold = 2.5f;
        [SerializeField] private float swingCooldown = 0.6f;

        private float lastSwingTime = -999f;

        private Quaternion attitude;
        private Vector3 frameAcceleration;
        private Vector3 velocity;

        public Vector3 Velocity => velocity;
        public Vector3 Acceleration => frameAcceleration;

        private IController _controllerRef;
        private AudioSource audioSource;

        public AudioClip whooshClip;

        void Awake()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f;
            audioSource.volume = 1f;
            audioSource.playOnAwake = false;
        } 

        void Start()
        {
            _controllerRef = HammerManager.Instance.hammerController;
        }

        public void CalibrateHammer()
        {
            _controllerRef.Update();
            attitude = _controllerRef.GetAttitude();
            HammerManager.Instance.SaveCalibration(Quaternion.Inverse(attitude));

        }

        void UpdateRotation()
        {
            transform.localRotation = HammerManager.Instance.CalibrationQuaternion * attitude;
        }

        void UpdatePosition()
        {
            Vector3 worldForward = transform.rotation * Vector3.forward;
            radialAcceleration = Vector3.Dot(frameAcceleration, worldForward);
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
            if (!canControl)
            {
                velocity = Vector3.zero;
                frameAcceleration = Vector3.zero;
                extensionVelocity = 0;
                momentum = 0;
                return;
            }
            _controllerRef.Update();
            attitude = _controllerRef.GetAttitude();
            frameAcceleration = _controllerRef.GetAcceleration();
            velocity += frameAcceleration * Time.deltaTime;
            velocity *= 0.98f;

            UpdateRotation();
            UpdatePosition();
            CheckForSwing();
        }

        void CheckForSwing()
        {
            Vector3 worldForward = transform.rotation * Vector3.forward;
            radialAcceleration = Vector3.Dot(frameAcceleration, worldForward);

            if (Mathf.Abs(radialAcceleration) >= swingAccelerationThreshold && Time.time >= lastSwingTime + swingCooldown)
            {
                lastSwingTime = Time.time;
                OnHammerSwing?.Invoke();
                Debug.Log("Hammer swing detected");
                if (whooshClip != null)
                {
                    audioSource.PlayOneShot(whooshClip, 1f);
                }
            }
        }

        public void Rumble()
        {
            _controllerRef.Rumble();
        }
        
        public void SlamRumble()
        {
            _controllerRef.SlamRumble();
        }

        public void DragRumble()
        {
            _controllerRef.DragRumble();
        }

        public void HitRumble()
        {
            _controllerRef.HitRumble();
        }

        public void DestroyRumble()
        {
            _controllerRef.DestroyRumble();
        }
    }

}
