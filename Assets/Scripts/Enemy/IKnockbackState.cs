namespace Enemy
{
    public interface IKnockbackState
    {
        public bool IsKnockedBack { get; }
        public bool IsGrounded();
    }
}