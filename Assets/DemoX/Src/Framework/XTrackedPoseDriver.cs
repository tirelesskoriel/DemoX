using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace DemoX.Framework
{
    public class XTrackedPoseDriver : TrackedPoseDriver
    {
        [SerializeField] private Vector3 _positionOffset = new(0.0f, -0.71f, 0.0f);

        protected override void SetLocalTransform(Vector3 newPosition, Quaternion newRotation)
        {
            newPosition += _positionOffset;
            base.SetLocalTransform(newPosition, newRotation);
        }
    }
}