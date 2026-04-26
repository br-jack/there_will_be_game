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
        [SerializeField] private Rigidbody[] ragdollRigidbodies;
        [SerializeField] private Collider[] ragdollColliders;
        [SerializeField] private CharacterJoint[] ragdollCharacterJoints;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Awake()
        {
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
            foreach (CharacterJoint joint in ragdollCharacterJoints)
            {
                joint.enableCollision = true;
            }

            foreach (Collider col in ragdollColliders)
            {
                Debug.Assert(!col.isTrigger);
                col.enabled = true;
            }
            foreach (Rigidbody rb in ragdollRigidbodies)
            {
                rb.isKinematic = false;
                rb.detectCollisions = true;
            }
        }
        
        public void UseAnimator()
        {
            animator.enabled = true;
            foreach (CharacterJoint joint in ragdollCharacterJoints)
            {
                joint.enableCollision = false;
            }

            foreach (Collider col in ragdollColliders)
            {
                Debug.Assert(!col.isTrigger);
                col.enabled = false;
            }
            foreach (Rigidbody rb in ragdollRigidbodies)
            {
                rb.isKinematic = true;
                //NOTE: this may prevent raycast from working on the ragdoll colliders.
                rb.detectCollisions = false;
            }
        }
    }
}

