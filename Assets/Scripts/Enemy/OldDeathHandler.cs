using System;
using UnityEngine;

namespace Enemy
{
    public class OldDeathHandler : MonoBehaviour, IDeathState
    {
        [SerializeField] private bool useTutorialKillLock = false;
        public bool CanBeKilled { get; private set; } = true;
        
        [SerializeField] private float maxDeathTime = 4f;
        
        public bool IsDying { get; private set; }
        
        public event Action OnDied;
        
        private float _deathTimer;
        private float _deathEndTime;
        
        private IKnockbackState _knockbackState;

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

            StartDeathTimer();

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

            _knockbackState.ApplyKnockback(knockDir * force);
            animator.SetTrigger(deadTrigger);

            //TryTrigger(deadTrigger);
            OnDied?.Invoke();
        }

        private void StartDeathTimer()
        {
            IsDying = true;
            _deathTimer = Time.time;
            _deathEndTime = _deathTimer + maxDeathTime;
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