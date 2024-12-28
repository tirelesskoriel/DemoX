using System;
using UnityEngine;

namespace DemoX.Framework
{
    public class GyroContainer : MonoBehaviour
    {
        private Quaternion _initQuaternion;

        private void Awake()
        {
            _initQuaternion = transform.rotation;
        }

        private void Start()
        {
        }

        private void LateUpdate()
        {
            transform.localRotation = Quaternion.identity;
        }
    }
}