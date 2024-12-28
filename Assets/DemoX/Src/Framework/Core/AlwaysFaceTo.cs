using UnityEngine;

namespace DemoX.Framework.Core
{
    public class AlwaysFaceTo : MonoBehaviour
    {
        [SerializeField] private Transform m_FaceTo;

        private void LateUpdate()
        {
            if (m_FaceTo)
            {
                Vector3 direction = Vector3.Normalize(transform.position - m_FaceTo.position);
                direction.y = 0.0f;
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }
}