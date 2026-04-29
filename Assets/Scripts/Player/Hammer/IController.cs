using System;
using UnityEngine;

namespace Hammer
{
    public interface IController
    {
        public void Connect();
        public void Update();
        public Quaternion GetAttitude();
        public Vector3 GetAcceleration();

        public void Rumble(int msDuration)
        {
            
        }

        public void Rumble()
        {} //optional, UR doesn't have

        public void SlamRumble()
        {
            
        }

        public void DragRumble()
        {
            
        }

        public void HitRumble()
        {
            
        }

        public void DestroyRumble()
        {
            
        }

        public void Cleanup();
    }
}
