using DemoX.Framework;
using Mirror;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Runtime
{
    public class XRControllerAgent : BaseNetworkBehaviour, IHandInteractor
    {
        [SerializeField] private HandType m_HandType;
        [SerializeField] private XRBaseController m_Controller;

        private bool m_ClientPinch;
        private bool m_ServerPinch;
        
        protected override void ClientUpdate()
        {
            if (!m_Controller) return;
            // Debug.Log($"GRAB_MENU_TAG: ClientUpdate ===== {m_Controller.selectInteractionState.value} : {m_Controller.selectInteractionState.activatedThisFrame} : {m_Controller.selectInteractionState.active}");

            bool isPinch = m_Controller.selectInteractionState.active;
            if (!m_ClientPinch.Equals(isPinch))
            {
                Debug.Log($"GRAB_MENU_TAG: ClientUpdate ===== {isPinch}");

                m_ClientPinch = isPinch;
                CmdSetPinch(m_ClientPinch);
            }
        }

        [Command(requiresAuthority = false)]
        private void CmdSetPinch(bool isPinch)
        {
            m_ServerPinch = isPinch;
            Debug.Log($"GRAB_MENU_TAG: CmdSetPinch ===== {m_ServerPinch}");

        }

        protected override void ClientLateUpdate()
        {
            if (m_Controller)
            {
                var controllerTransform = m_Controller.transform;
                transform.SetLocalPositionAndRotation(controllerTransform.localPosition, controllerTransform.localRotation);
            }
        }

        public bool IsPinching => m_ServerPinch;
        public bool IsExactPinching => m_ClientPinch;
        public bool IsFist { get; }
        public Transform PinchAttach => transform;
        public Transform HandCenterPoint { get; }
        public bool IsLocked()
        {
            return true;
        }

        public bool IsLockedBy(Transform lockBy)
        {
            return true;
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
        public HandType HandType => m_HandType;
    }
}