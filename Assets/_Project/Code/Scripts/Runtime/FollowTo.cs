using UnityEngine;

namespace Runtime
{
    public class FollowTo : MonoBehaviour
    {
        [SerializeField] private Transform m_Target;
        [SerializeField] private EFollowMode m_Mode;
        [SerializeField] private EUpdateMode m_UpdateMode;

        public enum EUpdateMode
        {
            Normal,
            Physic
        }

        public enum EFollowMode
        {
            Position,
            Rotation,
            PositionAndRotation,
        }

        public void SetTarget(Transform target)
        {
            if (m_Target == null || m_Target != target)
            {
                m_Target = target;
            }
        }

        private void FixedUpdate()
        {
            if (m_UpdateMode == EUpdateMode.Physic)
            {
                UpdateTransform();
            }
        }

        private void LateUpdate()
        {
            if (m_UpdateMode == EUpdateMode.Normal)
            {
                UpdateTransform();
            }
        }

        private void UpdateTransform()
        {
            if (!m_Target) return;

            switch (m_Mode)
            {
                case EFollowMode.Position:
                    transform.position = m_Target.position;
                    break;
                case EFollowMode.Rotation:
                    transform.rotation = m_Target.rotation;
                    break;
                case EFollowMode.PositionAndRotation:
                    transform.SetPositionAndRotation(m_Target.position, m_Target.rotation);
                    break;
            }
        }
    }
}