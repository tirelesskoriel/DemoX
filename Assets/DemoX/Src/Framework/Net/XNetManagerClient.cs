using System.Collections;
using DemoX.Framework.Bridge.Event;
using DemoX.Framework.Core;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DemoX.Framework.Net
{
    public partial class XNetManager
    {
        [SerializeField] private ECSceneLoad _ecSceneLoad;
        [SerializeField] private bool _reset;

        private string _newSceneName;
        private bool _bFirstLoad = true;

        private bool _resetFlag;

        public override void Update()
        {
            base.Update();
            if (_resetFlag != _reset)
            {
                _resetFlag = _reset;
                StartReset();
            }
        }

        public override void OnStartClient()
        {
            Game.Log("Client Tag: OnStartClient");
            base.OnStartClient();
            _ecSceneLoad.OnLoadingStop.AddListener(StartLoad);
        }

        public override void OnClientConnect()
        {
            Game.Log("Client Tag: OnClientConnect");
            base.OnClientConnect();
            EnableGUI(false);
        }

        public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation,
            bool customHandling)
        {
            base.OnClientChangeScene(newSceneName, sceneOperation, customHandling);
            Game.Log($"Client Tag: OnClientChangeScene {newSceneName} {sceneOperation}");

            if (sceneOperation == SceneOperation.UnloadAdditive)
                StartCoroutine(UnloadAdditive(newSceneName));

            if (sceneOperation == SceneOperation.LoadAdditive)
            {
                _newSceneName = newSceneName;
                // StartLoad();
                // _ecSceneLoad.OnStartLoading.Invoke(newSceneName);
                if (_bFirstLoad)
                {
                    _bFirstLoad = false;
                    StartLoad();
                }
                else
                {
                    _ecSceneLoad.OnStartLoading.Invoke(newSceneName);
                }
            }
        }

        private void StartLoad()
        {
            StartCoroutine(LoadScene(_newSceneName));
        }

        private IEnumerator LoadScene(string sceneName)
        {
            Game.Log($"Client Tag: LoadAdditive {sceneName}h");

            if (mode == NetworkManagerMode.ClientOnly)
            {
                loadingSceneAsync = SceneManager.LoadSceneAsync(sceneName);
                while (loadingSceneAsync != null && !loadingSceneAsync.isDone)
                    yield return null;
            }

            NetworkClient.isLoadingScene = false;
            OnClientSceneChanged();
            _ecSceneLoad.OnFinishLoading.Invoke(sceneName);
        }

        IEnumerator UnloadAdditive(string sceneName)
        {
            Game.Log($"Client Tag: UnloadAdditive {sceneName}h");

            if (mode == NetworkManagerMode.ClientOnly)
            {
                while (loadingSceneAsync != null && !loadingSceneAsync.isDone)
                    yield return null;
                yield return Resources.UnloadUnusedAssets();
            }

            NetworkClient.isLoadingScene = false;

            OnClientSceneChanged();
        }

        public override void OnClientSceneChanged()
        {
            Game.Log("Client Tag: OnClientSceneChanged");
            base.OnClientSceneChanged();
        }

        /// <summary>
        /// When the client stops 1
        /// </summary>
        public override void OnStopClient()
        {
            Game.Log("Client Tag: OnStopClient");
            base.OnStopClient();
        }

        /// <summary>
        /// When the client stops 2
        /// </summary>
        public override void OnClientDisconnect()
        {
            Game.Log("Client Tag: OnClientDisconnect");
            base.OnClientDisconnect();
            EnableGUI(false);
        }
    }
}