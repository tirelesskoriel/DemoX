using DemoX.Framework.Core;
using DemoX.Framework.Net;
using DG.Tweening;
using Mirror;
using Runtime;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace DemoX.Framework.Level
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class Ground : BaseNetworkBehaviour
    {
        [SerializeField] private Transform _door;

        private Transform _lastTrigger;
        private Vector3 _startPosition;

        protected override void OnClientCollisionEnter(Collision other)
        {
            Debug.Log($"DOOR_TRIGGER_TAG: ground OnCollisionEnter: {other.transform}");
            if (_lastTrigger == other.transform) return;

            DoorTrigger doorTrigger = other.transform.GetComponent<DoorTrigger>();
            Debug.Log($"DOOR_TRIGGER_TAG: 11111");

            if (!doorTrigger) return;
            // if (!doorTrigger || !doorTrigger.authority) return;
            
            
            _lastTrigger = other.transform;
            Debug.Log($"DOOR_TRIGGER_TAG: 22222");

            Vector3 contactPosition = other.contacts[0].point;
            _door.transform.position = contactPosition - Vector3.up * 5;

            if (doorTrigger.Interactor != null)
            {
                Debug.Log($"DOOR_TRIGGER_TAG: 3333");
            
                Vector3 doorDirection = Vector3.Normalize(_door.transform.position - doorTrigger.Interactor.transform.position);
                Vector3 forward = Vector3.ProjectOnPlane(doorDirection, transform.up).normalized;
                CmdDoorMove(doorTrigger, forward, contactPosition);
            }
        }

        [Command(requiresAuthority = false)]
        private void CmdDoorMove(DoorTrigger doorTrigger, Vector3 forward, Vector3 contactPosition)
        {
            _door.forward = forward;
            _door.transform.DOMove(contactPosition, 3.0f);
            NetworkServer.Destroy(doorTrigger.gameObject);

        }
    }
}