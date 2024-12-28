using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace DemoX.Framework.Core
{
    public class YRotateTrackedPoseDriver : TrackedPoseDriver
    {
        protected override void SetLocalTransform(Vector3 newPosition, Quaternion newRotation)
        {
            Vector3 euler = newRotation.eulerAngles;
            euler.x = 0;
            euler.z = 0;
            newRotation = Quaternion.Euler(euler);
            
            base.SetLocalTransform(newPosition, newRotation);
        }
    }
}