using System.Collections;
using System.Collections.Generic;
using DemoX.Framework.Core;
using DG.Tweening;
using Mirror;
using UnityEngine;

namespace DemoX.Framework.Level
{
    public class Handleable : NetworkBehaviour
    {
        [Header("Remote Control")] [SerializeField]
        private float _handleTenacity = 0.1f;

        [SerializeField] private float _remoteHandleTenacity = 0.8f;

        [Header("Hand Control")] [SerializeField]
        private float _minScaleMultiple = 0.5f;

        [SerializeField] private float _maxScaleMultiple = 1.5f;
        [SerializeField] private float _scaleTenacity = 0.3f;

        [Header("Physic Model")] [SerializeField]
        private bool _rigidbodyEnableAfterRelease;

        [SerializeField] private float _velMultiple = 2.0f;
        [SerializeField] private float _changeModelDuration;

        [SerializeField] private bool _doorTrigger;

        [Header("Menu")] [SerializeField] private bool _handControlToValid;
        [SerializeField] private MeshRenderer _renderer;
        [Header("or")] [SerializeField] private Transform _models;

        public bool DoorTrigger => _doorTrigger;

        public enum EHandleableState
        {
            Free,
            Move,
            Manipulate
        }

        private EHandleableState _eHandleableState;

        private Transform _target;
        private Vector3 _followOffset;

        private float _pullDuration;

        private readonly OrderedUniqueSet<IHandInteractor> _interactingHands = new();

        private const int MAX_HANDLE_COUNT = 2;

        private readonly List<IHandInteractor> _handlingHands = new();

        private float _handsDistance;
        private Vector3 _handleableStartScale;


        private Rigidbody _rigidbody;

        private GameObject _coordinateGameObj;
        private GameObject _fakeGameObj;
        private Rigidbody _rigidbodyFaker;

        private int _lastHandCount;

        [SyncVar(hook = nameof(OnRendererEnableChanged))]
        private bool _bRendererEnable;

        private Vector3 _lastPosition;
        private Vector3 _vel;

        private IHandInteractor _lastHandInteractor;

        public IHandInteractor LastHandInteractor => _lastHandInteractor;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();

            if (_handControlToValid && HasRenderedModel())
            {
                SetRenderedModelValid(false);
            }

            _lastPosition = transform.position;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            _coordinateGameObj = new("Coordinate");
            _fakeGameObj = new("PointInCoordinate");
            _fakeGameObj.transform.SetParent(_coordinateGameObj.transform);

            GameObject rbg = new GameObject("RigidbodyHandleable");
            rbg.transform.parent = transform;
            _rigidbodyFaker = rbg.AddComponent<Rigidbody>();
            _rigidbodyFaker.useGravity = false;
            _rigidbodyFaker.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _rigidbodyFaker.interpolation = RigidbodyInterpolation.Extrapolate;
        }

        public bool TryStartMove(Transform followTo, bool keepDistance = false, TweenCallback callback = null)
        {
            if (!isServer) return false;
            if (!followTo || !IsFree() || _handControlToValid) return false;

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
            Game.Log($"SHandle_TAG: free");
            transform.DOKill();
            _coordinateGameObj.transform.DOKill();
            _coordinateGameObj.transform.localScale = Vector3.one;
            ChangeState(EHandleableState.Free);
            _target = null;

            if (!_rigidbodyFaker) return;
            _rigidbodyFaker.transform.parent = null;
            transform.parent = _rigidbodyFaker.transform;
            _rigidbodyFaker.useGravity = true;
            _rigidbodyFaker.velocity = _vel * _velMultiple;
        }

        private IEnumerator Move(TweenCallback callback)
        {
            while (_eHandleableState == EHandleableState.Move && _target)
            {
                transform.DOMove(_target.position + _followOffset, _remoteHandleTenacity).onComplete = callback;
                yield return null;
            }
        }

        private void Update()
        {
            SHandle();
        }

        private void SHandle()
        {
            if (!isServer || _eHandleableState != EHandleableState.Manipulate) return;

            // 1. 剔除解除操作的手
            // 2. 挑选开启操作模式的手补充到 _handlingHands

            _handlingHands.Clear();
            foreach (var hand in _interactingHands.GetInsertionOrder())
            {
                if (hand.IsPinching && hand.SLock(transform))
                {
                    _handlingHands.Add(hand);

                    if (_handlingHands.Count == MAX_HANDLE_COUNT) break;
                }
                else if (!hand.IsPinching)
                {
                    hand.SUnlock(transform);
                }
            }

            if (_handlingHands.Count == 0 && _lastHandCount == 0)
            {
                _lastHandCount = 0;
                return;
            }

            if (_handlingHands.Count == 0 && _lastHandCount != 0)
            {
                _lastHandCount = 0;
                Free();
                return;
            }

            // 3.计算操作坐标系
            if (_handlingHands.Count == 1)
            {
                IHandInteractor hand = _handlingHands[0];
                Transform attach = hand.PinchAttach;

                _coordinateGameObj.transform.SetPositionAndRotation(attach.position, attach.rotation);
                _vel = hand.Vel;
            }
            else if (_handlingHands.Count == MAX_HANDLE_COUNT)
            {
                Transform firstHandAttach = _handlingHands[0].PinchAttach;
                Transform secondHandAttach = _handlingHands[1].PinchAttach;
                Vector3 toSecondVec = secondHandAttach.position - firstHandAttach.position;

                _coordinateGameObj.transform.position = firstHandAttach.position + toSecondVec / 2.0f;
                _coordinateGameObj.transform.forward = Vector3.Normalize(toSecondVec);
            }

            if (_handlingHands.Count != 0 && _lastHandCount == 0)
            {
                SCheckValidState();

                if (_rigidbodyFaker)
                {
                    _rigidbodyFaker.transform.SetParent(null);
                    _rigidbodyFaker.useGravity = false;
                    _rigidbodyFaker.velocity = Vector3.zero;
                }

                transform.SetParent(null);
            }

            bool bHandCountChanged = _handlingHands.Count != _lastHandCount;
            _lastHandCount = _handlingHands.Count;


            // 5.缩放
            // 计算两手初始距离
            if (bHandCountChanged)
            {
                if (_handlingHands.Count == MAX_HANDLE_COUNT)
                {
                    Vector3 handsPosSum = Vector3.zero;
                    Vector3 handToHand = Vector3.zero;

                    foreach (var h in _handlingHands)
                    {
                        Vector3 attachPosition = h.PinchAttach.position;
                        handsPosSum += attachPosition;
                        handToHand = attachPosition - handToHand;
                    }

                    _handsDistance = handToHand.magnitude;
                    _handleableStartScale = transform.localScale;
                }
            }

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
                scale = _handleableStartScale.x * scale;
                scale = Mathf.Clamp(scale, _minScaleMultiple, _maxScaleMultiple);
                Vector3 localScale = new Vector3(scale, scale, scale);

                _coordinateGameObj.transform.localScale = localScale;
                transform.localScale = localScale;
            }

            // 4.移动 & 旋转
            if (bHandCountChanged)
            {
                _fakeGameObj.transform.SetPositionAndRotation(transform.position, transform.rotation);
            }
            else
            {
                transform.rotation = _fakeGameObj.transform.rotation;
                transform.DOMove(_fakeGameObj.transform.position, _handleTenacity);
            }
        }

        private void ChangeState(EHandleableState handleableState)
        {
            Game.Log($"ChangeState: {handleableState}  {_eHandleableState}");
            _eHandleableState = handleableState;
        }

        /// <summary>
        /// 如果开启了“从菜单中拖拽出 3D 模型”模式，那么要在这里开启 renderer
        /// </summary>
        private void SCheckValidState()
        {
            if (_bRendererEnable || !HasRenderedModel()) return;
            _bRendererEnable = true;
        }

        private void OnRendererEnableChanged(bool _, bool newVal)
        {
            if (newVal)
            {
                transform.parent = null;
            }

            SetRenderedModelValid(newVal);
        }

        private bool HasRenderedModel()
        {
            return _renderer || _models;
        }

        private void SetRenderedModelValid(bool valid)
        {
            if (_renderer)
            {
                _renderer.enabled = valid;
            }

            if (_models)
            {
                _models.gameObject.SetActive(valid);
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.transform.CompareTag("Ground")
                && _eHandleableState == EHandleableState.Free
                && _rigidbodyEnableAfterRelease
                && !_doorTrigger)
            {
                transform.DOScale(_maxScaleMultiple, _changeModelDuration);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            Game.Log($"OnTriggerEnter OnTriggerEnter: {other.transform}");
            if (!isServer) return;

            if (!other.CompareTag("HandSpace")) return;

            IHandInteractor handRayHandInteractor = other.transform.GetComponent<IHandInteractor>();

            if (handRayHandInteractor == null || handRayHandInteractor.IsLocked()) return;

            _lastHandInteractor = handRayHandInteractor;
            _interactingHands.Add(handRayHandInteractor);
            ChangeState(EHandleableState.Manipulate);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!isServer) return;
            if (!other.CompareTag("HandSpace")) return;
            IHandInteractor handRayHandInteractor = other.transform.GetComponent<IHandInteractor>();

            if (!handRayHandInteractor.IsPinching && !handRayHandInteractor.IsLockedBy(transform))
            {
                Game.Log($"SHandle_TAG eee");

                _interactingHands.Remove(handRayHandInteractor);

                if (_eHandleableState == EHandleableState.Manipulate && _interactingHands.Count == 0)
                {
                    ChangeState(EHandleableState.Free);
                }
            }
        }
    }
}