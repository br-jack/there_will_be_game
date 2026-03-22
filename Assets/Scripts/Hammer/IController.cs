using UnityEngine;

namespace Hammer
{
    public interface IController
    {
        public void Connect();
        
        public void Update();

        public Vector3 GetAttitude();

        public Vector3 GetAcceleration();

        public void Cleanup();
    }
}