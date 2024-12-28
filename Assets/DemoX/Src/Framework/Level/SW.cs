using System.Collections;
using System.Collections.Generic;
using DemoX.Framework.Bridge.Event;
using DG.Tweening;
using Mirror;
using Unity.XR.PXR;
using UnityEngine;

namespace DemoX.Framework.Level
{
    public class SW : NetworkBehaviour
    {
        [SerializeField] private ECSWTrigger _ecswTrigger;
        [SerializeField] private List<HandModel> _hands;
        [SerializeField] private Transform _handle;
        [SerializeField] private float _rotateSpeed = -80.0f;
        [SerializeField] private Vector2 _rotateRange = new(-45.0f, 0.0f);
        [SerializeField] private float _resetDuration = 2.0f;
        [SerializeField] private float _resetDelay = 1.0f;

        private Vector3 _startAxis;

        private HandSeparateHandInteractor _hand;

        public enum EState
        {
            Free,
            Attached,
            Rotate,
            Trigger
        }

        private EState _eState;
        private bool _isHandOut;

        private Vector3 _startHandleAxis;
        private Quaternion _startRotation;

        private Vector3 _startHandPosition;
        private float _lastDistance;

        private bool _bWaitForReset;


        private void Awake()
        {
            _startHandleAxis = (_handle.position - transform.position).normalized;
            _startRotation = transform.rotation;
        }

        private void Update()
        {
            if (!isServer) return;

            if (!_hand) return;

            // Game.Log($"se : {_hand.IsFist} {_eState}");
            if (_hand.IsFist)
            {
                if (_eState == EState.Attached)
                {
                    _startHandPosition = _hand.transform.position;
                    _eState = EState.Rotate;
                }
                else if (_eState == EState.Rotate)
                {
                    SRotateHandle();
                }
            }
            else if (_eState is EState.Rotate or EState.Trigger)
            {
                SReleaseHand();
            }
        }

        private void SRotateHandle()
        {
            var handPosition = _hand.transform.position;
            Vector3 axis = handPosition - transform.position;

            float distance = Vector3.Distance(handPosition, _startHandPosition);
            float vel = (distance - _lastDistance) / Time.deltaTime;
            float dot = Vector3.Dot(axis.normalized, transform.forward);
            var shouldRotate = dot < 0.0f && vel > 0.1f;
            _lastDistance = distance;

            // XRLogger.Ins.Log($"dis = {dot} {vel}");

            Quaternion rotation = transform.rotation;
            if (shouldRotate)
            {
                transform.RotateAround(transform.position, transform.right, _rotateSpeed * Time.deltaTime);
            }

            Vector3 handleAxis = (_handle.position - transform.position).normalized;
            float angle = Vector3.SignedAngle(_startHandleAxis, handleAxis, transform.right);
            if (angle < _rotateRange.x || angle > _rotateRange.y)
            {
                transform.rotation = rotation;
                if (_eState == EState.Rotate)
                {
                    _eState = EState.Trigger;
                    _ecswTrigger.SWTrigger.Invoke(transform);
                    StartCoroutine(SResetDelay());
                }
            }
        }

        private IEnumerator SResetDelay()
        {
            _bWaitForReset = true;
            yield return new WaitForSeconds(_resetDelay);
            SReleaseHand();
            transform.DORotateQuaternion(_startRotation, _resetDuration).onComplete = () => { _bWaitForReset = false; };
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isServer) return;
            if (!other.CompareTag("HandSpace") || _bWaitForReset) return;
            if (!other.transform.TryGetComponent(out HandSeparateHandInteractor hand)) return;
            if (hand.IsFist) return;


            _hand = hand;
            _hand.Hand.SLock(true, transform);
            _hand.Hand.RpcVisible(false);
            _eState = EState.Attached;

            Attach(hand.HandType, _hand.Hand.HandController, true);
        }

        private void OnDestroy()
        {
            transform.DOKill();
            SReleaseHand();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!isServer) return;
            if (!other.CompareTag("HandSpace")) return;

            bool bIsHandOut = _hand != null && other.transform == _hand.transform;
            if (bIsHandOut && _eState != EState.Rotate)
            {
                SReleaseHand();
            }
        }

        private void SReleaseHand()
        {
            if (!_hand) return;

            Attach(_hand.HandType, _hand.Hand.HandController, false);
            _hand.Hand.SLock(false, transform);
            _hand.Hand.RpcVisible(true);
            _hand = null;
            _eState = EState.Free;
        }

        [ClientRpc]
        private void Attach(HandType handType, HandController handController, bool attach)
        {
            foreach (var handModel in _hands)
            {
                if (handModel.HandType == handType)
                {
                    handModel.HandRenderer.enabled = attach;
                    if (attach)
                    {
                        handModel.HandFixedController.BodyController = handController.BodyController;
                    }
                    else
                    {
                        handModel.HandFixedController.BodyController = null;
                    }

                    break;
                }
            }
        }
    }
}