using System;
using UnityEngine;

namespace Hammer
{
    public interface IController
    {
        public void resetAxes() 
        {Debug.Log("This controller's axes cannot be reset");} //default
        
        public void Connect();
        
        public void Update();

        public Quaternion GetAttitude();

        public Vector3 GetAcceleration();

        public void Cleanup();
    }
}