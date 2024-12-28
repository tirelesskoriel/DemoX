using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace DemoX.Framework.Core
{
    public static class Game
    {
        private static readonly Dictionary<Type, Component> _InsCache = new();

        public static void Save(Component behaviour)
        {
            _InsCache[behaviour.GetType()] = behaviour;
        }

        public static T Get<T>() where T : Component
        {
            return (T)_InsCache[typeof(T)];
        }

        [Conditional("UNITY_EDITOR")]
        public static void Log(string msg)
        {
            Debug.Log($"[{Time.realtimeSinceStartupAsDouble}][{Time.frameCount}] {msg}");
        }
    }
}