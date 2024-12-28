using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DemoX.Framework.Net
{
    public class NetSceneManager : NetworkBehaviour
    {
        public static NetSceneManager Ins;

        private void Awake()
        {
            if (!Ins)
            {
                Ins = this;
            }
        }

        public void StartSendPlayerToNewScene(string sceneName, bool global = false)
        {
            // StartCoroutine(SendPlayerTo(player, sceneName, global));
        }
        
        public void StartSendPlayerToNewScene(GameObject player, string sceneName, bool global = false)
        {
            // StartCoroutine(SendPlayerTo(player, sceneName, global));
        }

        
    }
}