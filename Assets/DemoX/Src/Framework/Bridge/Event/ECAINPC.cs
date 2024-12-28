using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace DemoX.Framework.Bridge.Event
{
    [CreateAssetMenu(menuName = "Event Channel/ainpcEventChannel", fileName = "EC_AIPNC")]
    public class ECAINPC : ScriptableObject
    {
        public UnityEvent<string> ShowArsToUI;
        public UnityEvent<string> ShowLlmResultToUI;
        public UnityEvent HideUI;
        public UnityEvent HideArsTriggerBtn;
        public UnityEvent<int, string> OnDebug;
    }
}