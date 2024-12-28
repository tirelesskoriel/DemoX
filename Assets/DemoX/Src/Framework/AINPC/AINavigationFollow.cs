using UnityEngine;
using UnityEngine.AI;

namespace DemoX.Framework.AINPC
{
    public class AINavigationFollow : MonoBehaviour
    {
        [SerializeField] private Animator m_BodyAnim;
        [SerializeField] private Transform m_Body;
        [SerializeField] private float m_Threshold = 0.3f;

        private Vector3 m_LastPosition;
        private NavMeshAgent m_NavMeshAgent;
        private static readonly int IsWalkingAnimKey = Animator.StringToHash("IsWalking");

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            m_NavMeshAgent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            if (!DynamicTargetSwitcher.Target) return;
            
            Vector3 currentPosition = DynamicTargetSwitcher.Target.position;
            if (Vector3.Distance(currentPosition, m_LastPosition) > m_Threshold)
            {
                m_NavMeshAgent.destination = currentPosition;
                m_LastPosition = currentPosition;
            }

            if (m_NavMeshAgent.velocity.sqrMagnitude <= 0.0f)
            {
                
                Vector3 direction = DynamicTargetSwitcher.Target.position - m_Body.position;
                direction.y = 0;

                Quaternion targetRotation = DynamicTargetSwitcher.Target.rotation;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, m_NavMeshAgent.angularSpeed * Time.deltaTime);
            }
            
            if (m_BodyAnim)
            {
                m_BodyAnim.SetBool(IsWalkingAnimKey, m_NavMeshAgent.velocity.sqrMagnitude > 0.0f);
            }
        }
    }
}