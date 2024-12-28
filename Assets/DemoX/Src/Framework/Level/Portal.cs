using DemoX.Framework.Core;
using Mirror;
using UnityEngine;

namespace DemoX.Framework.Level
{
    public class Portal : MonoBehaviour
    {
        [SerializeField] [SceneField] private string _destination;
        [SerializeField] private Transform _trigger;
        public string Destination => _destination;
        public Transform Trigger => _trigger;
    }
}