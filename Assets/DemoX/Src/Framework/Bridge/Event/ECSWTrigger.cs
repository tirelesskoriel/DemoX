using UnityEngine;
using UnityEngine.Events;

namespace DemoX.Framework.Bridge.Event
{
    [CreateAssetMenu(menuName = "Event Channel/SWTriggerEventChannel", fileName = "EC_SWTrigger")]
    public class ECSWTrigger : ScriptableObject
    {
        public UnityEvent<Transform> SWTrigger;
    }
}