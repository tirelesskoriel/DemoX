using DemoX.Framework.Net;
using Mirror;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace Runtime
{
    public class PlayerController : BaseNetworkBehaviour
    {
        [SerializeField] private Transform m_Xr;
        [SerializeField] private TrackedPoseDriver m_HeadTracked;
        [SerializeField] private Renderer m_HeadRenderer;

        private PXR_Manager m_PxrManager;
        private NetworkIdentity m_Identity;

        public NetworkIdentity Identity => m_Identity;
        
        private void Awake()
        {
            m_Identity = GetComponent<NetworkIdentity>();
            
            EnableTransform(m_Xr, false);
            EnableTransform(m_PxrManager, false);
            EnableTransform(m_HeadTracked, false);
            EnableTransform(m_HeadRenderer, false);

            DontDestroyOnLoad(gameObject);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            EnableTransform(m_Xr, isLocalPlayer);
            EnableTransform(m_PxrManager, isLocalPlayer);
            EnableTransform(m_HeadTracked, isLocalPlayer);
            EnableTransform(m_HeadRenderer, !isLocalPlayer);
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            EnableTransform(m_Xr, false);
            EnableTransform(m_PxrManager, false);
            EnableTransform(m_HeadTracked, false);
            EnableTransform(m_HeadRenderer, false);
        }

        
        
        [Command]
        public void CmdEnterScene(string sceneName)
        {
            Debug.Log($"CmdEnterScene: {sceneName}");
            if (string.Equals(gameObject.scene.name, sceneName))
            {
                Debug.Log($"CmdEnterScene: same scene!!!!");
                return;
            }

            if (string.Equals(sceneName, "reset"))
            {
                XNetManager.Ins.StartReset();
            }
            else
            {
                XNetManager.Ins.EnterScene(gameObject, sceneName);
            }
        }
    }
}