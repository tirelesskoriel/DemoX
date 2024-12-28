using UnityEngine;
using UnityEngine.Events;

namespace DemoX.Framework.Bridge.Event
{
    [CreateAssetMenu(menuName = "Event Channel/HandPoseTriggerEventChannel", fileName = "EC_HandPoseTrigger")]
    public class ECSOHandPoseTrigger : ScriptableObject
    {
        public UnityEvent PinchStart;
        public UnityEvent PinchStop;
        
        public UnityEvent ExactPinchStart;
        public UnityEvent ExactPinchStop;
        
        public UnityEvent FistStart;
        public UnityEvent FistStop;

        public UnityEvent<bool> HandUITrigger;
        public UnityEvent ShowDebugLoggerTrigger;
    }
}