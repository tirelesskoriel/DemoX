using System.Collections.Generic;
using DemoX.Framework.Core;
using DemoX.Framework.Level;
using DemoX.Framework.Net;
using DG.Tweening;
using Mirror;
using Unity.XR.PXR;
using UnityEngine;

namespace DemoX.Framework
{
    public class HandController : NetworkBehaviour
    {
        [SerializeField] private HandType _handType;

        [SerializeField] private Transform _handRoot;
        [SerializeField] private Transform _arm;
        [SerializeField] private Transform _armParent;
        [SerializeField] private UIHandController _uiHandController;
        [SerializeField] private bool _handAnchorEnable = true;

        [HideInInspector] [SyncVar(hook = nameof(OnBodyCtrlChange))]
        public BodyController BodyController;

        public Transform HandRoot => _handRoot;

        public HandType HandType => _handType;

        private Vector3 _directionArm;

        private float _lostTrackFrameCounter;

        private Vector3 _positionInBody;

        private List<Portal> _portals = new();

        public void SerSetBodyCtrl(BodyController bodyController)
        {
            BodyController = bodyController;
        }

        public void OnBodyCtrlChange(BodyController _, BodyController newVal)
        {
            if (_uiHandController)
            {
                Game.Log($"OnBodyCtrlChange :  {newVal}  {newVal.WheelMenu}  ");
                // _uiHandController.WheelMenu = newVal.WheelMenu;
                _uiHandController.SceneMenu = newVal.SceneMenu;
            }
        }

        private void Update()
        {
            ClientOwnedArmIK();
            ClientSetHandCtrlToBodyCtrl();
        }

        private void ClientSetHandCtrlToBodyCtrl()
        {
            if (isServer || !BodyController) return;
            if (BodyController.ClientIsHandCtrlSet()) return;
            BodyController.ClientSetHandCtrl(this);
        }

        public void ClientOwnedHandAnchor(bool isTracked)
        {
            // return;
            if (isServer || !isOwned || !_handAnchorEnable) return;

            if (isTracked)
            {
                _handRoot.DOKill();
                _lostTrackFrameCounter = 0;
            }
            else
            {
                if (_lostTrackFrameCounter == 0.0f)
                {
                    BodyController.GetHandPoint(_handType, out Transform handPoint);
                    handPoint.SetPositionAndRotation(_handRoot.position, _handRoot.rotation);
                }
                else if (_lostTrackFrameCounter is < 0.5f or > 1.6f)
                {
                    BodyController.GetHandPoint(_handType, out Transform handPoint);
                    _handRoot.SetPositionAndRotation(handPoint.position, handPoint.rotation);
                }
                else if (_lostTrackFrameCounter < 1.0f)
                {
                    BodyController.GetHandPoints(_handType, out Transform handRoot, out _, out _);
                    _handRoot.DOMove(handRoot.position, 0.5f);
                    _handRoot.DORotateQuaternion(handRoot.rotation, 0.5f).OnComplete(() =>
                    {
                        BodyController.GetHandPoint(_handType, out Transform handPoint);
                        handPoint.SetPositionAndRotation(_handRoot.position, _handRoot.rotation);
                    });
                }

                _lostTrackFrameCounter += Time.deltaTime;
            }
        }

        private void ClientOwnedArmIK()
        {
            if (isServer || !isOwned) return;

            // arm to elbow
            BodyController.GetIkPoints(_handType, out Transform elbow);
            _armParent.LookAt(elbow);
            _arm.transform.localEulerAngles = new Vector3(0, -180, _armParent.parent.localEulerAngles.z);
        }

        private void OnDrawGizmos()
        {
            if (_handRoot)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(_handRoot.position, _handRoot.position + _directionArm);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            SerOnTriggerEnter(other);
        }

        private void SerOnTriggerEnter(Collider other)
        {
            if (!isServer) return;
            if (!other.CompareTag("Portal")) return;
            if (!other.TryGetComponent(out Portal portal)) return;
            if (_portals.Contains(portal)) return;
            _portals.Add(portal);
            XNetManager.Ins.EnterScene(BodyController.gameObject, portal.Destination);

            // portal.Trigger.gameObject.SetActive(false);
            // portal.gameObject.SetActive(false);
        }
    }
}