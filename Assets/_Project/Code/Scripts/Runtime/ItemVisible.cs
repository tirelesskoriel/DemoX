using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Runtime
{
    public class ItemVisible : BaseNetworkBehaviour
    {
        [SerializeField] private List<Renderer> m_Renderers;
        private XRGrabInteractable m_GrabInteractable;

        private void Awake()
        {
            m_GrabInteractable = GetComponent<XRGrabInteractable>();
            m_GrabInteractable.selectEntered.AddListener(OnSelectEntered);

            foreach (var r in m_Renderers)
            {
                r.enabled = false;
            }
        }

        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            Show();
            CmdShow();
        }

        private void Show()
        {
            foreach (var r in m_Renderers)
            {
                r.enabled = true;
            }
        }

        [Command(requiresAuthority = false)]
        private void CmdShow()
        {
            RpcShow();
        }

        [ClientRpc(includeOwner = false)]
        private void RpcShow()
        {
            Show();
        }
    }
}