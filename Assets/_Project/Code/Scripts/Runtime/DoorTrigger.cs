using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Runtime
{
    public class DoorTrigger : BaseNetworkBehaviour
    {
        private XRBaseInteractable m_Interactable;
        private IXRSelectInteractor m_Interactor;

        public IXRSelectInteractor Interactor => m_Interactor;

        private void Awake()
        {
            m_Interactable = GetComponent<XRBaseInteractable>();
            if (m_Interactable)
            {
                m_Interactable.firstSelectEntered.AddListener((args) => { m_Interactor = args.interactorObject; });
            }
        }
    }
}