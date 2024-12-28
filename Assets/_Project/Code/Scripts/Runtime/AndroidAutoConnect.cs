using Mirror;
using Mirror.Discovery;
using UnityEngine;

namespace Runtime
{
    public class AndroidAutoConnect : MonoBehaviour
    {
        [SerializeField] private NetworkDiscovery m_Discovery;

        private void Awake()
        {
            if (m_Discovery && Application.platform == RuntimePlatform.Android)
            {
                m_Discovery.OnServerFound.AddListener(OnDiscovered);
                m_Discovery.StartDiscovery();
            }
        }

        public void OnDiscovered(ServerResponse response)
        {
            Debug.Log($"DDD_GGG_TAG: OnDiscovered === {response.uri}");
            if (!m_Discovery) return;
            if (Application.platform != RuntimePlatform.Android) return;
            Debug.Log($"DDD_GGG_TAG: OnDiscovered 000 === {response.uri}");

            m_Discovery.StopDiscovery();
            NetworkManager.singleton.StartClient(response.uri);
        }
    }
}