using System;
using DemoX.Framework;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Runtime
{
    public class NewHandInteractor : MonoBehaviour, IHandInteractor
    {
        private XRBaseController m_Controller;

        private void Awake()
        {
            m_Controller = GetComponent<XRBaseController>();
        }

        public bool IsPinching => m_Controller && m_Controller.selectInteractionState.active;
        public bool IsExactPinching { get; }
        public bool IsFist { get; }
        public Transform PinchAttach => transform;
        public Transform HandCenterPoint { get; }

        public bool IsLocked()
        {
            return false;
        }

        public bool IsLockedBy(Transform lockBy)
        {
            return false;
        }

        public bool SLock(Transform lockBy)
        {
            return true;
        }

        public bool SUnlock(Transform lockBy)
        {
            return true;
        }

        public void Visible(bool visible)
        {
            
        }

        public Vector3 Vel { get; }
        public HandType HandType { get; }
    }
}