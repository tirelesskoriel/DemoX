using UnityEngine.XR.Interaction.Toolkit;

namespace Runtime
{
    public class AuthGrabInteractable : XRGrabInteractable
    {
        private InteractableAuthSwitcher m_AuthSwitcher;
        protected override void Awake()
        {
            m_AuthSwitcher = GetComponent<InteractableAuthSwitcher>();
            base.Awake();
        }

        protected override void OnSelectEntering(SelectEnterEventArgs args)
        {
            if (m_AuthSwitcher)
            {
                m_AuthSwitcher.OnSelectEntered(args);
            }
            base.OnSelectEntering(args);
        }
    }
}