using DemoX.Framework.AINPC;
using UnityEngine;

namespace Runtime
{
    public class AiButtonRegister : MonoBehaviour
    {
        [SerializeField] private AiButton m_StartAiButton;
        [SerializeField] private AiButton m_StopAiButton;

        private void Awake()
        {
            AINPC.Ins.SetupAiButton(m_StopAiButton, m_StartAiButton);
        }
    }
}