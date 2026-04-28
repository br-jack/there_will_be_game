using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem; //temp
using Score;

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
        // [SerializeField] private CharacterController _horseCC;
        public Vector3 smallHitboxSize;
        public Vector3 smallHitboxCenter;
        public float mediumHitboxThreshold; //currently set to trail threshold, which may be sensible to maintain?
        public Vector3 mediumHitboxSize;
        public Vector3 mediumHitboxCenter;
        public float largeHitboxThreshold; //currently set to ghost effect threshold, which may be sensible to maintain?
        public Vector3 largeHitboxSize;
        public Vector3 largeHitboxCenter;
        [Tooltip("Seconds")]
        public float timeToChargeSlam = 3;
        public float chargingZoneSize = 20;

        private ScoreSettings scoreSettings;
        public float timeHeldUp;


        [SerializeField] private Transform pivotTransform;
        private Rigidbody _rb;

        [Tooltip("This should be from a TargetHammer prefab")]
        [SerializeField] private TargetHammer _targetHammer;

        [Header("Dynamic Hitbox")]
        [SerializeField] private bool useDynamicHitbox = true;

        //private bool _collisionsEnabled = true;

        private BoxCollider _hitbox;
        public UnityEvent slam;
        public hammerChargeState hammerChargeState { get; private set; }
        public UnityEvent<hammerChargeState> chargeStateChange;
        //InputAction temporarySlamActivate;


        public float slamRadius;
        public float slamKnockbackAmount;


        private void changeHammerChargeState(hammerChargeState newState)
        {
            hammerChargeState = newState;
            chargeStateChange.Invoke(hammerChargeState);
        }
        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _hitbox = GetComponent<BoxCollider>();
            Debug.Assert(_hitbox != null);
            scoreSettings = Resources.Load<ScoreSettings>("ScoreSettings");
            timeHeldUp = 0;
        }


        void FixedUpdate()
        {
            //check if the player is holding the hammer above their head
            float angleToUp = Vector3.Angle(Vector3.up, transform.up);            
            if (angleToUp < chargingZoneSize && ScoreManager.Instance.AweScore >= ScoreManager.Instance.MaxAweScore)
            {

                if (timeHeldUp > timeToChargeSlam) changeHammerChargeState(hammerChargeState.charged);
                else if (timeHeldUp > 0.05) changeHammerChargeState(hammerChargeState.charging);
                timeHeldUp += Time.deltaTime;
            }
            else if (hammerChargeState != hammerChargeState.charged)
            {
                changeHammerChargeState(hammerChargeState.uncharged);
                timeHeldUp = 0;
            }

            //Debug.Log($"Tensor position: {_rb.inertiaTensor}, Tensor rotation: {_rb.inertiaTensorRotation}");
            if (useDynamicHitbox)
            {
                if (_targetHammer.radialAcceleration < mediumHitboxThreshold)
                {
                    _hitbox.size = smallHitboxSize;
                    _hitbox.center = smallHitboxCenter;
                }
                else if (_targetHammer.radialAcceleration < largeHitboxThreshold)
                {
                    _hitbox.size = mediumHitboxSize;
                    _hitbox.center = mediumHitboxCenter;
                }
                else
                {
                    _hitbox.size = largeHitboxSize;
                    _hitbox.center = largeHitboxCenter;
                }
            }

            // TODO this should probably be continuous
            if (_targetHammer.radialAcceleration < mediumHitboxThreshold)
            {
                _hitbox.size = smallHitboxSize;
                _hitbox.center = smallHitboxCenter;
            }
            else if (_targetHammer.radialAcceleration < largeHitboxThreshold)
            {
                _hitbox.size = mediumHitboxSize;
                _hitbox.center = mediumHitboxCenter;
            }
            else
            {
                _hitbox.size = largeHitboxSize;
                _hitbox.center = largeHitboxCenter;
            }

            // maybe horse acceleration?
            //_rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, horseRigidBody.linearVelocity, 0.8f);

            // MoveToTargetPosition();
            // MoveToTargetRotation();
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (hammerChargeState == hammerChargeState.charged)
            {
                doSlam();
            }
            //_targetHammer.Rumble();
        }
        


        public void doSlam()
        {
            Debug.Log("boom");
            Collider[] colliders = Physics.OverlapSphere(transform.position, slamRadius);
            foreach (Collider c in colliders)
            {
                if (c.GetComponentInParent<StandardEnemyAI>())
                {
                    Vector3 knockbackDirection = c.ClosestPoint(transform.position) - transform.position; //knock away
                    c.GetComponentInParent<StandardEnemyAI>().getKilledBasic(knockbackDirection * slamKnockbackAmount);
                }
                // TODO fling Player

            }
            changeHammerChargeState(hammerChargeState.uncharged);
            timeHeldUp = 0;

        }
    }
}// TODO make big swing also push horse
