using System.Collections.Generic;
using DemoX.Framework.Level;
using DemoX.Framework.Net;
using UnityEngine;

namespace Runtime
{
    public class PlayerPortal : BaseNetworkBehaviour
    {
        private PlayerController m_PlayerController;
        private List<Portal> m_Portals = new();

        private void Awake()
        {
            m_PlayerController = GetComponentInParent<PlayerController>();
        }

        protected override void OnServerTriggerEnter(Collider other)
        {
            if (!m_PlayerController) return;
            if (!other.CompareTag("Portal")) return;
            if (!other.TryGetComponent(out Portal portal)) return;
            if (m_Portals.Contains(portal)) return;
            m_Portals.Add(portal);
            XNetManager.Ins.EnterScene(m_PlayerController.gameObject, portal.Destination);
        }
    }
}