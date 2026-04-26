using System.Linq;
using UnityEngine;

namespace Enemy
{
    public class RagdollToggler : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private Transform ragdollRoot;
        
        [SerializeField] private bool startAsRagdoll;

        //Rigidbodies and colliders that shouldn't be disabled by enabling ragdoll
        [SerializeField] private Rigidbody[] normalRigidbodies;
        [SerializeField] private Collider[] normalColliders;

        private Rigidbody[] _rigidbodies;
        private CharacterJoint[] _characterJoints;
        private Collider[] _colliders;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Awake()
        {
            _rigidbodies = ragdollRoot.GetComponentsInChildren<Rigidbody>().Where(rb => rb.gameObject != ragdollRoot.gameObject).ToArray();
            _colliders = ragdollRoot.GetComponentsInChildren<Collider>().Where(col =>  col.gameObject != ragdollRoot.gameObject).ToArray();
            //NOTE: assume root doesn't need/have any character joints
            _characterJoints = ragdollRoot.GetComponentsInChildren<CharacterJoint>();
            
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
                Debug.Assert(!col.isTrigger);
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
                Debug.Assert(!col.isTrigger);
                col.enabled = false;
            }
            foreach (Rigidbody rb in _rigidbodies)
            {
                rb.isKinematic = true;
                //NOTE: this may prevent raycast from working on the ragdoll colliders.
                rb.detectCollisions = false;
            }
        }
    }
}

