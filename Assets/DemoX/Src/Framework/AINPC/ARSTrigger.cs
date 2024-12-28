using System;
using DemoX.Framework.Bridge.Event;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace DemoX.Framework.AINPC
{
    public class ARSTrigger : MonoBehaviour
    {
        [SerializeField] private ECAINPC _ecainpc;

        [FormerlySerializedAs("_ARSTriggerEvent")] public UnityEvent ARSTriggerEvent;

        private Transform _touch;

        private void OnEnable()
        {
            if (_ecainpc)
            {
                
            }
        }
        
        private void OnDisable()
        {
            _touch = null;
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("HandSpace") && !_touch)
            {
                _touch = other.transform;
                Invoke(nameof(Trigger), 0.5f);
            }
        }

        private void Trigger()
        {
            ARSTriggerEvent.Invoke();
            
            // if (AINPC.Ins.Ars.StartArsServer())
            // {
            //     gameObject.SetActive(false);
            // }
        }


        private void OnTriggerExit(Collider other)
        {
            if (other.transform == _touch)
            {
                _touch = null;
                CancelInvoke(nameof(Trigger));
            }
        }
    }
}