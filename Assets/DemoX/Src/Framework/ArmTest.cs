using UnityEngine;
using UnityEngine.Events;

namespace DemoX.Framework
{
    public class ArmTest : MonoBehaviour
    {
        [SerializeField] private Transform _hand;
        [SerializeField] private Transform _arm;
        [SerializeField] private Transform _target;

        public UnityEvent<bool> t;
        private void Update()
        {
            t.Invoke(true);
            if (!_arm || !_target || !_hand) return;

            Vector3 angle = _hand.localRotation.eulerAngles;
            Vector3 direction = (_arm.position - _target.position).normalized;
            _arm.forward = direction;

            _arm.Rotate(Vector3.forward, angle.z, Space.Self);
        }

        private void LateUpdate()
        {
            // _arm.up = up;
        }
    }
}