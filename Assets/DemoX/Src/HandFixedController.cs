using System;
using DemoX.Framework;
using DemoX.Framework.Core;
using Mirror;
using Newtonsoft.Json;
using Unity.XR.PXR;
using UnityEngine;

namespace DemoX
{
    public class HandFixedController : NetworkBehaviour
    {
        [SerializeField] private HandType _handType;

        [SerializeField] private Transform _handRoot;
        [SerializeField] private Transform _arm;
        [SerializeField] private Transform _armParent;

        private BodyController _bodyController;

        public BodyController BodyController
        {
            set => _bodyController = value;
        }

        private void Update()
        {
            ClientArmIK();
        }

        public void ClientArmIK()
        {
            if (isServer || !_bodyController) return;
            // arm to elbow
            _bodyController.GetIkPoints(_handType, out Transform elbow);
            _armParent.LookAt(elbow);
            _arm.transform.localEulerAngles = new Vector3(0, -180, _armParent.parent.localEulerAngles.z);
        }
    }
}