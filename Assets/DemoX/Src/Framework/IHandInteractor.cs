using Unity.XR.PXR;
using UnityEngine;

namespace DemoX.Framework
{
    public interface IHandInteractor
    {
        public bool IsPinching { get; }
        public bool IsExactPinching { get; }
        public bool IsFist { get; }
        public Transform PinchAttach { get; }
        public Transform HandCenterPoint { get; }

        public bool IsLocked();
        public bool IsLockedBy(Transform lockBy);
        public bool SLock(Transform lockBy);
        public bool SUnlock(Transform lockBy);

        public void Visible(bool visible);

        public Vector3 Vel { get; }
        public HandType HandType { get; }
    }
}