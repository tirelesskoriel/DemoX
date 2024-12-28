using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace Runtime
{
    public class InteractableAuthSwitcher : BaseNetworkBehaviour
    {
        // [SerializeField] private ObjectManipulator m_Interactable;
        [SerializeField] private Rigidbody m_Rb;
        [SerializeField] private XRGrabInteractable m_Interactable;

        private IXRSelectInteractor m_Interactor;
        private FollowTo m_FollowTo;

        [SyncVar(hook = nameof(OnOwnerChange))]
        private uint m_Owner;

        private void Awake()
        {
            m_FollowTo = GetComponent<FollowTo>();
            if (m_Interactable)
            {
                // m_Interactable.movementType = XRBaseInteractable.MovementType.VelocityTracking;
                // m_Interactable.firstSelectEntered.AddListener(OnSelectEntered);
                // m_Interactable.lastSelectExited.AddListener(OnSelectExited);
            }
        }

        protected override void ClientUpdate()
        {
            if (Keyboard.current.gKey.wasPressedThisFrame)
            {
                if (NetworkClient.localPlayer.TryGetComponent(out PlayerController netPlayer))
                {
                    if (m_Rb)
                    {
                        m_Rb.isKinematic = false;
                        m_Interactable.movementType = XRBaseInteractable.MovementType.VelocityTracking;
                    }

                    CmdChangeAuth(netPlayer);
                }
            }
        }

        public void OnSelectEntered(SelectEnterEventArgs args)
        {
            if (m_FollowTo)
            {
                m_FollowTo.enabled = false;
            }
            
            m_Interactor = args.interactorObject;
            if (m_Interactor == null) return;
            PlayerController netPlayer = m_Interactor.transform.GetComponentInParent<PlayerController>();
            if (m_Rb)
            {
                m_Rb.isKinematic = false;
                m_Interactable.movementType = XRBaseInteractable.MovementType.VelocityTracking;
            }

            CmdChangeAuth(netPlayer);
        }

        private void OnSelectExited(SelectExitEventArgs args)
        {

        }

        [Command(requiresAuthority = false)]
        private void CmdChangeAuth(PlayerController netPlayer)
        {

            if (netPlayer)
            {
                if (connectionToClient != null)
                {
                    if (netPlayer.connectionToClient != connectionToClient)
                    {
                        netIdentity.RemoveClientAuthority();
                        netIdentity.AssignClientAuthority(netPlayer.connectionToClient);
                    }
                }
                else
                {
                    netIdentity.AssignClientAuthority(netPlayer.connectionToClient);
                }
            }
            else
            {
                netIdentity.RemoveClientAuthority();
            }
            m_Owner = netPlayer ? netPlayer.Identity.netId : uint.MaxValue;

        }

        private void ChangeAuthByOwner()
        {
            if (m_FollowTo)
            {
                m_FollowTo.enabled = false;
            }
            
            if (m_Interactable && m_Interactable.isSelected && m_Interactable.interactionManager &&
                m_Interactor != null)
            {
                m_Interactable.interactionManager.SelectExit(m_Interactor, m_Interactable);
            }
            else
            {
                Debug.Log($"DDD_GGG_TAG: Failed to invoke SelectExit method in {gameObject.name}");
            }

            if (m_Rb)
            {
                m_Rb.isKinematic = true;
                m_Interactable.movementType = XRBaseInteractable.MovementType.Kinematic;
            }
        }

        private void OnOwnerChange(uint oldOwner, uint newOwner)
        {
            if (NetworkClient.localPlayer.TryGetComponent(out PlayerController localPlayer))
            {
                if (localPlayer.Identity.netId != newOwner)
                {
                    ChangeAuthByOwner();
                }
            }
        }
    }
}