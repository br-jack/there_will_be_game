using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem; //temp

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
        public hammerHead head;
        public Vector3 smallHitboxSize;
        public Vector3 smallHitboxCenter;
        public float mediumHitboxThreshold; //currently set to trail threshold, which may be sensible to maintain?
        public Vector3 mediumHitboxSize;
        public Vector3 mediumHitboxCenter;
        public float largeHitboxThreshold; //currently set to ghost effect threshold, which may be sensible to maintain?
        public Vector3 largeHitboxSize;
        public Vector3 largeHitboxCenter;
        public float timeToChargeSlam;
        public float chargingZoneSize;
        private float timeHeldUp;


        [SerializeField] private Transform pivotTransform;
        private Rigidbody _rb;

        [Tooltip("This should be from a TargetHammer prefab")]
        [SerializeField] private TargetHammer _targetHammer;

        [Header("Dynamic Hitbox")]
        [SerializeField] private bool useDynamicHitbox = true;

        //private bool _collisionsEnabled = true;

        private BoxCollider _hitbox;    
        public UnityEvent slam;
        public UnityEvent<hammerChargeState> chargeStateChange;

        InputAction temporarySlamActivate;



        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _hitbox = GetComponent<BoxCollider>();
            Debug.Assert(_hitbox != null);
            timeHeldUp = 0;
        }

        void FixedUpdate()
        {
            //check if the player is holding the hammer above their head
            float angleToUp = Vector3.Angle(Vector3.up,transform.up);
            if (angleToUp < chargingZoneSize) {
                    timeHeldUp += Time.deltaTime;
                } 
            else timeHeldUp = 0;


            if (timeHeldUp > timeToChargeSlam)
            {
                chargeStateChange.Invoke(hammerChargeState.charged);
            }

            
            //Debug.Log($"Tensor position: {_rb.inertiaTensor}, Tensor rotation: {_rb.inertiaTensorRotation}");
            if (useDynamicHitbox)
            {
                if (head.forwardSpeed < mediumHitboxThreshold)
                {
                    _hitbox.size = smallHitboxSize;
                    _hitbox.center = smallHitboxCenter;
                }
                else if (head.forwardSpeed < largeHitboxThreshold)
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

            /*
            Thoughts and notes:
            - There should be a separate mode which makes the hammer more powerful and shiny and stuff.
                - Maybe this is with awe maybe not
                - This should have to be active to destroy buildings
                - It should do an explosion in an area
                - It should fling you upwards
            - If the hammer has significant velocity (maybe rotational too) the horse should be effected
                (like a dash)
            - The target hammer should be update. maybe both are smaller which would let us use the 
                bungeeing more and would maybe be more fun (though maybe we can scrap it considering how massive
                the hammers gonne be anyways)
                - We should deffo use a small spring system instead of teleporting. This would reduce the jitter
                    and allow us to add animations
                - Yeah yeah the way I did it didn't work we can do it though cause now were not bothing with full]
                    collisions
            - The hitbox size should be continuious with whatever velocity we use
                - I dont even know if were going to use the linear acceleration data from the IMU

            Order of operations (TODO):
                1. Decide on the final size of the hammer/ update target hammer movement
                2. Get some actual velocity value that we use (probably a combination of rotation and acceleration)
                3. Use that velocity for the hammer rigid body (even if we teleport it otherwise)
                4. Use velocity for collider size
                5. Use spring system to make visual hammer movement a little nicer
                6. Make a powered mode
                7. Disable building destruction outside powered mode
                8. Slams....
                -. Implement velocity dash

            Actually had a thought about the visual hammer bungeeing: we should just to all the bungee calculations (specifically)
            the force stuff) ourselves then teleport it still. Trust me.

            */

            // TODO this should probably be continuous
            if (head.forwardSpeed < mediumHitboxThreshold)
            {
                _hitbox.size = smallHitboxSize;
                _hitbox.center = smallHitboxCenter;
            }
            else if (head.forwardSpeed < largeHitboxThreshold)
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
            //_targetHammer.Rumble();
        }
    }
}
