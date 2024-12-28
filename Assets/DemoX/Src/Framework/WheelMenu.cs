using System;
using System.Collections;
using System.Collections.Generic;
using DemoX.Framework.Core;
using DG.Tweening;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace DemoX.Framework
{
    public class WheelMenu : MonoBehaviour
    {
        [SerializeField] private Transform _menusContainer;
        [SerializeField] private List<Menu> _menus;
        [SerializeField] private float _radius;
        [SerializeField] private float _activatedRadius;
        [SerializeField] private float _activatedAnimDuration = 0.3f;
        [SerializeField] private float _dotValue = 0.5f;
        [SerializeField] private float _rotateToTargetDuration = 0.3f;
        [SerializeField] private float _rotateIntensityMultiple = 1.5f;
        [SerializeField] private float _triggerDelay = 2.0f;

        public UnityEvent<string> triggerEvent;

        private IHandInteractor _trigger;
        private Vector3 _lastVecToTrigger;

        private Vector3 _localBaseLine;
        private Camera _camera;
        private Menu _targetMenu;

        private bool _lastPinchState;

        private WaitForSeconds _activateAnimInternal = new(0.1f);

        private Coroutine _activatedAnimCoroutine;
        private Coroutine _delayToSceneCoroutine;

        private bool _bValid;
        public bool IsValid => _bValid;

        private Vector3 _staticPosition;
        private Vector3 _staticQ;

        [Serializable]
        public class Menu
        {
            public Transform MenuTrsf;
            [SceneField] public string SceneName;
        }

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            if (_menus.Count == 0) return;
            float angle = 360.0f / _menus.Count;

            for (var i = 0; i < _menus.Count; i++)
            {
                Transform menu = _menus[i].MenuTrsf;
                // menu.SetParent(transform);
                menu.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                menu.Rotate(transform.right, angle * i, Space.World);
                menu.position += menu.forward * _radius;
            }
        }

        void Start()
        {
            SetupCamera();
            SetupBaseLine();
            Check();
            RotateToTarget(false);
        }

        private void SetupCamera()
        {
            if (!_camera)
            {
                _camera = Camera.main;
            }
        }

        private void SetupBaseLine()
        {
            if (_camera)
            {
                _localBaseLine = Vector3.Normalize(_camera.transform.position - transform.position);
                _localBaseLine = Vector3.ProjectOnPlane(_localBaseLine, transform.right).normalized;
                _localBaseLine = transform.InverseTransformDirection(_localBaseLine);
            }
        }

        private void Check()
        {
            for (var i = 0; i < _menus.Count; i++)
            {
                Transform menu = _menus[i].MenuTrsf;
                float value = Vector3.Dot(menu.forward, GetWorldBaseLine());
                if (value > _dotValue)
                {
                    _targetMenu = _menus[i];
                }
            }
        }

        private void RotateToTarget(bool triggerEnable = true)
        {
            if (_targetMenu != null)
            {
                Transform menu = _targetMenu.MenuTrsf;
                float rotateAngle = Vector3.SignedAngle(menu.forward, GetWorldBaseLine(), transform.right);
                Quaternion q = transform.rotation * Quaternion.AngleAxis(rotateAngle, transform.right);
                if (triggerEnable)
                {
                    transform.DORotateQuaternion(q, _rotateToTargetDuration);
                    StartToScene();
                }
                else
                {
                    transform.rotation = q;
                }
            }
        }

        private void StartToScene()
        {
            if (_delayToSceneCoroutine != null)
            {
                StopCoroutine(_delayToSceneCoroutine);
            }

            _delayToSceneCoroutine = StartCoroutine(DelayToScene());
        }

        private void Update()
        {
            SetupCamera();
            if (_trigger == null) return;

            if (_lastPinchState && !_trigger.IsPinching)
            {
                RotateToTarget();
            }
            else if (!_lastPinchState && _trigger.IsPinching)
            {
                _lastVecToTrigger = Vector3.Normalize(transform.position - _trigger.PinchAttach.position);
                _lastVecToTrigger = Vector3.ProjectOnPlane(_lastVecToTrigger, transform.right).normalized;
            }

            // Game.Log($"wheel: {_trigger.IsPinching} {_lastPinchState}");

            _lastPinchState = _trigger.IsPinching;

            if (!_lastPinchState) return;

            Vector3 vecToTrigger = Vector3.Normalize(transform.position - _trigger.PinchAttach.position);
            vecToTrigger = Vector3.ProjectOnPlane(vecToTrigger, transform.right).normalized;

            float angle = Vector3.SignedAngle(_lastVecToTrigger, vecToTrigger, transform.right) *
                          _rotateIntensityMultiple;
            _lastVecToTrigger = vecToTrigger;


            transform.Rotate(transform.right, angle, Space.World);

            Check();
        }

        private void OnTriggerEnter(Collider other)
        {
            Game.Log($"on trigger enter: {other.transform}");

            if (!other.CompareTag("HandSpace")) return;

            IHandInteractor handRayHandInteractor = other.transform.GetComponent<IHandInteractor>();

            if (handRayHandInteractor == null
                || handRayHandInteractor.IsPinching
                || !handRayHandInteractor.SLock(transform)) return;

            Game.Log($"on trigger enter 00: {other.transform} {handRayHandInteractor.HandCenterPoint}");

            transform.DOKill();
            _trigger = handRayHandInteractor;
            SetupBaseLine();

            StartActivateMenuAnim(true);
        }

        private void OnTriggerExit(Collider other)
        {
            Game.Log($"on trigger Exit: {other.transform}");

            if (_trigger != null && _trigger.HandCenterPoint == other.transform)
            {
                Game.Log($"on trigger Exit 00: {other.transform} {_trigger.HandCenterPoint}");
                _trigger.SUnlock(transform);
                _trigger = null;

                RotateToTarget();
                StartActivateMenuAnim(false);
            }
        }

        private IEnumerator ActivateMenus(bool activated)
        {
            foreach (var menu in _menus)
            {
                Transform menuTrsf = menu.MenuTrsf;
                float targetRadius = activated ? _activatedRadius : _radius;

                Vector3 forward = menuTrsf.forward * targetRadius + transform.position;
                menuTrsf.DOLocalMove(menuTrsf.parent.InverseTransformPoint(forward), _activatedAnimDuration);
                yield return _activateAnimInternal;
            }
        }

        private void StartActivateMenuAnim(bool activated)
        {
            if (_activatedAnimCoroutine != null)
            {
                StopCoroutine(_activatedAnimCoroutine);
            }

            _activatedAnimCoroutine = StartCoroutine(ActivateMenus(activated));
        }

        private IEnumerator DelayToScene()
        {
            if (_targetMenu == null) yield break;
            yield return new WaitForSeconds(_triggerDelay);

            Game.Log($"DelayToScene: {_targetMenu.SceneName}");
            triggerEvent.Invoke(_targetMenu.SceneName);
        }

        public void SetValid(bool valid, Vector3 anchorPoint)
        {
            _menusContainer.gameObject.SetActive(valid);
            if (!_bValid && valid)
            {
                if (_trigger != null)
                {
                    _trigger.SUnlock(transform);
                    _trigger = null;
                }
            }

            _bValid = valid;
        }

        private Vector3 GetWorldBaseLine()
        {
            return transform.TransformDirection(_localBaseLine);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + _localBaseLine * 3);
        }
    }
}