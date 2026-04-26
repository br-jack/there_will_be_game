using UnityEngine;

namespace Enemy
{
    public interface IKnockbackState
    {
        public bool IsKnockedBack { get; }
        public bool IsGrounded();
        public void ApplyKnockback(Vector3 force, bool playHitAnim = true);
    }
}