using System.Collections;
using System.Collections.Generic;
using DemoX.Framework.Bridge.Event;
using DG.Tweening;
using Mirror;
using Unity.XR.PXR;
using UnityEngine;

namespace DemoX.Framework.Level
{
    public class DeskHandle : NetworkBehaviour
    {
        [SerializeField] private ECSWTrigger _ecswTrigger;
        [SerializeField] private List<HandModel> _hands;
        [SerializeField] private float _maxDistance = 0.2f;
        [SerializeField] private float _speed = 0.12f;
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

        private Vector3 _startHandPosition;
        private float _lastMoveDistance;
        private Vector3 _startHandlePosition;
        private float _lastDistance;

        private bool _bWaitForReset;

        private void Update()
        {
            SUpdate();
        }

        private void SUpdate()
        {
            if (!isServer) return;

            if (!_hand) return;

            // Game.Log($"se : {_hand.IsFist} {_eState}");
            if (_hand.IsFist)
            {
                if (_eState == EState.Attached)
                {
                    _startHandPosition = _hand.transform.position;
                    _startHandlePosition = transform.position;
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
            if (Vector3.Distance(_startHandlePosition, transform.position) > _maxDistance)
            {
                _eState = EState.Trigger;
                _ecswTrigger.SWTrigger.Invoke(transform);
                StartCoroutine(SResetDelay());
                return;
            }

            Vector3 toLastHandPosition = _hand.transform.position - _startHandPosition;

            // 是否有移动产生
            float distance = toLastHandPosition.magnitude;
            if (_lastMoveDistance >= distance) return;
            _lastMoveDistance = distance;

            // 移动方向是否正确
            Vector3 direction = toLastHandPosition.normalized;
            float dotVal = Vector3.Dot(direction, transform.forward);
            if (dotVal <= 0.0f) return;

            transform.position += transform.forward * (_speed * Time.deltaTime);
        }

        private IEnumerator SResetDelay()
        {
            _bWaitForReset = true;
            yield return new WaitForSeconds(_resetDelay);
            SReleaseHand();
            transform.DOMove(_startHandlePosition, _resetDuration).onComplete = () => { _bWaitForReset = false; };
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