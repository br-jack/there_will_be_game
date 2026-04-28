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
        [Tooltip("Seconds")]
        [SerializeField] private float timeToChargeSlam = 3;
        [SerializeField] private float chargingZoneSize = 20;

        [SerializeField] private float slamAccelThreshold;

        private ScoreSettings scoreSettings;
        [SerializeField] private float timeHeldUp;

        [SerializeField] private GameObject slamEffectsPrefab; 

        [SerializeField] private Transform pivotTransform;
        private Rigidbody _rb;

        [Tooltip("This should be from a TargetHammer prefab")]
        [SerializeField] private TargetHammer _targetHammer;

        [Header("Dynamic Hitbox")]
        [SerializeField] private bool useDynamicHitbox = true;
        [SerializeField] private Vector3 smallHitboxSize;
        [SerializeField] private Vector3 smallHitboxCenter;
        [SerializeField] private float mediumHitboxThreshold; //currently set to trail threshold, which may be sensible to maintain?
        [SerializeField] private Vector3 mediumHitboxSize;
        [SerializeField] private Vector3 mediumHitboxCenter;
        [SerializeField] private float largeHitboxThreshold; //currently set to ghost effect threshold, which may be sensible to maintain?
        [SerializeField] private Vector3 largeHitboxSize;
        [SerializeField] private Vector3 largeHitboxCenter;

        //private bool _collisionsEnabled = true;
        [SerializeField] private BoxCollider _hitbox;
        public hammerChargeState hammerChargeState { get; private set; }
        public UnityEvent<hammerChargeState> chargeStateChange;
        public UnityEvent slam;
        //InputAction temporarySlamActivate;


        public float slamRadius;
        public float slamKnockbackAmount;


        private void changeHammerChargeState(hammerChargeState newState)
        {
            if (hammerChargeState != newState) {
                hammerChargeState = newState;
                chargeStateChange.Invoke(hammerChargeState);
            } else Debug.Log("hammer state change event despite state remaining the same");
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

        public void onSlamHitboxTrigger(Vector3 slamCenter)
        {
            if (_targetHammer.radialAcceleration <= slamAccelThreshold)
            {
                doSlam(slamCenter);
            }
            //_targetHammer.Rumble();
        }
        


        void doSlam(Vector3 slamCenter)
        {
            //Debug.Log("boom")

             
            slam.Invoke(); //fling player + other effects

            Instantiate(slamEffectsPrefab,slamCenter,Quaternion.identity);

            Collider[] colliders = Physics.OverlapSphere(slamCenter, slamRadius);
            foreach (Collider c in colliders)
            {
                if (c.GetComponentInParent<DestructibleObject>())
                {
                    c.GetComponentInParent<DestructibleObject>().Break(c.ClosestPoint(transform.position), 3000);
                }
                // TODO particles
                // TODO use aarons ragdolls
            }
            changeHammerChargeState(hammerChargeState.uncharged);
            timeHeldUp = 0;
            ScoreManager.Instance.ResetAwe();

        }
    }
}// TODO make big swing also push horse
