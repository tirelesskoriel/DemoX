using UnityEngine;

namespace DemoX.Framework
{
    public class PhysicsTest : MonoBehaviour
    {
        [SerializeField] private Transform _forcePoint;
        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (!_forcePoint || !_rb) return;

            _rb.AddForceAtPosition(_forcePoint.forward, _forcePoint.position, ForceMode.VelocityChange);
        }
    }
}