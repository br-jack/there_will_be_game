using UnityEngine;

namespace Hammer
{
    public interface IRotatable
    {
        public void Connect();

        public Vector3 GetRotationOffset();

        public void Cleanup();
    }
}