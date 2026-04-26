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

        private Rigidbody[] normalRigidbodies;
        private Collider[] normalColliders;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Awake()
        {
            normalRigidbodies = ragdollRoot.GetComponentsInChildren<Rigidbody>().Where(rb => ragdollRigidbodies.Contains(rb)).ToArray();
            normalColliders = ragdollRoot.GetComponentsInChildren<Collider>().Where(col => ragdollColliders.Contains(col)).ToArray();
            
            if (startAsRagdoll)
            {
                UseRagdoll();
            }
            else
            {
                UseAnimator();
            }
        }

        private void SetColliderStates(Collider[] colliders, bool state)
        {
            foreach (Collider col in colliders)
            {
                col.enabled = state;
            }
        }
        private void SetRigidbodyStates(Rigidbody[] rigidbodies, bool state)
        {
            foreach (Rigidbody rb in ragdollRigidbodies)
            {
                rb.isKinematic = !state;
                rb.detectCollisions = state;
            }
        }
        private void SetCharacterJointState(CharacterJoint[] characterJoints, bool state)
        {
            foreach (CharacterJoint joint in ragdollCharacterJoints)
            {
                joint.enableCollision = state;
            }
        }

        public void UseRagdoll()
        {
            animator.enabled = false;

            SetColliderStates(normalColliders, false);
            SetColliderStates(ragdollColliders, true);
            
            SetRigidbodyStates(normalRigidbodies, false);
            SetRigidbodyStates(ragdollRigidbodies, true);
            
            SetCharacterJointState(ragdollCharacterJoints, true);
        }
        
        public void UseAnimator()
        {
            animator.enabled = true;
            
            SetColliderStates(ragdollColliders, false);
            SetColliderStates(normalColliders, true);
            
            SetRigidbodyStates(ragdollRigidbodies, false);
            SetRigidbodyStates(normalRigidbodies, true);
            
            SetCharacterJointState(ragdollCharacterJoints, false);
        }
    }
}

