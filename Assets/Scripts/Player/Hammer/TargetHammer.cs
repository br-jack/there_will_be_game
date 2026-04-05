using UnityEngine;

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

        [SerializeField] private Transform pivotTransform;

        private Quaternion attitude;
        private Vector3 frameAcceleration;

        public Quaternion Rotation
        {
            get { return transform.rotation; }
        }
       
        public Vector3 Position
        {
            get { return transform.position; }
        }

        private IController _controllerRef;

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
        }

    }

}
