using DG.Tweening;
using UnityEngine;
using Component = UnityEngine.Component;

namespace DemoX.Framework.Core
{
    public static class ComponentEX
    {
        public static T GetSys<T>(this Component comp) where T : Component
        {
            return Game.Get<T>();
        }

        public static void EnableAnyWhereComponents<T>(this Component comp, bool enable) where T : Behaviour
        {
            if (comp.EnableComponents<T>(enable))
            {
                return;
            }

            if (comp.EnableChildrenComponents<T>(enable))
            {
                return;
            }

            if (comp.EnableParentComponents<T>(enable))
            {
                return;
            }
        }

        public static bool EnableComponents<T>(this Component comp, bool enable) where T : Behaviour
        {
            bool result = false;
            foreach (var c in comp.GetComponents<T>())
            {
                Log(enable, c);
                c.enabled = enable;
                result = true;
            }

            return result;
        }

        public static bool EnableChildrenComponents<T>(this Component comp, bool enable) where T : Behaviour
        {
            bool result = false;
            foreach (var c in comp.GetComponentsInChildren<T>())
            {
                Log(enable, c);
                c.enabled = enable;
                result = true;
            }

            return result;
        }

        public static bool EnableParentComponents<T>(this Component comp, bool enable) where T : Behaviour
        {
            bool result = false;
            foreach (var c in comp.GetComponentsInParent<T>())
            {
                Log(enable, c);
                c.enabled = enable;
                result = true;
            }

            return result;
        }

        private static void Log<T>(bool enable, T t) where T : Behaviour
        {
            if (!t) return;
            XRLogger.Log($"[{enable}] {t.gameObject.name} {typeof(T)}");
        }

        public static void KillDOTween(this Component comp, Transform target)
        {
            if (target)
            {
                target.DOKill();
            }
        }
    }
}