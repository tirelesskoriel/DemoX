using System;
using System.Collections;
using DemoX.Framework.Core;
using UnityEngine;

namespace DemoX.Framework
{
    public class ShowRotation : MonoBehaviour
    {
        [SerializeField] private Transform _anchor;
        [SerializeField] private Transform _target;
        private void Update()
        {

            float angle = Vector3.SignedAngle(_anchor.forward, _target.forward, _anchor.up);
            float diff = Mathf.Abs(angle) - 60.0f;
            if (diff > 0.0f)
            {
                _anchor.RotateAround(_anchor.position, _anchor.up, Mathf.Sign(angle) * diff);
            }
            
            Game.Log($"=== {_anchor.forward}");

        }
    }
}