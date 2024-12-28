using System.Collections;
using Mirror;
using UnityEngine;

namespace DemoX.Framework.Level
{
    public class SkyCover : NetworkBehaviour
    {
        [SyncVar(hook = nameof(ChangeAlpha))] private float _alphaValue;

        private MeshRenderer _meshRenderer;

        private void Awake()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        private void ChangeAlpha(float _, float newVal)
        {
            var material = _meshRenderer.material;
            Color color = material.color;
            color.a = newVal;
            material.color = color;
        }

        public void StartToVisible()
        {
            StartCoroutine(VisibleGlobal());
        }

        private IEnumerator VisibleGlobal()
        {
            if (!_meshRenderer) yield break;
            Color color = _meshRenderer.material.color;
            _alphaValue = color.a;
            while (_alphaValue > 0)
            {
                _alphaValue -= Time.deltaTime * 0.2f;
                yield return null;
            }
        }
    }
}