using Unity.XR.PXR;
using UnityEngine;

namespace DemoX.Framework
{
    public class HandSeparateHandInteractor : MonoBehaviour, IHandInteractor
    {
        [SerializeField] private HandType _handType;
        public bool IsPinching => _handRayHandInteractor && _handRayHandInteractor.IsPinching;
        public bool IsExactPinching => _handRayHandInteractor && _handRayHandInteractor.IsExactPinching;
        public bool IsFist => _handRayHandInteractor && _handRayHandInteractor.IsFist;

        public Transform PinchAttach => _handRayHandInteractor ? _handRayHandInteractor.PinchAttach : null;
        public Transform HandCenterPoint => _handRayHandInteractor ? _handRayHandInteractor.HandCenterPoint : null;

        private HandRayHandInteractor _handRayHandInteractor;

        public HandRayHandInteractor Hand => _handRayHandInteractor;
        public HandType HandType => _handType;

        public Vector3 Vel => _handRayHandInteractor.Vel;

        private void Awake()
        {
            foreach (var hand in GetComponentsInParent<HandRayHandInteractor>())
            {
                if (hand.WhichHand == _handType)
                {
                    _handRayHandInteractor = hand;
                    break;
                }
            }
        }

        public bool IsLocked()
        {
            return _handRayHandInteractor.IsLocked();
        }

        public bool IsLockedBy(Transform lockBy)
        {
            return _handRayHandInteractor && _handRayHandInteractor.IsLockedBy(lockBy);
        }

        public bool SLock(Transform lockBy)
        {
            return _handRayHandInteractor && _handRayHandInteractor.SLock(lockBy);
        }

        public bool SUnlock(Transform lockBy)
        {
            return _handRayHandInteractor && _handRayHandInteractor.SUnlock(lockBy);
        }

        public void Visible(bool visible)
        {
            if (_handRayHandInteractor)
            {
                _handRayHandInteractor.Visible(visible);
            }
        }
    }
}