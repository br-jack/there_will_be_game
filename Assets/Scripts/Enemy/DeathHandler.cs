using System;
using UnityEngine;

namespace Enemy
{
    public class DeathHandler : MonoBehaviour
    {
        [SerializeField] private bool useTutorialKillLock = false;
        public bool CanBeKilled { get; private set; } = true;
        
        [SerializeField] private float maxDeathTime = 4f;
        
        public bool IsDying { get; private set; }
        
        public event Action OnDied;
        
        private float _deathTimer;
        private float _deathEndTime;
        
        private RagdollToggler _ragdollToggler;

        public void EnableTutorialKillLockMode()
        {
            useTutorialKillLock = true;
            CanBeKilled = true;
        }

        public void SetCanBeKilled(bool canBeKilled)
        {
            if (!useTutorialKillLock)
            {
                return;
            }

            CanBeKilled = canBeKilled;
        }
        
        public void KilledBy(Collider other, AttackHitbox hitBox)
        {
            if (IsDying) return;

            IsDying = true;
            IsKnockedBack = true;
            knockbackTimer = KnockbackTime;
            _deathTimer = maxDeathTime;

            if (agent != null)
            {
                agent.enabled = false;
            }

            Renderer r = GetComponent<Renderer>() ?? GetComponentInChildren<Renderer>();
            if (r != null)
            {
                //FIXME this may not properly tint the whole enemy grey
                r.material.color = Color.gray;
            }

            float force = hitBox != null ? hitBox.GetKnockbackForce() : 30f;
            Vector3 knockDir = transform.position - other.transform.position;
            knockDir.y = Mathf.Clamp(force / 75f, 0.2f, 1.5f);
            knockDir.Normalize();

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.AddForce(knockDir * force, ForceMode.Impulse);
            }

            TryTrigger(deadTrigger);
            OnDied?.Invoke();
        }

        public void StartDeathTimer()
        {
            _deathTimer = Time.time;
            _deathEndTime = _deathTimer + maxDeathTime;
        }

        private void TickDeathTimer()
        {
            _deathTimer += Time.deltaTime;

            if (IsKnockedBack)
            {
                knockbackTimer -= Time.deltaTime;
                if (knockbackTimer <= 0f && IsGrounded())
                {
                    IsKnockedBack = false;
                }
            }

            if (!IsKnockedBack || _deathTimer >= _deathEndTime)
            {
                Destroy(gameObject);
            }
        }

        // Update is called once per frame
        private void Update()
        {
            if (IsDying)
            {
                TickDeathTimer(); 
                return;
            }
        }
    }
}