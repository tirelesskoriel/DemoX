using System;
using UnityEngine;
using UnityEngine.Events;

namespace Runtime
{
    public class AiButton : MonoBehaviour
    {
        public class ButtonClickedEvent : UnityEvent {}

        public ButtonClickedEvent OnClick = new();
        private Transform _touch;
        
        private void Trigger()
        {
            Debug.Log($"AIBUTTON_TAG: Trigger");
            OnClick?.Invoke();
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("IndexFinger") && !_touch)
            {
                _touch = other.transform;
                Invoke(nameof(Trigger), 0.5f);
            }
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