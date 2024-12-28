using System.Collections;
using DemoX.Framework.Core;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DemoX.Framework.Net
{
    public partial class XNetManager
    {
        private bool _subScenesLoaded;

        /// <summary>
        /// When the server starts 1
        /// </summary>
        public override void OnStartServer()
        {
            Game.Log("OnStartServer");
            base.OnStartServer();
            if (_networkDiscovery)
            {
                _networkDiscovery.AdvertiseServer();
            }

            // EnableGUI(false);
        }
        
        /// <summary>
        /// When the server starts 1
        /// </summary>
        public override void OnServerSceneChanged(string sceneName)
        {
            base.OnServerSceneChanged(sceneName);
            Game.Log($"OnServerSceneChanged {sceneName}");

            if (sceneName == onlineScene)
            {
                StartCoroutine(LoadSubScenes());
            }
        }

        private IEnumerator UnloadSubScenes()
        {
            foreach (var scene in _netSceneSetting.Scenes)
            {
                Game.Log($"UnloadSubScenes: {scene}");
                yield return SceneManager.UnloadSceneAsync(scene);
                yield return Resources.UnloadUnusedAssets();
            }
        }

        private IEnumerator LoadSubScenes()
        {
            LoadSceneParameters loadSceneParameters = new LoadSceneParameters()
            {
                loadSceneMode = LoadSceneMode.Additive,
                localPhysicsMode = LocalPhysicsMode.Physics3D,
            };
            foreach (var scene in _netSceneSetting.Scenes)
            {
                yield return SceneManager.LoadSceneAsync(scene, loadSceneParameters);
            }

            _subScenesLoaded = true;
        }

        public override void OnServerChangeScene(string newSceneName)
        {
            base.OnServerChangeScene(newSceneName);
            Game.Log($"OnServerChangeScene {newSceneName}");
        }

        /// <summary>
        /// When a client connects 1
        /// </summary>
        /// <param name="conn"></param>
        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            Game.Log("OnServerConnect");
            base.OnServerConnect(conn);
        }

        /// <summary>
        /// When a client connects 2
        /// </summary>
        /// <param name="conn"></param>
        public override void OnServerReady(NetworkConnectionToClient conn)
        {
            Game.Log("OnServerReady");
            base.OnServerReady(conn);

            if (conn.identity == null)
            {
                StartCoroutine(AddPlayerDelayed(conn));
            }
        }

        IEnumerator AddPlayerDelayed(NetworkConnectionToClient conn)
        {
            while (!_subScenesLoaded)
            {
                yield return null;
            }

            string scenePath = _netSceneSetting.Scenes[0];
            conn.Send(new SceneMessage
            {
                sceneName = scenePath, sceneOperation = SceneOperation.LoadAdditive,
                customHandling = true
            });

            yield return CreatePlayer(playerPrefab, conn, scenePath, true);

            // foreach (var spawnPrefab in spawnPrefabs)
            // {
            //     yield return CreatePlayer(spawnPrefab, conn, scenePath);
            // }
        }

        private IEnumerator CreatePlayer(GameObject playerGameObj, NetworkConnectionToClient conn, string scenePath,
            bool isPlayer = false)
        {
            GameObject player;
            Transform start = GetStartPosition();
            if (start)
            {
                player = Instantiate(playerGameObj, start);
                player.transform.SetParent(null);
            }
            else
            {
                player = Instantiate(playerGameObj);
                Scene firstScene = SceneManager.GetSceneByPath(scenePath);
                SceneManager.MoveGameObjectToScene(player, firstScene);
            }

            yield return null;

            if (isPlayer)
            {
                NetworkServer.AddPlayerForConnection(conn, player);
            }
            else if (conn.identity.TryGetComponent(out NetPlayer netPlayer))
            {
                NetworkServer.Spawn(player, conn);
                if (player.TryGetComponent(out HandController handController))
                {
                    netPlayer.SerAddOwnedHand(handController);

                    if (player.TryGetComponent(out NetHand hand))
                    {
                        hand.Player = netPlayer;
                    }
                }
            }
        }

        /// <summary>
        /// When a client connects 3
        /// </summary>
        /// <param name="conn"></param>
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            Game.Log("OnServerAddPlayer");
            base.OnServerAddPlayer(conn);
        }

        /// <summary>
        /// When a client disconnects:
        /// </summary>
        /// <param name="conn"></param>
        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            Game.Log("OnServerDisconnect");
            base.OnServerDisconnect(conn);
        }

        public override void OnStopServer()
        {
            Game.Log("OnStopServer");
            base.OnStopServer();
            // EnableGUI(true);
        }

        public void EnterScene(GameObject player, string sceneName)
        {
            Game.Log($"EnterScene: {sceneName}");
            if (string.IsNullOrEmpty(sceneName)) return;
            
            Game.Log($"CmdEnterScene: {sceneName}");
            if (string.Equals(player.scene.name, sceneName))
            {
                Game.Log($"CmdEnterScene: same scene!!!!");
                return;
            }
            
            StartCoroutine(SendPlayerTo(player, sceneName));
        }

        public void EnterScene(string sceneName)
        {
            Game.Log($"EnterScene only name: {sceneName}");
            if (string.IsNullOrEmpty(sceneName)) return;
            StartCoroutine(SendPlayerTo(sceneName));
        }

        public IEnumerator SendPlayerTo(string sceneName)
        {
            foreach (var (_, conn) in NetworkServer.connections)
            {
                yield return SendPlayer(conn.identity.gameObject, conn, sceneName);
            }
        }

        public IEnumerator SendPlayerTo(GameObject player, string sceneName)
        {
            NetworkIdentity identity = player.GetComponent<NetworkIdentity>();
            NetworkConnectionToClient conn = identity.connectionToClient;
            yield return SendPlayer(player, conn, sceneName);
        }

        private IEnumerator SendPlayer(GameObject player, NetworkConnectionToClient conn, string sceneName)
        {
            if (!player || conn == null) yield break;

            string lastScenePath = player.scene.path;

            conn.Send(new SceneMessage
                { sceneName = sceneName, sceneOperation = SceneOperation.LoadAdditive, customHandling = true });

            MovePlayerToScene(sceneName, player);
            yield return null;
            
            conn.Send(new SceneMessage
            {
                sceneName = lastScenePath, sceneOperation = SceneOperation.UnloadAdditive,
                customHandling = true
            });
            yield return null;
        }

        public void StartReset()
        {
            StartCoroutine(ResetScene());
        }

        public IEnumerator ResetScene()
        {
            foreach (var (_, conn) in NetworkServer.connections)
            {
                GameObject player = conn.identity.gameObject;

                conn.Send(new SceneMessage
                {
                    sceneName = player.scene.path, sceneOperation = SceneOperation.UnloadAdditive,
                    customHandling = true
                });

                yield return null;

                SceneManager.MoveGameObjectToScene(player, SceneManager.GetSceneByPath(onlineScene));

                if (player.TryGetComponent(out NetPlayer netPlayer))
                {
                    foreach (var hand in netPlayer.SerOwnedHands)
                    {
                        SceneManager.MoveGameObjectToScene(hand.gameObject, SceneManager.GetSceneByPath(onlineScene));
                    }
                }

                yield return null;
            }

            yield return UnloadSubScenes();
            yield return LoadSubScenes();

            string firstScenePath = _netSceneSetting.Scenes[0];

            foreach (var (_, conn) in NetworkServer.connections)
            {
                conn.Send(new SceneMessage
                {
                    sceneName = firstScenePath, sceneOperation = SceneOperation.LoadAdditive,
                    customHandling = true
                });

                yield return null;

                GameObject player = conn.identity.gameObject;
                SceneManager.MoveGameObjectToScene(player, SceneManager.GetSceneByPath(firstScenePath));

                if (player.TryGetComponent(out NetPlayer netPlayer))
                {
                    foreach (var hand in netPlayer.SerOwnedHands)
                    {
                        SceneManager.MoveGameObjectToScene(hand.gameObject,
                            SceneManager.GetSceneByPath(firstScenePath));
                    }
                }

                yield return null;
            }
        }

        public void MovePlayerToScene(string sceneName, GameObject player)
        {
            Scene activityScene = SceneManager.GetSceneByName(sceneName);
            Game.Log($"MovePlayerToScene : {sceneName}  {activityScene.IsValid()}  {activityScene.path}");
            SceneManager.MoveGameObjectToScene(player, activityScene);
            if (player.TryGetComponent(out NetPlayer netPlayer))
            {
                foreach (var hand in netPlayer.SerOwnedHands)
                {
                    // NetworkServer.UnSpawn(identity.gameObject);
                    SceneManager.MoveGameObjectToScene(hand.gameObject, SceneManager.GetSceneByName(sceneName));
                }
            }
        }
    }
}