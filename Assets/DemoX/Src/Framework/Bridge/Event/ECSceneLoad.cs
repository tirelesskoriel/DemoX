using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace DemoX.Framework.Bridge.Event
{
    [CreateAssetMenu(menuName = "Event Channel/SceneLoadEventChannel", fileName = "EC_SceneLoad")]
    public class ECSceneLoad : ScriptableObject
    {
        public UnityEvent<string> OnStartLoading;
        public UnityEvent<string> OnFinishLoading;

        public UnityEvent OnLoadingStop;
        public UnityEvent<string> OnLoadingAnimStop;
    }
}