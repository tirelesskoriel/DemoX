using Mirror;

namespace DemoX.Framework.Net
{
    public class NetGameManager : NetworkBehaviour
    {
        public static NetGameManager Ins;

        // [SyncVar] private bool _bGameStarted;
        //
        // public bool GameStarted => _bGameStarted;
        //
        // [Command(requiresAuthority = false)]
        // public void CmdSetGameStarted(bool val)
        // {
        //     _bGameStarted = val;
        // }

        public void Awake()
        {
            Ins = this;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            Ins = this;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            Ins = this;
        }
    }
}