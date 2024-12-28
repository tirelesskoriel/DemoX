using DemoX.Framework.Bridge.Event;
using Mirror;
using Unity.XR.PXR;
using UnityEngine;

namespace DemoX.Framework
{
    public class HandPoseTrigger : NetworkBehaviour
    {
        [SerializeField] private PXR_Hand _pxrHand;
        [SerializeField] private Transform _indexTip;
        [SerializeField] private Transform _thumbTip;
        [SerializeField] private Transform _middleTip;
        [SerializeField] private Transform _fistTriggerPoint;
        [SerializeField] private Transform _velTrackerPoint;
        [SerializeField] private Transform _velTracker;

        [SerializeField] private ECSOHandPoseTrigger _ecHandPoseTrigger;

        [SerializeField] private float _inPinchThreshold = 0.03f;
        [SerializeField] private float _outPinchThreshold = 0.05f;

        [SerializeField] private float _inFistThreshold = 0.035f;
        [SerializeField] private float _outFistThreshold = 0.04f;

        [SerializeField] private HandType _handType;

        [SerializeField] private SODebugKeyboardKey _debugKeyboard;

        private bool _bIsPinching;
        private bool _bIsExactPinching;

        private bool _bIsFisting;
        private bool _bLeftWatchDebug;

        private Vector3 _lastVelTrackPosition;


        public void LeftWatchPose(bool start)
        {
            XRLogger.Log($"WatchPose: {start}");

            _ecHandPoseTrigger.HandUITrigger.Invoke(start);
        }

        public void RightWatchPose()
        {
            _ecHandPoseTrigger.ShowDebugLoggerTrigger.Invoke();
        }

        private void Update()
        {
            if (isServer) return;

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                DebugTrigger();
            }
            else
            {
                PinchTrigger();
                FistTrigger();
            }

            CCalVel(Time.deltaTime);
            CCacheVelPoint();

            XRLogger.Pinch(_pxrHand.PinchStrength, _handType);
            XRLogger.Pinch(_bIsPinching, _handType);
        }

        private void DebugTrigger()
        {
            if (!_debugKeyboard) return;
            if (_debugKeyboard.LeftWatchPose.WasPressedThisFrame())
            {
                _bLeftWatchDebug = !_bLeftWatchDebug;
                LeftWatchPose(_bLeftWatchDebug);
            }

            if (_debugKeyboard.Pinching.WasPressedThisFrame())
            {
                if (_bIsPinching)
                {
                    _bIsPinching = false;
                    _bIsExactPinching = false;
                    Pinch(_bIsPinching);
                    ExactPinch(_bIsExactPinching);
                }
                else
                {
                    _bIsPinching = true;
                    _bIsExactPinching = true;
                    Pinch(_bIsPinching);
                    ExactPinch(_bIsExactPinching);
                }
            }

            if (_debugKeyboard.Fisting.WasPressedThisFrame())
            {
                if (_bIsFisting)
                {
                    _bIsFisting = false;
                    Fist(_bIsFisting);
                }
                else
                {
                    _bIsFisting = true;
                    Fist(_bIsFisting);
                }
            }
        }

        private void FistTrigger()
        {
            Vector3 fistTriggerPoint = _fistTriggerPoint.position;
            float indexDistance = Vector3.Distance(_indexTip.position, fistTriggerPoint);
            float middleDistance = Vector3.Distance(_middleTip.position, fistTriggerPoint);

            if (!_bIsFisting && indexDistance <= _inFistThreshold && middleDistance <= _inFistThreshold)
            {
                _bIsFisting = true;
                Fist(_bIsFisting);
            }
            else if (_bIsFisting && indexDistance >= _outFistThreshold && middleDistance >= _outFistThreshold)
            {
                _bIsFisting = false;
                Fist(_bIsFisting);
            }
        }

        private void PinchTrigger()
        {
            // Game.Log($"PinchTrigger: {_pxrHand.Pinch}  {_pxrHand.PinchStrength}");

            float distance = Vector3.Distance(_indexTip.position, _thumbTip.position);

            bool pxrPinch = _pxrHand.PinchStrength > 0.1f;
            if (distance < _inPinchThreshold && pxrPinch && !_bIsPinching)
            {
                _bIsPinching = true;
                Pinch(_bIsPinching);
            }
            else if (distance > _outPinchThreshold && !pxrPinch && _bIsPinching)
            {
                _bIsPinching = false;
                Pinch(_bIsPinching);
            }

            // exact pinch
            if (distance < _inPinchThreshold && !_bIsExactPinching)
            {
                _bIsExactPinching = true;
                ExactPinch(_bIsExactPinching);
            }
            else if (distance > _outPinchThreshold && _bIsExactPinching)
            {
                _bIsExactPinching = false;
                ExactPinch(_bIsExactPinching);
            }
        }

        private void Fist(bool isFist)
        {
            if (isFist)
            {
                _ecHandPoseTrigger.FistStart.Invoke();
            }
            else
            {
                _ecHandPoseTrigger.FistStop.Invoke();
            }
        }

        private void Pinch(bool isStart)
        {
            if (isStart)
            {
                _ecHandPoseTrigger.PinchStart.Invoke();
            }
            else
            {
                _ecHandPoseTrigger.PinchStop.Invoke();
            }
        }

        private void ExactPinch(bool isStart)
        {
            if (isStart)
            {
                _ecHandPoseTrigger.ExactPinchStart.Invoke();
            }
            else
            {
                _ecHandPoseTrigger.ExactPinchStop.Invoke();
            }
        }

        private void CCalVel(float deltaTime)
        {
            _velTracker.position = (_velTrackerPoint.position - _lastVelTrackPosition) / deltaTime;
            _lastVelTrackPosition = _velTrackerPoint.position;
        }

        private void CCacheVelPoint()
        {
            _lastVelTrackPosition = _velTrackerPoint.position;
        }
    }
}