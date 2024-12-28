using DemoX.Framework.Bridge.Event;
using DemoX.Framework.Core;
using DemoX.Framework.Net;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DemoX.Framework
{
    public class UIHandController : NetworkBehaviour
    {
        [SerializeField] private Transform _uiRoot;
        [SerializeField] private Transform _gameStartBtnsContainer;

        [SceneField] [SerializeField] private string _initScene;
        [SerializeField] private ECGameManager _ecGameManager;
        [SerializeField] private ECSOHandPoseTrigger _ecHandPoseTrigger;

        [SerializeField] [SceneField] private string _debugTestRoomName;

        [SerializeField] private SODebugKeyboardKey _debugKeyboard;
        private Camera _camera;
        private bool _bHandUITrigger;

        private NetHand _netHand;

        private SceneMenu _sceneMenu;

        public SceneMenu SceneMenu
        {
            set
            {
                _sceneMenu = value;
                if (isOwned)
                {
                    // _sceneMenu.triggerEvent.AddListener(EnterScene);
                }
            }
        }

        private void Awake()
        {
            _uiRoot.gameObject.SetActive(false);
            _netHand = GetComponent<NetHand>();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (isOwned && _ecHandPoseTrigger)
            {
                _ecHandPoseTrigger.HandUITrigger.AddListener(OnVisible);
            }
        }

        private void Update()
        {
            if (isServer) return;
            if (isOwned && !_camera)
            {
                _camera = Camera.main;
            }

            CButtonSwitch();
            CRotateToCamera();
            CDebug();
        }

        private void CButtonSwitch()
        {
            if (!isOwned || !_gameStartBtnsContainer || !_sceneMenu) return;

            if (CheckScene())
            {
                _gameStartBtnsContainer.gameObject.SetActive(true);
                _sceneMenu.SetValid(false);
                _sceneMenu.Enable(false);
            }
            else
            {
                _gameStartBtnsContainer.gameObject.SetActive(false);
                _sceneMenu.Enable(true);
            }
        }

        private bool CheckScene()
        {
            int activeSceneIndex = SceneManager.sceneCount > 1 ? 1 : 0;

            Scene s = SceneManager.GetSceneAt(activeSceneIndex);
            // Game.Log($"{_initScene} {s.name} {s.path}");

            return string.Equals(_initScene, s.name);
        }

        private void CDebug()
        {
            if (!_debugKeyboard) return;

            if (!string.IsNullOrEmpty(_debugTestRoomName) && _debugKeyboard.DirectPickScene.WasPressedThisFrame())
            {
                if (string.Equals(_debugTestRoomName, "reset"))
                {
                    ResetScene();
                }
                else
                {
                    CmdEnterScene(_debugTestRoomName);
                }
            }

            if (_debugKeyboard.StartGame.WasPressedThisFrame())
            {
                StartGame();
            }

            if (_debugKeyboard.Reset.WasPressedThisFrame())
            {
                ResetScene();
            }
        }

        private void CRotateToCamera()
        {
            if (!isOwned || !_uiRoot || !_uiRoot.gameObject.activeSelf || !_camera) return;
            Vector3 uiRootPos = _uiRoot.position;
            Vector3 direction = Vector3.Normalize(uiRootPos - _camera.transform.position);
            Vector3 point = uiRootPos + direction;
            _uiRoot.LookAt(point);
        }

        public void StartGame()
        {
            XRLogger.Log($"game start button is clicked");
            // NetGameManager.Ins.CmdSetGameStarted(true);
            CmdStartGame();
        }

        [Command]
        private void CmdStartGame()
        {
            Game.Log("CmdStartGame====");
            if (!_ecGameManager) return;
            _ecGameManager.GameStart.Invoke();
        }

        public void OnVisible(bool visible)
        {
            XRLogger.Log($"ui: {visible}");
            if (!_uiRoot || !_sceneMenu) return;
            _uiRoot.gameObject.SetActive(visible);
            _sceneMenu.SetValid(visible);
        }

        public void EnterScene(string sceneName)
        {
            Game.Log($"EnterScene: {sceneName}");
            CmdEnterScene(sceneName);
        }

        [Command]
        private void CmdEnterScene(string sceneName)
        {
            Game.Log($"CmdEnterScene: {sceneName}");
            if (string.Equals(_netHand.Player.gameObject.scene.name, sceneName))
            {
                Game.Log($"CmdEnterScene: same scene!!!!");
                return;
            }

            if (string.Equals(sceneName, "reset"))
            {
                XNetManager.Ins.StartReset();
            }
            else
            {
                XNetManager.Ins.EnterScene(_netHand.Player.gameObject, sceneName);
            }
        }

        public void ResetScene()
        {
            // NetGameManager.Ins.CmdSetGameStarted(false);
            CmdReset();
        }

        [Command]
        public void CmdReset()
        {
            XNetManager.Ins.StartReset();
        }
    }
}