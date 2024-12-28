using Mirror;
using UnityEngine;

namespace DemoX.Framework
{
    public class NetRenderCtrl : NetworkBehaviour
    {
        [SerializeField] private MeshRenderer _renderer;
        [SyncVar] private bool _bValid;
    }
}