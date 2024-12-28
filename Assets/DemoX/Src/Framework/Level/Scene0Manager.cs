using System;
using System.Collections;
using System.Collections.Generic;
using DemoX.Framework.Bridge.Event;
using DemoX.Framework.Core;
using DemoX.Framework.Net;
using DG.Tweening;
using Mirror;
using UnityEngine;

namespace DemoX.Framework.Level
{
    public class Scene0Manager : NetworkBehaviour
    {
        [Header("Event Channel")] [SerializeField]
        private ECGameManager _ecGameManager;

        [Header("Spot Light")] [SerializeField]
        private List<LightConfig> _lightConfigs;

        [Header("Platform")] [SerializeField] private Transform _platform;
        [SerializeField] private Transform _platformDestination;
        [SerializeField] private float _platformUpDuration = 2.0f;
        [SerializeField] private Transform _platformMenuContainer;
        [SerializeField] private ECSWTrigger _ecHandleTrigger;

        [Header("Others")] [SerializeField] private int _swCount = 2;
        [SerializeField] private Transform _items;
        [SerializeField] private Transform _itemsPosition;

        [SceneField] [SerializeField] private string _mainScene;
        [SerializeField] private float _delayToMainScene = 2.0f;
        [SerializeField] private SODebugKeyboardKey _debugKeyboard;

        [Serializable]
        public class LightConfig
        {
            public StageLight StageLight;
            public float GameStartLightIntensity;
            public float MaxLightIntensity;
        }

        public enum ELightStage
        {
            Off,
            Lighting,
            Max
        }

        public enum EStage
        {
            None,
            Start,
            StageLighting,
            SwitchComplete
        }

        private EStage _eStage;
        private HashSet<Transform> _swCache = new();
        private WaitForSeconds _delayToMainWaitFor;

        public override void OnStartServer()
        {
            base.OnStartServer();
            _ecGameManager.GameStart.AddListener(OnGameStart);
            _ecHandleTrigger.SWTrigger.AddListener(OnSWTrigger);
            _delayToMainWaitFor = new WaitForSeconds(_delayToMainScene);
        }

        private void OnSWTrigger(Transform sw)
        {
            if (_eStage != EStage.Start && _eStage != EStage.StageLighting) return;

            _swCache.Add(sw);
            if (_swCache.Count == _swCount)
            {
                _eStage = EStage.SwitchComplete;
                ChangeLight(ELightStage.Off);
                // RpcShowMenu();
                // ItemsVisible();
                StartDelayToMain();
            }
        }

        private void StartDelayToMain()
        {
            StartCoroutine(DelayToMain());
        }

        private IEnumerator DelayToMain()
        {
            yield return _delayToMainWaitFor;
            XNetManager.Ins.EnterScene(_mainScene);
        }

        [ClientRpc]
        private void RpcShowMenu()
        {
            if (_platformMenuContainer)
            {
                _platformMenuContainer.gameObject.SetActive(true);
            }
        }

        public void ItemsVisible()
        {
            XRLogger.Log($"ItemsVisible: {_eStage}");

            CmdItemsVisible();
        }

        [Command(requiresAuthority = false)]
        private void CmdItemsVisible()
        {
            Game.Log($"CmdItemsVisible: {_eStage}");

            if (_items && _itemsPosition)
            {
                Game.Log($"CmdItemsVisible 000: {_eStage}");

                _items.transform.position = _itemsPosition.position;
            }
        }

        private void Update()
        {
            if (isServer) return;
            if (_debugKeyboard && _debugKeyboard.ShowItems.WasPressedThisFrame())
            {
                ItemsVisible();
            }
        }

        private void OnGameStart()
        {
            if (_eStage != EStage.None) return;
            if (!_platform || !_platformDestination) return;

            _eStage = EStage.Start;
            _platform.DOMove(_platformDestination.position, _platformUpDuration);

            ChangeLight(ELightStage.Lighting);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isServer || _eStage != EStage.Start) return;
            if (!other.CompareTag("Player")) return;
            _eStage = EStage.StageLighting;
            ChangeLight(ELightStage.Max);
            Game.Log($"game: {_eStage}");
        }

        private void ChangeLight(ELightStage eLightStage)
        {
            foreach (var lightConfig in _lightConfigs)
            {
                lightConfig.StageLight.SetTargetIntensity = eLightStage switch
                {
                    ELightStage.Off => 0.0f,
                    ELightStage.Lighting => lightConfig.GameStartLightIntensity,
                    ELightStage.Max => lightConfig.MaxLightIntensity,
                    _ => lightConfig.StageLight.SetTargetIntensity = 0.0f
                };
            }
        }

        private void OnDestroy()
        {
            this.KillDOTween(_platform);
        }
    }
}