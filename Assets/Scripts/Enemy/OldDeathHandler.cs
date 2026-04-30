using System;
using UnityEngine;

namespace Enemy
{
    public class OldDeathHandler : MonoBehaviour, IDeathState
    {
        [SerializeField] private bool useTutorialKillLock = false;
        public bool CanBeKilled { get; private set; } = true;
        
        [SerializeField] private float maxDeathTime = 4f;
        [SerializeField] private float maxFireDeathTime = 3f;
        
        public bool IsDying { get; private set; }
        
        public event Action OnDied;
        
        private bool isOnFire = false;
        
        private float _deathTimer;
        private float _deathEndTime;
        
        private IKnockbackState _knockbackState;
        public IKnockbackState KnockbackState => _knockbackState;
        public Transform Transform => transform;
        
        [SerializeField] private Animator animator;
        [SerializeField] private string deadTrigger = "Die";

        public void Init(IKnockbackState knockbackState)
        {
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
            
            if (hitBox != null)
            {
                _knockbackState.ApplyKnockbackToAll(_knockbackState.CalcKnockbackForce(other.transform, hitBox.GetKnockbackForce()));
            }
            else _knockbackState.ApplyKnockbackToAll(_knockbackState.CalcKnockbackForce(other.transform, 30f));
            
            animator.SetTrigger(deadTrigger);

            //TryTrigger(deadTrigger);
            OnDied?.Invoke();
            
            hitBox.KilledEnemy();
        }

        public void KilledBy(Vector3 position, float force)
        {
            if (IsDying)
            {
                return;
            }

            if (!CanBeKilled)
            {
                return;
            }
            
            isOnFire = true;

            StartDeathTimer();

            Renderer r = GetComponent<Renderer>() ?? GetComponentInChildren<Renderer>();
            if (r != null)
            {
                //FIXME this may not properly tint the whole enemy grey
                r.material.color = Color.gray;
            }
            
            animator.SetTrigger(deadTrigger);
            
            _knockbackState.ApplyKnockbackToAll(_knockbackState.CalcKnockbackForce(position, force));

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

            //TryTrigger(deadTrigger);
            OnDied?.Invoke();
        }

        private void StartDeathTimer()
        {
            IsDying = true;
            _deathTimer = Time.time;
            if (isOnFire)
            {
                _deathEndTime = _deathTimer + maxFireDeathTime;
            }
            else
            {
                _deathEndTime = _deathTimer + maxDeathTime;
            }
        }

        private void TickDeathTimer()
        {
            _deathTimer += Time.deltaTime;
            
            if (!_knockbackState.IsKnockedBack || _deathTimer >= _deathEndTime)
            {
                Destroy(gameObject);
            }
        }

        // Update is called once per frame
        private void Update()
        {
            if (IsDying)
            {
                Debug.Assert(_knockbackState != null);
                
                TickDeathTimer(); 
                return;
            }
        }
    }
}