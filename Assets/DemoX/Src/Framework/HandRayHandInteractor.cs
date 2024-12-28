using System.Collections;
using DemoX.Framework.Bridge.Event;
using DemoX.Framework.Core;
using DemoX.Framework.Level;
using DG.Tweening;
using Mirror;
using Unity.XR.PXR;
using UnityEngine;
using Scene = UnityEngine.SceneManagement.Scene;

namespace DemoX.Framework
{
    public class HandRayHandInteractor : NetworkBehaviour, IHandInteractor
    {
        [Header("RayCast")] [SerializeField] private Transform _rayFollowed;
        [SerializeField] private Transform _rayStart;
        [SerializeField] private Transform _rayClosest;
        [SerializeField] private Transform _rayHitPoint;
        [SerializeField] private LayerMask _rayLayer;
        [SerializeField] private bool _bIsRayEnable = false;

        [SerializeField] private float _maxDistance = 10.0f;
        [SerializeField] private float _stabilizeIntensity = 0.2f;
        [SerializeField] private float _maxStabilizeIntensity = 2.0f;

        [Header("Handle")] [SerializeField] private Transform _pinchAttach;
        [SerializeField] private Transform _pinchAttachFollowed;
        [SerializeField] private float _pinchAttachStabilizeIntensity = 0.2f;
        [SerializeField] private Transform _handSpacePoint;
        [SerializeField] private Transform _velTracker;

        [Header("HandPose")] [SerializeField] private ECSOHandPoseTrigger _ecHandPoseTrigger;
        [SerializeField] private HandType _handType;
        [SerializeField] private SkinnedMeshRenderer _handMeshRenderer;
        [SerializeField] private HandController _handController;
        public HandController HandController => _handController;

        public HandType WhichHand => _handType;

        private Handleable _handleable;
        private RaycastHit _hitInfo;

        public enum EHandState
        {
            None,
            Pinch,
            DoubleTap,
            Lock
        }

        [SyncVar] private EHandState _eHandState;
        public EHandState HandState => _eHandState;

        private Transform _hitTransform;

        public Transform PinchAttach => _pinchAttach;
        public Transform HandCenterPoint => _handSpacePoint;

        private bool _sIsPinching;
        private bool _cIsPinching;
        public bool IsPinching => _sIsPinching || _cIsPinching;

        private bool _sIsExactPinching;
        private bool _cIsExactPinching;
        public bool IsExactPinching => _sIsExactPinching || _cIsExactPinching;


        private bool _sIsFist;
        public bool IsFist => _sIsFist;

        public float DistanceFromHit => _distanceFromHit;
        [SyncVar] private float _distanceFromHit;

        private Vector3 _handleableAttachingLocalPosition;
        [SyncVar] private Vector3 _handleableAttachingPosition;
        private bool _isHandling;
        public Vector3 HandleableAttachingPosition => _handleableAttachingPosition;

        public Transform GetRayStartPoint => _rayStart;

        private Scene _lastScene;

        public Vector3 Vel => _velTracker.position;
        public HandType HandType => _handType;

        private Vector3 _lastPosition;

        private bool _bRayInteractEnable = true;
        public bool IsRayEnable => _bIsRayEnable && _bRayInteractEnable;

        private Transform _lockedBy;

        private void Start()
        {
            StartCoroutine(DebugState());
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (isOwned)
            {
                SetupHandPoseTrigger();
            }
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            if (_handleable)
            {
                _handleable.Free();
            }
        }

        private void Update()
        {
            if (isServer)
            {
                // SCalVel(Time.deltaTime);
                SerSceneChangeDetect();
                RayCast();
                CalculateHandleableDestination();
                CalTargetToDestination();
            }
            else if (isOwned)
            {
                RayStable();
                HandleAttachStable();
            }
        }

        private void SerSceneChangeDetect()
        {
            if (gameObject.scene != _lastScene)
            {
                _lastScene = gameObject.scene;
                RpcVisible(true);
                SLock(false, transform);
            }
        }

        public void CalculateHandleableDestination()
        {
            if (!IsRayEnable) return;
            _rayHitPoint.position = _rayStart.position + _rayStart.forward * _hitInfo.distance;
        }

        public void CalTargetToDestination()
        {
            if (!_handleable || _eHandState != EHandState.Pinch || !IsRayEnable)
            {
                _handleableAttachingPosition = Vector3.zero;
                return;
            }

            _handleableAttachingPosition = _handleable.transform.TransformPoint(_handleableAttachingLocalPosition);
        }

        public void SetupHandPoseTrigger()
        {
            _ecHandPoseTrigger.PinchStart.AddListener(OnPinchStart);
            _ecHandPoseTrigger.PinchStop.AddListener(OnPinchStop);

            _ecHandPoseTrigger.ExactPinchStart.AddListener(OnExactPinchStart);
            _ecHandPoseTrigger.ExactPinchStop.AddListener(OnExactPinchStop);

            _ecHandPoseTrigger.FistStart.AddListener(OnFistStart);
            _ecHandPoseTrigger.FistStop.AddListener(OnFistStop);
        }

        [Command]
        private void CmdOnFistStop()
        {
            _sIsFist = false;
        }

        [Command]
        private void CmdOnFistStart()
        {
            _sIsFist = true;
        }

        [Command]
        private void CmdOnPinchStop()
        {
            Game.Log($"CmdOnPinchStop: {_sIsPinching}");
            _sIsPinching = false;
            if (!_handleable || _eHandState != EHandState.Pinch) return;

            _eHandState = EHandState.None;
            _handleable.Free();
        }

        [Command]
        private void CmdOnPinchStart()
        {
            Game.Log($"CmdOnPinchStart: {_sIsPinching}");
            _sIsPinching = true;

            if (!_hitTransform || _eHandState != EHandState.None) return;
            if (!_hitTransform.TryGetComponent(out _handleable)) return;
            if (!_handleable.TryStartMove(_rayHitPoint, true)) return;

            SaveAttachingLocalPosition(_rayHitPoint);
            _eHandState = EHandState.Pinch;
        }

        [Command]
        private void CmdOnExactPinchStop()
        {
            Game.Log($"CmdOnExactPinchStop: {_sIsExactPinching}");
            _sIsExactPinching = false;
        }

        [Command]
        private void CmdOnExactPinchStart()
        {
            Game.Log($"CmdOnExactPinchStart: {_sIsExactPinching}");
            _sIsExactPinching = true;
        }

        public void RayCast()
        {
            if (_eHandState != EHandState.None || !IsRayEnable) return;
            gameObject.scene.GetPhysicsScene().Raycast(
                _rayStart.position,
                _rayStart.forward,
                out _hitInfo,
                _maxDistance, _rayLayer);

            _hitTransform = _hitInfo.transform;
            _distanceFromHit = _hitInfo.distance;
        }

        public void RayStable()
        {
            if (!_rayStart || !_rayFollowed || !IsRayEnable) return;

            float intensity = _eHandState == EHandState.Pinch ? _maxStabilizeIntensity : _stabilizeIntensity;

            if (intensity == 0.0f)
            {
                _rayStart.SetPositionAndRotation(_rayFollowed.position, _rayFollowed.rotation);
            }
            else
            {
                _rayStart.position = _rayFollowed.position;
                _rayStart.DORotate(_rayFollowed.rotation.eulerAngles, intensity);
            }
        }

        public void HandleAttachStable()
        {
            if (!_pinchAttach || !_pinchAttachFollowed) return;

            if (_pinchAttachStabilizeIntensity == 0.0f)
            {
                _pinchAttach.SetPositionAndRotation(_pinchAttachFollowed.position, _pinchAttachFollowed.rotation);
            }
            else
            {
                _pinchAttach.DORotateQuaternion(_pinchAttachFollowed.rotation, _pinchAttachStabilizeIntensity);
                _pinchAttach.DOMove(_pinchAttachFollowed.position, _pinchAttachStabilizeIntensity);
            }
        }

        private void OnPinchStart()
        {
            _cIsPinching = true;
            CmdOnPinchStart();
        }

        private void OnPinchStop()
        {
            _cIsPinching = false;
            CmdOnPinchStop();
        }

        private void OnExactPinchStart()
        {
            _cIsExactPinching = true;
            CmdOnExactPinchStart();
        }

        private void OnExactPinchStop()
        {
            _cIsExactPinching = false;
            CmdOnExactPinchStop();
        }

        private void OnFistStart()
        {
            CmdOnFistStart();
        }

        private void OnFistStop()
        {
            CmdOnFistStop();
        }

        private void SaveAttachingLocalPosition(Transform attaching)
        {
            if (!attaching) return;
            _handleableAttachingLocalPosition = _handleable.transform.InverseTransformPoint(attaching.position);
        }

        public bool IsLocked()
        {
            return _eHandState == EHandState.Lock;
        }

        public bool IsLockedBy(Transform lockBy)
        {
            return IsLocked() && lockBy == _lockedBy;
        }

        public void SLock(bool isLock, Transform lockBy)
        {
            if (isLock)
            {
                SLock(lockBy);
            }
            else
            {
                SUnlock(lockBy);
            }
        }

        public bool SLock(Transform lockBy)
        {
            bool isLocked = _eHandState == EHandState.Lock && _lockedBy == lockBy;
            bool lockSuccessful = _eHandState != EHandState.Lock && !_lockedBy;

            if (isLocked || lockSuccessful)
            {
                _lockedBy = lockBy;
                _eHandState = EHandState.Lock;
                return true;
            }

            return false;
        }

        public bool SUnlock(Transform lockBy)
        {
            if (_eHandState == EHandState.Lock && lockBy == _lockedBy)
            {
                _lockedBy = null;
                _eHandState = EHandState.None;
                return true;
            }

            return false;
        }

        public void Visible(bool visible)
        {
            if (!_handMeshRenderer) return;
            _handMeshRenderer.enabled = visible;
        }

        [ClientRpc]
        public void RpcVisible(bool visible)
        {
            Game.Log($"RpcVisible: {visible}");
            Visible(visible);
        }

        private void OnTriggerEnter(Collider other)
        {
            // Game.Log($"OnTriggerEnter : {other.transform}");

            SOnTriggerEnter(other);
        }

        private void SOnTriggerEnter(Collider other)
        {
            if (!isServer) return;
            _bRayInteractEnable = !other.CompareTag("Handleable");
        }

        private WaitForSeconds w = new(2.0f);

        private IEnumerator DebugState()
        {
            while (true)
            {
                XRLogger.Log($"DebugStateHand {_handType}: {_eHandState}");
                yield return w;
            }
        }
    }
}