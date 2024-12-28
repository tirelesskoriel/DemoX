using UnityEngine;
using UnityEngine.Events;

namespace DemoX.Framework.Bridge.Event
{
    [CreateAssetMenu(menuName = "Event Channel/GameManagerEventChannel", fileName = "EC_GameManager")]
    public class ECGameManager : ScriptableObject
    {
        public UnityEvent GameStart;
    }
}