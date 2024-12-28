using Mirror;
using Mirror.Discovery;

namespace DemoX.Framework.Net
{
    public partial class XNetManager : NetworkManager
    {
        public static XNetManager Ins;

        private NetSceneSetting _netSceneSetting;
        
        private NetworkDiscovery _networkDiscovery;
        private NetworkManagerHUD _networkManagerHUD;
        private NetDiscoveryUI _netDiscoveryUI;
        public NetSceneSetting NetSceneSetting => _netSceneSetting;
        

        public override void Awake()
        {
            _networkDiscovery = GetComponent<NetworkDiscovery>();
            _networkManagerHUD = GetComponent<NetworkManagerHUD>();
            _netDiscoveryUI = GetComponent<NetDiscoveryUI>();

            Ins = this;
            base.Awake();
            autoCreatePlayer = false;
            RetrieveSceneSettings();
        }

        private void EnableGUI(bool enable)
        {
            if (_networkDiscovery)
            {
                _networkDiscovery.enabled = enable;
            }
            
            if (_networkManagerHUD)
            {
                _networkManagerHUD.enabled = enable;
            }
            
            if (_netDiscoveryUI)
            {
                _netDiscoveryUI.enabled = enable;
            }
        }

        private void RetrieveSceneSettings()
        {
            _netSceneSetting = GetComponent<NetSceneSetting>();
            if (!_netSceneSetting) return;
            offlineScene = _netSceneSetting.Offline;
            onlineScene = _netSceneSetting.Online;
        }
    }
}