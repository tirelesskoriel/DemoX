using System;
using System.Collections.Generic;
using DemoX.Framework.Level;
using DemoX.Framework.Net;
using DG.Tweening;
using Mirror;
using RootMotion.FinalIK;
using Unity.XR.PXR;
using UnityEngine;

namespace DemoX.Framework
{
    public class BodyController : NetworkBehaviour
    {
        [SerializeField] private VRIK _vrik;
        [SerializeField] private BodyVelocityTrack _bodyVelocityTrack;
        [SerializeField] private Animator _bodyAnimator;

        [SerializeField] private Transform _leftElbow;
        [SerializeField] private Transform _rightElbow;

        [SerializeField] private Transform _leftHandLocalPoint;
        [SerializeField] private Transform _rightHandLocalPoint;

        [SerializeField] private List<HandAnchor> _handAnchors;

        [Header("WheelMenu")] [SerializeField] private Transform _wheelMenuFollowed;
        [SerializeField] private Transform _wheelMenuContainer;
        [SerializeField] private WheelMenu _wheelMenu;
        [SerializeField] private SceneMenu _sceneMenu;
        public SceneMenu SceneMenu => _sceneMenu;

        [SerializeField] private float _stabilizeIntensity;
        public WheelMenu WheelMenu => _wheelMenu;

        private Vector3 _lastPosition;
        private static readonly int SpeedAnimKey = Animator.StringToHash("Speed");

        [Serializable]
        public class HandAnchor
        {
            public HandType handType;
            public Transform handRoot;
            public Transform arm;
            public Transform armParent;
        }

        private void Update()
        {
            if (isServer) return;

            WheelMenuStable();

            if (_bodyVelocityTrack && _bodyAnimator)
            {
                _bodyAnimator.SetFloat(SpeedAnimKey, _bodyVelocityTrack.MotionVel.magnitude);
            }
        }

        public void ClientSetHandCtrl(HandController handController)
        {
            if (!_vrik || isServer) return;
            if (handController.HandType == HandType.HandLeft)
            {
                _vrik.solver.leftArm.target = handController.HandRoot;
            }
            else
            {
                _vrik.solver.rightArm.target = handController.HandRoot;
            }
        }

        public bool ClientIsHandCtrlSet()
        {
            return _vrik && _vrik.solver.rightArm.target && _vrik.solver.leftArm.target;
        }

        public void GetIkPoints(HandType handType, out Transform elbow)
        {
            elbow = handType == HandType.HandRight ? _rightElbow : _leftElbow;
        }

        public void GetHandPoint(HandType handType, out Transform handPoint)
        {
            handPoint = handType == HandType.HandRight ? _rightHandLocalPoint : _leftHandLocalPoint;
        }

        public void GetHandPoints(HandType handType, out Transform handRoot, out Transform armParent, out Transform arm)
        {
            handRoot = null;
            armParent = null;
            arm = null;

            foreach (var handAnchor in _handAnchors)
            {
                if (handAnchor.handType != handType) continue;
                handRoot = handAnchor.handRoot;
                armParent = handAnchor.armParent;
                arm = handAnchor.arm;
            }
        }

        private void WheelMenuStable()
        {
            if (!_sceneMenu || !_sceneMenu.IsEnable) return;

            if (_stabilizeIntensity != 0)
            {
                _wheelMenuContainer.DOMove(_wheelMenuFollowed.position, _stabilizeIntensity);
                _wheelMenuContainer.DORotateQuaternion(_wheelMenuFollowed.rotation, _stabilizeIntensity);
            }
            else
            {
                _wheelMenuContainer.SetPositionAndRotation(_wheelMenuFollowed.position, _wheelMenuFollowed.rotation);
            }
        }
        
        
    }
}