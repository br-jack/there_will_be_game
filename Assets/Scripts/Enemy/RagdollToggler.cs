using UnityEngine;

namespace Enemy
{
    public class RagdollToggler : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private Transform ragdollRoot;
        
        [SerializeField] private bool startAsRagdoll;

        private Rigidbody[] _rigidbodies;
        private CharacterJoint[] _characterJoints;
        private Collider[] _colliders;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Awake()
        {
            _rigidbodies = ragdollRoot.GetComponentsInChildren<Rigidbody>();
            _characterJoints = ragdollRoot.GetComponentsInChildren<CharacterJoint>();
            _colliders = ragdollRoot.GetComponentsInChildren<Collider>();

            if (startAsRagdoll)
            {
                UseRagdoll();
            }
            else
            {
                UseAnimator();
            }
        }

        public void UseRagdoll()
        {
            animator.enabled = false;
            foreach (CharacterJoint joint in _characterJoints)
            {
                joint.enableCollision = true;
            }

            foreach (Collider col in _colliders)
            {
                col.enabled = true;
            }
            foreach (Rigidbody rb in _rigidbodies)
            {
                rb.isKinematic = false;
                rb.detectCollisions = true;
            }
        }
        
        public void UseAnimator()
        {
            animator.enabled = true;
            foreach (CharacterJoint joint in _characterJoints)
            {
                joint.enableCollision = false;
            }

            foreach (Collider col in _colliders)
            {
                //TODO change this to not count the capsule colliders at the root which are used by CPU
                col.enabled = false;
            }
            foreach (Rigidbody rb in _rigidbodies)
            {
                //TODO change this to not count the rigidbody at the root which is used by CPU
                rb.isKinematic = true;
                //NOTE: this may prevent raycast from working on the ragdoll colliders.
                rb.detectCollisions = false;
            }
        }
    }
}

