using System;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy
{
    public class KnockbackHandler : MonoBehaviour,  IKnockbackState
    {
        public bool IsKnockedBack { get; private set; }

        public Action KnockbackEnded;
        
        private const float KnockbackTime = 0.5f;
        
        private const float GroundCheckDistance = 0.4f;
        
        private float knockbackTimer;

        [SerializeField] private NavMeshAgent agent;

        private Rigidbody _rb;

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
        }
        
        public bool IsGrounded()
        {
            return Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, GroundCheckDistance + 0.2f);
        }

        public void ApplyKnockback(Vector3 force, bool playHitAnim = true)
        {
            IsKnockedBack = true;
            knockbackTimer = KnockbackTime;

            if (agent != null)
            {
                agent.enabled = false;
            }

            if (_rb != null)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.AddForce(force, ForceMode.Impulse);
            }

            // if (playHitAnim)
            // {
            //     TryTrigger(hitTrigger);
            // }
        }
        
        private void HandleKnockback()
        {
            knockbackTimer -= Time.deltaTime;

            if (knockbackTimer <= 0.0f && IsGrounded())
            {
                IsKnockedBack = false;
                knockbackTimer = KnockbackTime;
                
                KnockbackEnded?.Invoke();

                if (agent != null)
                {
                    agent.enabled = true;
                    if (!agent.isOnNavMesh && NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                    {
                        agent.Warp(hit.position);
                    }
                }
            }
        }

        private void Update()
        {
            if (IsKnockedBack)
            {
                HandleKnockback();
            }
        }
    }
}