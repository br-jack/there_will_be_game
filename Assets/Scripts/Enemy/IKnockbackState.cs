using System;
using UnityEngine;

namespace Enemy
{
    public interface IKnockbackState
    {
        public bool IsKnockedBack { get; }
        public bool IsGrounded();
        public void ApplyKnockback(Vector3 force);
        public void ApplyKnockbackToAll(Vector3 force);
        public Vector3 CalcKnockbackForce(Transform other, float force);
        public Vector3 CalcKnockbackForce(Vector3 from, float force);
    }
}