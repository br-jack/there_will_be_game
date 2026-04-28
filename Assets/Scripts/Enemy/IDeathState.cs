using System;
using UnityEngine;

namespace Enemy
{
    public interface IDeathState
    {
        public bool IsDying { get; }
        public bool CanBeKilled { get; }
        public event Action OnDied;

        public void EnableTutorialKillLockMode();
        public void SetCanBeKilled(bool canBeKilled);
        public void KilledBy(Collider other, AttackHitbox hitBox);
        public void KilledByFire(Vector3 sourcePosition, float force = 18f);
    }
}