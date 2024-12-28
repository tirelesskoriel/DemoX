using DemoX.Framework.Core;
using DemoX.Framework.Level;
using DemoX.Framework.Net;
using Mirror;
using Unity.XR.PXR;
using UnityEngine;

namespace DemoX.Framework
{
    public class NetHand : NetworkBehaviour
    {
        [SerializeField] private Transform _hand;
        [SerializeField] private NetPlayer _player;

        public NetPlayer Player
        {
            get => _player;
            set => _player = value;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            DontDestroyOnLoad(gameObject);

            if (isOwned)
            {
                _hand.EnableAnyWhereComponents<HandPoseTrigger>(true);
                _hand.EnableAnyWhereComponents<PXR_Hand>(true);
                _hand.EnableAnyWhereComponents<PXR_HandPose>(true);
                _hand.EnableAnyWhereComponents<UIHandController>(true);
            }
            else
            {
                // _hand.EnableAnyWhereComponents<HandRayLine>(false);
                _hand.EnableAnyWhereComponents<HandPoseTrigger>(false);
                _hand.EnableAnyWhereComponents<PXR_Hand>(false);
                _hand.EnableAnyWhereComponents<PXR_HandPose>(false);
                _hand.EnableAnyWhereComponents<UIHandController>(false);

            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            _hand.EnableAnyWhereComponents<HandPoseTrigger>(false);
            _hand.EnableAnyWhereComponents<PXR_Hand>(false);
            _hand.EnableAnyWhereComponents<PXR_HandPose>(false);
            _hand.EnableAnyWhereComponents<UIHandController>(false);
            _hand.EnableAnyWhereComponents<HandController>(false);

        }
    }
}