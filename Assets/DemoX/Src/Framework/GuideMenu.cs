using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace DemoX.Framework
{
    public class GuideMenu : NetworkBehaviour
    {
        [SerializeField] private List<MenuAnimConfig> _menuAnimConfigs;
        [SerializeField] private GuideMenuRingController _ringController;
        
        private float _lastProgress;
        private Animator _animator;

        [Serializable]
        public class MenuAnimConfig
        {
            public List<GuideMenuRenderer> menus;
            public Transform originPosition;
            public float positionThreshold;

            [HideInInspector] public float distance;
            [HideInInspector] public List<Vector3> startPositions;

        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            _animator.enabled = true;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            _animator.enabled = false;
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            foreach (var menuAnimConfig in _menuAnimConfigs)
            {
                menuAnimConfig.distance = menuAnimConfig.positionThreshold * (menuAnimConfig.menus.Count - 1);
                menuAnimConfig.startPositions = new();
                foreach (var guideMenuRenderer in menuAnimConfig.menus)
                {
                    menuAnimConfig.startPositions.Add(guideMenuRenderer.transform.position);
                }
            }
        }

        private void SMove()
        {
            if (!isServer) return;
            if (_ringController && Math.Abs(_lastProgress - _ringController.Progress) > 1e-5)
            {
                _lastProgress = _ringController.Progress;

                foreach (var menuAnimConfig in _menuAnimConfigs)
                {
                    for (var i = 0; i < menuAnimConfig.menus.Count; i++)
                    {
                        Vector3 startPosition = menuAnimConfig.startPositions[i];
                        GuideMenuRenderer guideMenuRenderer = menuAnimConfig.menus[i];

                        Transform guideMenuRendererTrans = guideMenuRenderer.transform;
                        guideMenuRendererTrans.position =
                            startPosition - _lastProgress * menuAnimConfig.distance * guideMenuRendererTrans.forward;
                    }
                }
            }
        }

        private void CChangeAlpha()
        {
            if (!isClient) return;
            foreach (var menuAnimConfig in _menuAnimConfigs)
            {
                for (var i = 0; i < menuAnimConfig.menus.Count; i++)
                {
                    GuideMenuRenderer guideMenuRenderer = menuAnimConfig.menus[i];
                    float distanceToOrigin =
                        Vector3.Distance(guideMenuRenderer.transform.position, menuAnimConfig.originPosition.position);
                    guideMenuRenderer.SetAlpha(1 - distanceToOrigin / menuAnimConfig.positionThreshold);
                }
            }
        }

        private void Update()
        {
            SMove();
            CChangeAlpha();
        }
    }
}