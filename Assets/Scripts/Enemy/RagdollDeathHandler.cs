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
        
        public IKnockbackState KnockbackState => _knockbackState;
        public Transform Transform => transform;

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

            if (!CanBeKilled)
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
            
            if (hitBox != null)
            {
                _knockbackState.ApplyKnockbackToAll(_knockbackState.CalcKnockbackForce(other.transform, hitBox.GetKnockbackForce()));
            }
            else _knockbackState.ApplyKnockbackToAll(_knockbackState.CalcKnockbackForce(other.transform, 30f));
            

            //TryTrigger(deadTrigger);
            OnDied?.Invoke();
        }

        public void KilledBy(Vector3 other, float force)
        {
            if (IsDying)
            {
                return;
            }

            if (!CanBeKilled)
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
            
            _knockbackState.ApplyKnockbackToAll(_knockbackState.CalcKnockbackForce(other, force));

            //TryTrigger(deadTrigger);
            OnDied?.Invoke();
        }
        
        public void KilledByFire(Vector3 sourcePosition, float force = 18f)
        {
            if (IsDying) return;
            if (!CanBeKilled) return;

            StartDeathTimer();
            
            Renderer r = GetComponent<Renderer>() ?? GetComponentInChildren<Renderer>();
            if (r != null) r.material.color = new Color(0.2f, 0.2f, 0.2f);
            
            _ragdollToggler.UseRagdoll();

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