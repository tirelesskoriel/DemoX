using DemoX.Framework.Core;
using DemoX.Framework.Level;
using Mirror;
using UnityEngine;

namespace DemoX.Framework
{
    public class NetObject : NetworkBehaviour
    {
        public override void OnStartServer()
        {
            base.OnStartServer();
            transform.EnableComponents<Handleable>(true);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            transform.EnableComponents<Handleable>(false);
        }
    }
}