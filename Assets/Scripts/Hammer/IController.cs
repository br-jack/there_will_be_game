using UnityEngine;

namespace Hammer
{
    public interface IController
    {
        //For changing axis alignment in the editor at runtime if the axes get in a mess. No need to implement
        public void UpdateTestQuaternionVariables(bool flipXInAttitudeQuaternion,
            bool flipYInAttitudeQuaternion,
            bool flipZInAttitudeQuaternion,
            int indexSentToXInOuputQuaternion,
            int indexSentToYInOuputQuaternion,
            int indexSentToZInOuputQuaternion) 
        {return;} //default method

        public void Connect();
        
        public void Update();

        public Quaternion GetAttitude();

        public Vector3 GetAcceleration();

        public void Cleanup();

        
    }
}