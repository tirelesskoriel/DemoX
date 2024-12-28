using System;
using Mirror;
using UnityEngine;

namespace DemoX.Framework.Level
{
    public class StageLight : NetworkBehaviour
    {
        [SerializeField] private float _changeSpeed = 20.0f;

        private float _targetIntensity = 0.0f;

        public float SetTargetIntensity
        {
            set => _targetIntensity = value;
        }

        [SyncVar(hook = nameof(ChangeLightIntensity))]
        private float _lightIntensity = 0.0f;

        private Light _light;

        private void Awake()
        {
            _light = GetComponent<Light>();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            if (_light)
            {
                _lightIntensity = _light.intensity;
            }
        }

        private void Update()
        {
            SerUpdate();
        }

        private void SerUpdate()
        {
            if (isServer)
            {
                SerCalLightIntensity();
            }
        }

        private void SerCalLightIntensity()
        {
            float difference = _targetIntensity - _lightIntensity;
            int frameCount = (int)(1.0f / Time.deltaTime);
            float speedPerFrame = _changeSpeed / frameCount * 2;
            
            if (Math.Abs(_lightIntensity - _targetIntensity) > speedPerFrame)
            {
                float diffSign = Mathf.Sign(difference);
                float intensity = _lightIntensity + diffSign * _changeSpeed * Time.deltaTime;
                if (Mathf.Abs(_lightIntensity - intensity) < speedPerFrame)
                {
                    _lightIntensity = intensity * 100 / 100.0f;
                }
            }
        }

        private void ChangeLightIntensity(float _, float newVal)
        {
            if (!_light) return;
            _light.intensity = newVal;
        }
    }
}