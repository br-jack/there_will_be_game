using System;
using UnityEngine;

namespace Enemy
{
    public class RagdollDeathHandler : MonoBehaviour, IDeathState
    {
        [SerializeField] private bool useTutorialKillLock = false;
        public bool CanBeKilled { get; private set; } = true;
        
        [SerializeField] private float maxAbsoluteDeathTime = 20f;
        [SerializeField] private float maxGroundedDeathTime = 10f;
        
        public bool IsDying { get; private set; }
        
        public event Action OnDied;
        
        private float _deathAbsoluteTimer;
        private float _deathAbsoluteEndTime;

        private bool _knockbackOver = false;
        private float _deathTimerOnGround;
        private float _deathEndTimeOnGround;
        
        private RagdollToggler _ragdollToggler;
        private IKnockbackState _knockbackState;

        public void Init(RagdollToggler ragdollToggler, IKnockbackState knockbackState)
        {
            _ragdollToggler = ragdollToggler;
            _knockbackState = knockbackState;
        }

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
            if (IsDying)
            {
                return;
            }

            StartDeathTimer();

            Renderer r = GetComponent<Renderer>() ?? GetComponentInChildren<Renderer>();
            if (r != null)
            {
                //FIXME this may not properly tint the whole enemy grey
                r.material.color = Color.gray;
            }
            
            _ragdollToggler.UseRagdoll();

            float force = hitBox != null ? hitBox.GetKnockbackForce() : 30f;
            Vector3 knockDir = transform.position - other.transform.position;
            knockDir.y = Mathf.Clamp(force / 75f, 0.2f, 1.5f);
            knockDir.Normalize();

            _knockbackState.ApplyKnockbackToAll(knockDir * force);

            //TryTrigger(deadTrigger);
            OnDied?.Invoke();
        }

        private void StartDeathTimer()
        {
            IsDying = true;
            _deathAbsoluteTimer = Time.time;
            _deathAbsoluteEndTime = _deathAbsoluteTimer + maxAbsoluteDeathTime;
        }

        private void TickDeathTimer()
        {
            _deathAbsoluteTimer += Time.deltaTime;
            if (_deathAbsoluteTimer >= _deathAbsoluteEndTime)
            {
                Destroy(gameObject);
            }

            if (!_knockbackState.IsKnockedBack && !_knockbackOver)
            {
                _deathTimerOnGround = Time.time;

                if (_knockbackOver)
                {
                    if (_deathTimerOnGround >= _deathEndTimeOnGround)
                    {
                        Destroy(gameObject);
                    }
                }
                else
                {
                    _knockbackOver = true;
                    _deathEndTimeOnGround = _deathAbsoluteTimer + maxGroundedDeathTime;
                }
            }
        }

        // Update is called once per frame
        private void Update()
        {
            if (IsDying)
            {
                Debug.Assert(_ragdollToggler != null);
                Debug.Assert(_knockbackState != null);
                
                TickDeathTimer(); 
            }
        }
    }
}