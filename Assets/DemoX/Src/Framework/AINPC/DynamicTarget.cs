using Runtime;

namespace DemoX.Framework.AINPC
{
    public class DynamicTarget : BaseNetworkBehaviour
    {
        public override void OnStartClient()
        {
            base.OnStartClient();
            DynamicTargetSwitcher.Target = transform;
        }
    }
}