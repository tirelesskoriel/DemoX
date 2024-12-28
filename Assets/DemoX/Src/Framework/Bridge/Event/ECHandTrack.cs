using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.Events;

namespace DemoX.Framework.Bridge.Event
{
    [CreateAssetMenu(menuName = "Event Channel/HandPoseTriggerEventChannel", fileName = "EC_HandPoseTrigger")]
    public class ECHandTrack : ScriptableObject
    {
        public UnityEvent<HandType, bool> HandTrackState;
    }
}