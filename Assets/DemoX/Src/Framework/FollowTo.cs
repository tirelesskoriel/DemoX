using DG.Tweening;
using UnityEngine;

namespace DemoX.Framework
{
    public class FollowTo : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _offset;
        [SerializeField] private ESpace _eSpace;
        [SerializeField] private ETransform _eTransform;
        [SerializeField] private float _delay = 0.0f;

        public enum ESpace
        {
            World,
            Local
        }

        public enum ETransform
        {
            Position,
            Rotation,
            PositionAndRotation
        }

        private void Update()
        {
            switch (_eSpace)
            {
                case ESpace.Local:
                    SetLocal();
                    break;
                case ESpace.World:
                    SetWorld();
                    break;
            }
        }

        private void SetLocal()
        {
            switch (_eTransform)
            {
                case ETransform.Position:
                    SetLocalPosition();
                    break;
                case ETransform.Rotation:
                    SetLocalRotation();
                    break;
                case ETransform.PositionAndRotation:
                    SetLocalPositionAndRotation();
                    break;
            }
        }

        private void SetWorld()
        {
            switch (_eTransform)
            {
                case ETransform.Position:
                    SetPosition();
                    break;
                case ETransform.Rotation:
                    SetRotation();
                    break;
                case ETransform.PositionAndRotation:
                    SetPositionAndRotation();
                    break;
            }
        }

        private void SetPosition()
        {
            if (_delay > 0.0f)
            {
                transform.DOMove(_target.position + _offset, _delay);
            }
            else
            {
                transform.position = _target.position + _offset;
            }
        }

        private void SetRotation()
        {
            if (_delay > 0.0f)
            {
                transform.DORotateQuaternion(_target.rotation, _delay);
            }
            else
            {
                transform.rotation = _target.rotation;
            }
        }

        private void SetPositionAndRotation()
        {
            if (_delay > 0.0f)
            {
                transform.DOMove(_target.position + _offset, _delay);
                transform.DORotateQuaternion(_target.rotation, _delay);
            }
            else
            {
                transform.SetPositionAndRotation(_target.position + _offset, _target.rotation);
            }
        }

        private void SetLocalPosition()
        {
            if (_delay > 0.0f)
            {
                transform.DOLocalMove(_target.localPosition + _offset, _delay);
            }
            else
            {
                transform.localPosition = _target.localPosition + _offset;
            }
        }

        private void SetLocalRotation()
        {
            if (_delay > 0.0f)
            {
                transform.DOLocalRotateQuaternion(_target.localRotation, _delay);
            }
            else
            {
                transform.localRotation = _target.localRotation;
            }
        }

        private void SetLocalPositionAndRotation()
        {
            if (_delay > 0.0f)
            {
                transform.DOLocalMove(_target.localPosition + _offset, _delay);
                transform.DOLocalRotateQuaternion(_target.localRotation, _delay);
            }
            else
            {
                transform.SetLocalPositionAndRotation(_target.localPosition + _offset, _target.localRotation);
            }
        }
    }
}