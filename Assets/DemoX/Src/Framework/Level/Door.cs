using System;
using System.Collections.Generic;
using DG.Tweening;
using Mirror;
using Unity.XR.PXR;
using UnityEngine;

namespace DemoX.Framework.Level
{
    public class Door : NetworkBehaviour
    {
        [SerializeField] private Transform _door;
        [SerializeField] private List<HandModel> _hands;
        [SerializeField] private List<DoorConfig> _doorSheets;
        [SerializeField] private float _openSpeed = 20.0f;
        [SerializeField] private float _minAngle = 50.0f;
        [SerializeField] private float _resetDuration = 5.0f;
        [SerializeField] private float _resetDelay = 3.0f;

        private IHandInteractor _rHand;
        private IHandInteractor _lHand;

        private Vector3 _rLastHandPosition;
        private Vector3 _rStartHandPosition;

        private Vector3 _handleStartAxis;
        private bool _bWaitForReset;

        public enum Side
        {
            Left,
            Right
        }

        [Serializable]
        public class DoorConfig
        {
            public Transform Door;
            public Transform Handle;
            public Transform FinalRotation;
            public Side Side;
            [HideInInspector] public Vector3 HandleStartAxis;
            [HideInInspector] public Quaternion StartRotation;
        }

        private void Awake()
        {
            foreach (var doorConfig in _doorSheets)
            {
                doorConfig.HandleStartAxis = Vector3.Normalize(doorConfig.Handle.position - doorConfig.Door.position);
                doorConfig.StartRotation = doorConfig.Door.rotation;
            }
        }

        private void Update()
        {
            if (!isServer) return;
            if (_rHand == null || _lHand == null || _bWaitForReset) return;

            Vector3 rHandPosition = _rHand.PinchAttach.position;
            float currentDistance = Vector3.Distance(rHandPosition, _rStartHandPosition);
            float lastDistance = Vector3.Distance(_rLastHandPosition, _rStartHandPosition);
            _rLastHandPosition = rHandPosition;

            bool bIsFurther = currentDistance > lastDistance;

            Vector3 moveDirection = Vector3.Normalize(rHandPosition - _rStartHandPosition);
            float moveDirectionSign = Vector3.Dot(_door.transform.forward, moveDirection);

            if (!bIsFurther || moveDirectionSign <= 0) return;

            float maxAngle = 0.0f;
            foreach (var doorSheet in _doorSheets)
            {
                float sign = doorSheet.Side == Side.Left ? -1 : 1;
                doorSheet.Door.RotateAround(doorSheet.Door.position, doorSheet.Door.up,
                    sign * _openSpeed * Time.deltaTime);

                Vector3 currentHandleAxis = Vector3.Normalize(doorSheet.Handle.position - doorSheet.Door.position);
                float angle = Vector3.Angle(doorSheet.HandleStartAxis, currentHandleAxis);
                maxAngle = Mathf.Max(angle, maxAngle);
            }

            if (maxAngle >= _minAngle)
            {
                _lHand = null;
                _rHand = null;
                // SReleaseHand(ref _lHand);
                // SReleaseHand(ref _rHand);
                ResetDelay();
            }
        }

        private void ResetDelay()
        {
            _bWaitForReset = true;
            // yield return new WaitForSeconds(_resetDelay);
            foreach (var doorConfig in _doorSheets)
            {
                doorConfig.Door.DORotateQuaternion(doorConfig.FinalRotation.rotation, _resetDuration).onComplete = () =>
                {
                    _bWaitForReset = false;
                };
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isServer) return;
            if (_bWaitForReset) return;

            if (_rHand != null && _lHand != null) return;

            if (!other.CompareTag("HandSpace") || !other.TryGetComponent(out IHandInteractor hand)) return;
            switch (hand.HandType)
            {
                case HandType.HandLeft:
                    _lHand = hand;
                    // SSetupHand(ref _lHand, hand);
                    break;
                case HandType.HandRight:
                    _rStartHandPosition = hand.PinchAttach.position;
                    _rHand = hand;
                    // SSetupHand(ref _rHand, hand);
                    break;
            }
        }

        private void SSetupHand(ref HandSeparateHandInteractor refhand, IHandInteractor hand)
        {
            // if (!hand) return;
            // refhand = hand;
            // SVisiblePlayerHandModel(hand, false);
            // Attach(hand.HandType, hand.Hand.HandController, true);
        }

        private void SVisiblePlayerHandModel(HandSeparateHandInteractor hand, bool visible)
        {
            if (!hand) return;
            hand.Hand.SLock(!visible, transform);
            hand.Hand.RpcVisible(visible);
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
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!isServer) return;

            if (!other.CompareTag("HandSpace")) return;
            if (_rHand != null && other.transform == _rHand.PinchAttach)
            {
                _rHand = null;
                // SReleaseHand(ref _rHand);
            }
            else if (_lHand != null && other.transform == _lHand.PinchAttach)
            {
                _lHand = null;
                // SReleaseHand(ref _lHand);
            }
        }

        private void SReleaseHand(ref HandSeparateHandInteractor hand)
        {
            if (!hand) return;
            // SVisiblePlayerHandModel(hand, true);
            // Attach(hand.HandType, hand.Hand.HandController, false);
            hand = null;
        }
    }
}