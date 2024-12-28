using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DemoX.Framework.Core
{
    public static class FrameworkLauncher
    {
        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        // static void OnBeforeSceneLoad()
        // {
        //     var types = TypeCache.GetTypesWithAttribute<GameSystem>();
        //     foreach (var type in types)
        //     {
        //         GameSystem gameSystemAttr = (GameSystem)type.GetCustomAttribute(typeof(GameSystem), false);
        //         if (gameSystemAttr.InsType != GameSystem.InstanceType.Create) continue;
        //
        //         GameObject gameSystem = new GameObject(type.Name);
        //         var component = gameSystem.AddComponent(type);
        //         Object.DontDestroyOnLoad(gameSystem);
        //         Game.Save(component);
        //     }
        // }

        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        // static void OnAfterSceneLoad()
        // {
        //     var types = TypeCache.GetTypesWithAttribute<GameSystem>();
        //     foreach (var type in types)
        //     {
        //         GameSystem gameSystemAttr = (GameSystem)type.GetCustomAttribute(typeof(GameSystem), false);
        //         if (gameSystemAttr.InsType != GameSystem.InstanceType.Find) continue;
        //
        //         Object obj = Object.FindFirstObjectByType(type);
        //         Component comp = obj.GetComponent(type);
        //         Game.Save(comp);
        //     }
        // }
    }
}