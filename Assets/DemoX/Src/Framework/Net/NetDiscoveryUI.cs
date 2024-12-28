using System.Collections;
using System.Collections.Generic;
using DemoX.Framework.Core;
using Mirror;
using Mirror.Discovery;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DemoX.Framework.Net
{
    public class NetDiscoveryUI : MonoBehaviour
    {
        [SerializeField] private Transform _container;
        [SerializeField] private Button _addressBtnPrefab;

        [SerializeField] private int _waitForDiscovery = 3;
        [SerializeField] private NetworkDiscovery _networkDiscovery;

        private Dictionary<string, Button> _buttons = new();

        private void Start()
        {
            StartCoroutine(FindServer());
        }

        private IEnumerator FindServer()
        {
            yield return new WaitForSeconds(_waitForDiscovery);
            if (NetworkClient.active || NetworkServer.active || !_networkDiscovery) yield break;
            _networkDiscovery.StartDiscovery();
        }

        public void OnDiscovery(ServerResponse info)
        {
            Game.Log($"OnDiscovery ==== {info.EndPoint.Address} {info.uri}");
            if (NetworkClient.active) return;
            
            _networkDiscovery.StopDiscovery();
            XNetManager.Ins.networkAddress = info.EndPoint.Address.ToString();
            XNetManager.Ins.StartClient();
        }
    }
}