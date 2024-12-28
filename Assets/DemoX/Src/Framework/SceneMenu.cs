using System;
using System.Collections;
using System.Collections.Generic;
using DemoX.Framework.Core;
using DG.Tweening;
using Runtime;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace DemoX.Framework
{
    public class SceneMenu : MonoBehaviour
    {
        [SerializeField] private float _totalAngle = 180.0f;
        [SerializeField] private float _angleForDamping = 20.0f;
        [SerializeField] private int _menuCount = 5;
        [SerializeField] private float _multiple = 1.0f;
        [SerializeField] private float _reviseTime = 0.3f;
        [SerializeField] private PlayerController m_Controller;

        [SerializeField] private List<SceneConfig> _scenes;

        [SerializeField] private Animator _animator;
        [SerializeField] private Transform m_AiUiContainer;

        private static readonly int EnterAnimKey = Animator.StringToHash("Enter");

        private bool _bEnable = true;
        public bool IsEnable => _bEnable;

        private bool _bIsValid;

        public Transform target;
        public Animator planeAnimator;

        private IHandInteractor _trigger;

        private bool _lastTriggerState;
        private Vector3 _lastBaseLine;

        private Tweener _tweener;

        private bool _hasRevised;
        private int _targetSceneIndex;

        private bool _isInit;

        private Collider _triggerCollider;

        [Serializable]
        public class SceneConfig
        {
            [SceneField] public string scene;
        }

        private void Awake()
        {
            _triggerCollider = GetComponent<Collider>();
        }

        void Update()
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                _bEnable = true;
                _bIsValid = true;
                EnterScene(1);
            }
            else if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                _bEnable = true;
                _bIsValid = true;
                EnterScene(2);
            }
            else if (Keyboard.current.digit3Key.wasPressedThisFrame)
            {
                _bEnable = true;
                _bIsValid = true;
                EnterScene(3);
            }
            else if (Keyboard.current.digit4Key.wasPressedThisFrame)
            {
                _bEnable = true;
                _bIsValid = true;
                EnterScene(4);
            }
            else if (Keyboard.current.digit5Key.wasPressedThisFrame)
            {
                _bEnable = true;
                _bIsValid = true;
                EnterScene(5);
            }

            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                SetValid(true);
            }
            else if (Keyboard.current.tKey.wasPressedThisFrame)
            {
                SetValid(false);
            }

            if (CheckScene())
            {
                SetValid(false);
                Enable(false);
            }
            else
            {
                Enable(true);
            }

            Rotate(CalAngle());
            AnimUI();
        }

        private bool CheckScene()
        {
            int activeSceneIndex = SceneManager.sceneCount > 1 ? 1 : 0;

            Scene s = SceneManager.GetSceneAt(activeSceneIndex);
            // Game.Log($"{_initScene} {s.name} {s.path}");

            return string.Equals("Offline", s.name);
        }

        private float CalAngle()
        {
            if (_trigger == null) return 0.0f;
            // Game.Log($"SceneMenuTag CalAngle 00: {_trigger}");

            Transform parent = target.parent;
            Vector3 localPinchPos = parent.InverseTransformPoint(_trigger.PinchAttach.position);
            Vector3 localRight = parent.InverseTransformDirection(target.right);

            if (!_lastTriggerState && _trigger.IsExactPinching)
            {
                // Game.Log($"SceneMenuTag CalAngle 11: {_lastTriggerState} {_trigger.IsExactPinching}");

                _lastBaseLine = Vector3.ProjectOnPlane(localPinchPos - target.localPosition, localRight);
                _lastTriggerState = true;
            }

            // Game.Log($"SceneMenuTag CalAngle 22: {_lastTriggerState} {_trigger.IsExactPinching}");

            float deltaAngle = 0.0f;
            if (_trigger.IsExactPinching)
            {
                // Game.Log($"SceneMenuTag CalAngle 22 00");

                var currentBaseLine = Vector3.ProjectOnPlane(localPinchPos - target.localPosition, localRight);
                deltaAngle = Vector3.SignedAngle(_lastBaseLine, currentBaseLine, localRight);
                _lastBaseLine = currentBaseLine;
                _isInit = true;
            }
            else if (_lastTriggerState)
            {
                // Game.Log($"SceneMenuTag CalAngle 22 00");
                _lastTriggerState = false;
                ReviseRotateResult();
            }

            // Game.Log($"SceneMenuTag CalAngle 33: {deltaAngle}");

            return deltaAngle * _multiple;
        }

        private void Rotate(float deltaAngle)
        {
            float rotateToAngle = deltaAngle;

            if (rotateToAngle != 0)
            {
                if (_tweener != null)
                {
                    _tweener.Kill();
                }

                _hasRevised = false;

                float currentRoundAngle = GetRoundAngle();
                float targetAngle = currentRoundAngle - rotateToAngle;

                // Game.Log($"SceneMenuTag Rotate: {deltaAngle} {currentRoundAngle} {targetAngle} {_totalAngle + _angleForDamping}");

                if (targetAngle > _totalAngle && targetAngle < _totalAngle + _angleForDamping
                    || targetAngle > 360.0f - _angleForDamping)
                {
                    rotateToAngle = 0.0f;
                }

                target.Rotate(target.right, rotateToAngle, Space.World);
            }
        }

        private void AnimUI()
        {
            // 获取目标对象的当前旋转四元数
            Quaternion currentRotation = target.localRotation;
            // 从当前旋转四元数中提取 x 轴旋转角度
            float angle = Quaternion.Angle(currentRotation, Quaternion.identity);
            // 将角度映射到0到1之间
            float progress = Mathf.Clamp01(Mathf.Abs(angle / _totalAngle));

            // Game.Log($"SceneMenuTag UIPresenter: {progress} {angle}");

            AnimUI(progress);
        }

        private void AnimUI(float progress)
        {
            // 获取当前动画状态信息
            AnimatorStateInfo stateInfo = planeAnimator.GetCurrentAnimatorStateInfo(0);

            // 检查是否在播放指定动画
            if (stateInfo.IsName("AnimationParameter"))
            {
                // 播放动画，并设置进度
                planeAnimator.Play(stateInfo.fullPathHash, 0, progress); // 保持使用 progress
            }
        }

        private float GetRoundAngle()
        {
            float angle = GetSignAngle();
            return angle >= 0 ? angle : 360 + angle;
        }

        private float GetSignAngle()
        {
            Vector3 localUp = target.parent.InverseTransformDirection(target.transform.up);
            return Vector3.SignedAngle(localUp, Vector3.up, Vector3.right);
        }

        private void ReviseRotateResult()
        {
            // Game.Log($"ReviseRotateResult 000");
            // if (!enabled || !_bIsValid) return;
            if (_tweener != null && _tweener.IsPlaying() || _hasRevised) return;

            int maxIndex = _menuCount - 1;
            float angleStep = _totalAngle / maxIndex;

            float currentRoundAngle = GetRoundAngle();
            if (currentRoundAngle > _totalAngle && currentRoundAngle < _totalAngle + _angleForDamping)
            {
                _targetSceneIndex = maxIndex;
            }
            else if (currentRoundAngle > 360.0f - _angleForDamping)
            {
                _targetSceneIndex = 0;
            }
            else
            {
                _targetSceneIndex = Mathf.RoundToInt(currentRoundAngle / angleStep);
                _targetSceneIndex = Mathf.Abs(_targetSceneIndex);
            }

            _targetSceneIndex = Mathf.Clamp(_targetSceneIndex, 0, maxIndex);

            float targetAngle = _targetSceneIndex * -angleStep;
            _tweener = target.DOLocalRotate(new Vector3(targetAngle - 0.1f, 0.0f, 0.0f), _reviseTime);
            _hasRevised = true;

            // Game.Log($"SceneMenuTag ReviseRotateResult: {currentRoundAngle} {_targetSceneIndex} {angleStep} {targetAngle}");
        }

        private Coroutine _waitToScene;

        public IEnumerator EnterScene()
        {
            // Game.Log($"ReviseRotateResult 111");

            yield return new WaitForSeconds(2.0f);
            Debug.Log($"SceneMenuTag: EnterScene {_targetSceneIndex}");
            if (_targetSceneIndex >= 0 && _targetSceneIndex < _scenes.Count)
            {
                // XNetManager.Ins.EnterScene(_scenes[_targetSceneIndex].scene);
                // triggerEvent.Invoke(_scenes[_targetSceneIndex].scene);
                EnterScene(_scenes[_targetSceneIndex].scene);
            }
        }

        public void EnterScene(int index)
        {
            Debug.Log($"SceneMenuTag: {_bEnable}  {index} {_targetSceneIndex} {Mathf.Abs(index - _menuCount + 1)}");
            if (!_bEnable || !_bIsValid) return;

            if (index == -1 || index == 0)
            {
                // triggerEvent.Invoke("reset");
                EnterScene("reset");
            }
            else if (Mathf.Abs(index - _menuCount + 1) == _targetSceneIndex)
            {
                // XNetManager.Ins.EnterScene(_scenes[_targetSceneIndex].scene);
                Debug.Log($"SceneMenuTag EnterScene: {_scenes[index].scene}");

                // triggerEvent.Invoke(_scenes[index].scene);
                EnterScene(_scenes[index].scene);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"SceneMenuTag OnTriggerEnter: {other.transform}");

            if (!enabled || !_bIsValid || !other.CompareTag("HandSpace")) return;

            IHandInteractor handRayHandInteractor = other.transform.GetComponent<IHandInteractor>();

            if (handRayHandInteractor is not { HandType: HandType.HandRight }) return;

            // Game.Log($"SceneMenuTag OnTriggerEnter 00: {other.transform} {handRayHandInteractor.HandCenterPoint} {handRayHandInteractor.IsLocked()}");

            if (!handRayHandInteractor.SLock(transform)) return;

            // Game.Log($"SceneMenuTag OnTriggerEnter 11: {other.transform} {handRayHandInteractor.HandCenterPoint}");
            _trigger = handRayHandInteractor;
        }

        private void OnTriggerExit(Collider other)
        {
            Debug.Log($"SceneMenuTag OnTriggerExit 00: {other.transform}");

            if (_trigger != null
                && _trigger.PinchAttach == other.transform
                && _trigger.IsLockedBy(transform))
            {
                Debug.Log($"SceneMenuTag OnTriggerExit 11: {other.transform} {_trigger.HandCenterPoint}");
                ReviseRotateResult();
                _trigger.SUnlock(transform);
                _trigger = null;
                _lastTriggerState = false;
            }
        }

        public void Enable(bool enable)
        {
            _bEnable = enable;
            if (!_bEnable)
            {
                ReleaseTrigger();
            }
        }

        public void SetValid(bool valid)
        {
            if (_animator)
            {
                if (m_AiUiContainer)
                {
                    m_AiUiContainer.gameObject.SetActive(!valid);
                }
                _bIsValid = valid;
                _animator.SetBool(EnterAnimKey, _bIsValid && _bEnable);
            }

            if (!valid)
            {
                ReleaseTrigger();
            }

            if (_triggerCollider)
            {
                _triggerCollider.enabled = _bIsValid && _bEnable;
            }

            Debug.Log($"SceneMenuTag SetValid: {valid} {_bEnable} {_triggerCollider} {_triggerCollider.enabled}");
        }

        private void ReleaseTrigger()
        {
            if (_trigger != null && _trigger.IsLockedBy(transform))
            {
                Debug.Log($"SceneMenuTag ReleaseTrigger");
                ReviseRotateResult();
                _trigger.SUnlock(transform);
                _trigger = null;
                _lastTriggerState = false;
            }
        }

        public void EnterScene(string sceneName)
        {
            Debug.Log($"EnterScene: {sceneName}");
            if (m_Controller)
            {
                m_Controller.CmdEnterScene(sceneName);
            }
        }
    }
}