using System.Collections.Generic;
using UnityEngine;

namespace DemoX.Framework
{
    public class GuideMenuRenderer : MonoBehaviour
    {
        private readonly List<SpriteRenderer> _spriteRenderers = new();

        private void Awake()
        {
            _spriteRenderers.AddRange(GetComponentsInChildren<SpriteRenderer>());
        }

        public void SetAlpha(float alpha)
        {
            // foreach (var spriteRenderer in _spriteRenderers)
            // {
            //     Color color = spriteRenderer.color;
            //     color.a = alpha;
            //     spriteRenderer.color = color;
            // }
        }
    }
}