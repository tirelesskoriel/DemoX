using System.Collections;
using System.Collections.Generic;
using DemoX.Framework;
using DemoX.Framework.Core;
using DG.Tweening;
using UnityEngine;

namespace DemoX.Src.Framework
{
    public class HandleableTest : MonoBehaviour
    {
        [SerializeField] private float _handleTenacity = 0.1f;
        [SerializeField] private float _handleMotionTenacity = 0.8f;
        [SerializeField] private float _remoteHandleTenacity = 0.8f;

        [SerializeField] private float _minScaleMultiple = 0.5f;
        [SerializeField] private float _maxScaleMultiple = 1.5f;
        [SerializeField] private float _scaleTenacity = 0.3f;
        [SerializeField] private Transform _centerPoint;

        [SerializeField] private bool _rigidbodyEnableAfterRelease;
        [SerializeField] private bool _doorTrigger;

        public Transform _axisX;
        public Transform _axisY;
        public Transform _axisZ;
        public bool DoorTrigger => _doorTrigger;

        public enum EHandleableState
        {
            Free,
            Move,
            Manipulate
        }

        private EHandleableState _eHandleableState;
        private EHandleableState _eLastHandleableState;

        public EHandleableState HandleableState => _eHandleableState;

        private Transform _target;
        private Vector3 _followOffset;

        private float _pullDuration;

        private readonly OrderedUniqueSet<IHandInteractor> _handsInteracting = new();

        private class HandlingHandData
        {
            public Vector3 StartPosition;
            public Vector3 OffsetToOrigin;
        }

        private const int MAX_HANDLE_COUNT = 2;

        private readonly List<HandlingHandData> _handlingHandData = new();
        private readonly List<IHandInteractor> _handlingHands = new();

        private Vector3 _handsCenterToOriginOffset;
        private float _handsDistance;
        private Vector3 _handleableStartScale;


        private Rigidbody _rigidbody;

        private void Awake()
        {
            for (int i = 0; i < MAX_HANDLE_COUNT; i++)
            {
                _handlingHandData.Add(new HandlingHandData());
            }

            _rigidbody = GetComponent<Rigidbody>();
            _coordinateGameObj = new("Coordinate");
            _fakeGameObj = new("fake");
            _fakeGameObj.transform.SetParent(_coordinateGameObj.transform);
        }

        public bool TryStartMove(Transform followTo, bool keepDistance = false, TweenCallback callback = null)
        {
            if (!followTo || !IsFree()) return false;

            ChangeState(EHandleableState.Move);
            _rigidbody.isKinematic = true;

            _target = followTo;
            _followOffset = keepDistance ? transform.position - followTo.position : Vector3.zero;

            StartCoroutine(Move(callback));

            return true;
        }

        public bool IsFree()
        {
            return _eHandleableState == EHandleableState.Free;
        }

        public void Free()
        {
            transform.DOKill();
            ChangeState(EHandleableState.Free);
            _target = null;
            if (_rigidbody)
            {
                _rigidbody.isKinematic = !_rigidbodyEnableAfterRelease;
                _rigidbody.velocity = Vector3.zero;
            }
        }

        private IEnumerator Move(TweenCallback callback)
        {
            while (_eHandleableState == EHandleableState.Move && _target)
            {
                transform.DOMove(_target.position + _followOffset, _remoteHandleTenacity).onComplete = callback;
                yield return null;
            }
        }

        private float _timer;
        private bool _logger;


        private Vector3 _cacheForward;
        private Vector3 _cacheUp;
        private Vector3 _cacheRight;
        private Vector3 _cachePosition;

        private Coordinate _coordinate = new();
        private GameObject _coordinateGameObj;
        private GameObject _fakeGameObj;

        private int _lastHandCount;

        private void Update()
        {
            if (Time.realtimeSinceStartup - _timer > 1.0f)
            {
                _timer = Time.realtimeSinceStartup;
                _logger = true;
            }
            else
            {
                _logger = false;
            }

            // 1. 剔除解除操作的手
            for (int i = _handlingHands.Count - 1; i >= 0; i--)
            {
                IHandInteractor hand = _handlingHands[i];
                if (!hand.IsPinching)
                {
                    _handlingHands.RemoveAt(i);

                    // 数据前移，保证数据索引与 _handlingHands 集合一致
                    HandlingHandData handlingHandData = _handlingHandData[i];
                    _handlingHandData.RemoveAt(i);
                    _handlingHandData.Add(handlingHandData);
                }
            }

            // 2. 挑选开启操作模式的手补充到 _handlingHands
            int baseHandCount = _handlingHands.Count;
            int handCounter = baseHandCount;
            if (handCounter != MAX_HANDLE_COUNT)
            {
                foreach (var hand in _handsInteracting.GetInsertionOrder())
                {
                    if (!hand.IsPinching || _handlingHands.Contains(hand)) continue;
                    handCounter += 1;
                    _handlingHands.Add(hand);

                    HandlingHandData handlingHandData = _handlingHandData[_handlingHands.Count - 1];
                    Vector3 attachPos = hand.PinchAttach.position;
                    handlingHandData.StartPosition = attachPos;
                    handlingHandData.OffsetToOrigin = transform.position - attachPos;

                    if (handCounter == MAX_HANDLE_COUNT)
                    {
                        Vector3 handsPosSum = Vector3.zero;
                        Vector3 handToHand = Vector3.zero;

                        foreach (var h in _handlingHands)
                        {
                            Vector3 attachPosition = h.PinchAttach.position;
                            handsPosSum += attachPosition;
                            handToHand = attachPosition - handToHand;
                        }

                        _handsCenterToOriginOffset = transform.position - handsPosSum / _handlingHands.Count;
                        _handsDistance = handToHand.magnitude;
                        _handleableStartScale = transform.localScale;
                        break;
                    }
                }
            }


            if (_handsInteracting.Count == 0 && _eHandleableState == EHandleableState.Manipulate)
            {
                Free();
            }

            bool bHandCountChanged = _handlingHands.Count != _lastHandCount;

            // 3.计算操作坐标系
            if (_handlingHands.Count == 1)
            {
                IHandInteractor hand = _handlingHands[0];
                Transform attach = hand.PinchAttach;

                _coordinateGameObj.transform.position = attach.position;
                _coordinateGameObj.transform.rotation = attach.rotation;
            }
            else if (_handlingHands.Count == MAX_HANDLE_COUNT)
            {
                Transform firstHandAttach = _handlingHands[0].PinchAttach;
                Transform secondHandAttach = _handlingHands[1].PinchAttach;
                Vector3 newCoordinateZ = Vector3.Normalize(secondHandAttach.position - firstHandAttach.position);

                _coordinateGameObj.transform.position = firstHandAttach.position;
                _coordinateGameObj.transform.forward = newCoordinateZ;
            }
            else
            {
                _lastHandCount = 0;
                return;
            }

            // 4.移动旋转
            if (bHandCountChanged)
            {
                _fakeGameObj.transform.position = transform.position;
                _fakeGameObj.transform.rotation = transform.rotation;
            }
            else
            {
                transform.DORotateQuaternion(_fakeGameObj.transform.rotation, 0.2f);
                transform.DOMove(_fakeGameObj.transform.position, 0.2f);
            }

            _lastHandCount = _handlingHands.Count;


            // 5.缩放
            if (_handlingHands.Count == MAX_HANDLE_COUNT)
            {
                Vector3 betweenHands = Vector3.zero;
                for (int i = 0; i < _handlingHands.Count; i++)
                {
                    IHandInteractor hand = _handlingHands[i];
                    Transform handleAttach = hand.PinchAttach;
                    Vector3 handleAttachPosition = handleAttach.position;
                    betweenHands = handleAttachPosition - betweenHands;
                }

                float scale = betweenHands.magnitude / _handsDistance;
                scale = Mathf.Clamp(scale, _minScaleMultiple, _maxScaleMultiple);
                transform.DOScale(_handleableStartScale * scale, _scaleTenacity);
            }
        }

        private void ChangeState(EHandleableState handleableState)
        {
            _eLastHandleableState = _eHandleableState;
            _eHandleableState = handleableState;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("HandSpace")) return;
            IHandInteractor handRayHandInteractor = other.transform.GetComponent<IHandInteractor>();

            XRLogger.Log($"OnTriggerEnter 0: {handRayHandInteractor}");

            if (handRayHandInteractor == null || handRayHandInteractor.IsPinching ||
                !handRayHandInteractor.SLock(transform)) return;

            XRLogger.Log($"OnTriggerEnter 1: {other.transform}");

            _handsInteracting.Add(handRayHandInteractor);
            ChangeState(EHandleableState.Manipulate);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("HandSpace")) return;
            IHandInteractor handRayHandInteractor = other.transform.GetComponent<IHandInteractor>();
            _handsInteracting.Remove(handRayHandInteractor);
            handRayHandInteractor.SUnlock(transform);
            if (_eHandleableState == EHandleableState.Manipulate && _handsInteracting.Count == 0)
            {
                ChangeState(EHandleableState.Free);
            }
        }
    }
}