using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem; //temp
using Score;
using Enemy;
using UnityEngine.UIElements;

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
        [SerializeField] private GameObject aweSlamEffectsPrefab; 

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
        public float slamForce;
        public float aweSlamRadius;
        public float aweSlamForce;


        private void changeHammerChargeState(hammerChargeState newState)
        {
            if (hammerChargeState != newState) {
                hammerChargeState = newState;
                chargeStateChange.Invoke(hammerChargeState);
            } // else Debug.Log("hammer state change event despite state remaining the same as: "+newState);
        }
        
        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            Debug.Assert(_hitbox != null);
            scoreSettings = Resources.Load<ScoreSettings>("ScoreSettings");
            timeHeldUp = 0;
        }


        void FixedUpdate()
        {
            //if not already charged, check if the player is holding the hammer above their head
            if (hammerChargeState != hammerChargeState.charged) {
                float angleToUp = Vector3.Angle(Vector3.up, transform.up); //gives signed angle          
                if (angleToUp < chargingZoneSize && angleToUp > 0.0f)
                {
                    if (timeHeldUp > timeToChargeSlam) changeHammerChargeState(hammerChargeState.charged);
                    else if (timeHeldUp > 0.05 && hammerChargeState != hammerChargeState.charging)
                    {
                        changeHammerChargeState(hammerChargeState.charging);
                    }
                    timeHeldUp += Time.deltaTime;
                }
                else
                {
                    if (hammerChargeState != hammerChargeState.uncharged)
                    {
                        changeHammerChargeState(hammerChargeState.uncharged);
                    }
                    timeHeldUp = 0;
                }
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
            //if (_targetHammer.radialAcceleration <= slamAccelThreshold)
            //{
                if (ScoreManager.Instance.AweScore >= ScoreManager.Instance.MaxAweScore) {
                    doAweSlam(slamCenter);
                    ScoreManager.Instance.ResetAwe();

                } else doSlam(slamCenter);
            
            //}
            //_targetHammer.Rumble();
        }
        


        void doSlam(Vector3 slamCenter)
        {
            //Debug.Log("boom")

            
            slam.Invoke(); //fling player + other effects
            
            Instantiate(slamEffectsPrefab,slamCenter,Quaternion.identity);

            Collider[] colliders = Physics.OverlapSphere(slamCenter, slamRadius,LayerMask.GetMask("Enemy", "Buildings", "normalEnemy"));
            foreach (Collider c in colliders)
            {
                if (c.GetComponentInParent<DestructibleObject>())
                {   
                    c.GetComponentInParent<DestructibleObject>().Break(c.ClosestPointOnBounds(transform.position), 100);
                }
                // TODO particles
                // TODO use aarons ragdolls
                if (c.GetComponentInParent<RagdollDeathHandler>())
                {
                    c.GetComponentInParent<RagdollDeathHandler>().KilledBy(slamCenter,slamForce);
                    if (c.GetComponentInParent<BodyHit>() != null) 
                    {
                        c.GetComponentInParent<BodyHit>().awardScoreForSlamAttack(); //they better have a body hit
                    }
                }

            }
            changeHammerChargeState(hammerChargeState.uncharged);
            timeHeldUp = 0;

        }
        void doAweSlam(Vector3 slamCenter)
        {
            //Debug.Log("boom")

            
            slam.Invoke(); //fling player + other effects

            Instantiate(aweSlamEffectsPrefab,slamCenter,Quaternion.identity);

            Collider[] colliders = Physics.OverlapSphere(slamCenter, aweSlamRadius, LayerMask.GetMask("Enemy", "Buildings", "normalEnemy"));
            foreach (Collider c in colliders)
            {
                if (c.GetComponentInParent<DestructibleObject>())
                {
                    c.GetComponentInParent<DestructibleObject>().Break(c.ClosestPointOnBounds(transform.position), 200);
                }
                // TODO particles
                // TODO use aarons ragdolls
                if (c.GetComponentInParent<RagdollDeathHandler>())
                {
                    c.GetComponentInParent<RagdollDeathHandler>().KilledBy(slamCenter,aweSlamForce);
                    if (c.GetComponentInParent<BodyHit>() != null) 
                    {
                        c.GetComponentInParent<BodyHit>().awardScoreForSlamAttack(); //they better have a body hit
                    }
                }

            }
            changeHammerChargeState(hammerChargeState.uncharged);
            timeHeldUp = 0;
            ScoreManager.Instance.ResetAwe();

        }
    }
}// TODO make big swing also push horse
