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
        
        public void ConstantRumble(int msDuration, int strength)
        {
            
        }

        public void GradientRumble(int totalDuration, int startStrength, int endStrength, int fadeDuration)
        {
            
        }

        public void Rumble();

        public void Cleanup();
    }
}
