using UnityEngine;

namespace DemoX.Framework
{
    public class TrackPoseDebug : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Mode _mode;

        public enum Mode
        {
            Position,
            Rotate,
            PositionAndRotate,
        }

        private void Update()
        {
            if (!_target) return;
            switch (_mode)
            {
                case Mode.Position:
                    transform.position = _target.position;
                    break;
                case Mode.Rotate:
                    transform.rotation = _target.rotation;
                    break;
                default:
                    transform.SetPositionAndRotation(_target.position, _target.rotation);
                    break;
            }
        }
    }
}