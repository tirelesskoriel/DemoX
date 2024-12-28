using DemoX.Framework.Core;
using UnityEngine;

namespace DemoX.Framework
{
    public class BodyVelocityTrack : MonoBehaviour
    {
        private Vector3 _lastPosition;
        private Vector3 _motion;
        private Vector3 _motionVel;
        private Vector3 _motionDirection;

        public Vector3 Motion => _motion;
        public Vector3 MotionVel => _motionVel;
        public Vector3 MotionDirection => _motionDirection;

        private void FixedUpdate()
        {
            _motion = transform.position - _lastPosition;
            _motionDirection = _motion.normalized;
            _motionVel = _motion / Time.fixedDeltaTime;
            _lastPosition = transform.position;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + _motion * 2000);
        }
    }
}