using DemoX.Framework.Core;
using DG.Tweening;
using Mirror;
using UnityEngine;

namespace DemoX.Framework
{
    public class GuideMenuRingController : NetworkBehaviour
    {
        [SerializeField] private Transform _ring;
        [SerializeField] private Vector2 _range;
        [SerializeField] private int _menuCount;

        private IHandInteractor _handInteractor;

        private bool _bLastPinching;

        private Vector3 _startPosition;
        private Quaternion _startRingQuaternion;

        private float _currentAngle;
        private bool _bCheckRotateStart;

        private float _progress;
        public float Progress => _progress;

        private void Update()
        {
            _currentAngle = SpaceTools.ConvertToSignAngle(_ring.localEulerAngles.z);

            SCalProgress();
            SControlRing();
        }

        private void SCalProgress()
        {
            if (!isServer) return;
            _progress = (_currentAngle - _range.y) / Mathf.Abs(_range.x - _range.y);
            _progress = 1 - _progress;
        }

        private void SControlRing()
        {
            if (!isServer) return;
            if (_handInteractor == null)
            {
                CheckRotation();
                return;
            }

            if (!_bLastPinching && _handInteractor.IsPinching)
            {
                StopCheckRotation();
                _startPosition = _handInteractor.PinchAttach.position;
            }
            else if (_handInteractor.IsPinching)
            {
                Vector3 baseAxis = Vector3.Normalize(_ring.position - _startPosition);
                Vector3 currentAxis = Vector3.Normalize(_ring.position - _handInteractor.PinchAttach.position);
                float angleDelta = Vector3.SignedAngle(baseAxis, currentAxis, _ring.forward);

                _ring.RotateAround(_ring.position, _ring.forward, angleDelta);
            }
            else
            {
                CheckRotation();
            }

            _startPosition = _handInteractor.PinchAttach.position;
            _bLastPinching = _handInteractor.IsPinching;
        }

        private void StopCheckRotation()
        {
            _bCheckRotateStart = false;
            _ring.DOKill();
        }

        private void CheckRotation()
        {
            if (_bCheckRotateStart) return;
            _bCheckRotateStart = true;

            float angleRange = _range.x - _range.y;
            float angleBlock = angleRange / (_menuCount - 1);
            float angleThreshold = angleBlock / 2.0f;
            float angle = _range.x;

            for (int i = 0; i < _menuCount; i++)
            {
                if (_currentAngle > _range.x)
                {
                    angle = _range.x;
                    break;
                }

                if (_currentAngle < _range.y)
                {
                    angle = _range.y;
                    break;
                }

                float angleResult = angle - i * angleBlock;

                if (Mathf.Abs(_currentAngle - angleResult) < angleThreshold)
                {
                    angle = angleResult;
                    break;
                }
            }

            _ring.DOKill();
            _bCheckRotateStart = true;
            Quaternion targetQuaternion = Quaternion.Euler(new Vector3(0.0f, 0.0f, angle));
            _ring.DOLocalRotateQuaternion(targetQuaternion, 1.0f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isServer) return;
            if (!other.CompareTag("HandSpace")) return;

            IHandInteractor handRayHandInteractor = other.transform.GetComponent<IHandInteractor>();

            // if (handRayHandInteractor == null || handRayHandInteractor.IsLocked()) return;
            if (handRayHandInteractor == null
                // || handRayHandInteractor.IsLocked()
                || handRayHandInteractor.IsPinching) return;

            _handInteractor = handRayHandInteractor;
            _handInteractor.SUnlock(_ring);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!isServer) return;
            if (!other.CompareTag("HandSpace")) return;
            IHandInteractor handRayHandInteractor = other.transform.GetComponent<IHandInteractor>();

            if (_handInteractor == handRayHandInteractor)
            {
                // _handInteractor.SUnlock(_ring);
                _bLastPinching = false;
                _handInteractor = null;
            }
        }
    }
}