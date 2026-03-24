using UnityEngine;

namespace Hammer
{
    public interface IController
    {
        //TEST ONLY, this is dumb
        public void UpdateTestQuaternionVariables(bool flipXInAttitudeQuaternion,
            bool flipYInAttitudeQuaternion,
            bool flipZInAttitudeQuaternion,
            int indexSentToXInOuputQuaternion,
            int indexSentToYInOuputQuaternion,
            int indexSentToZInOuputQuaternion);
        public void Connect();
        
        public void Update();

        public Quaternion GetAttitude();

        public Vector3 GetAcceleration();

        public void Cleanup();

        
    }
}