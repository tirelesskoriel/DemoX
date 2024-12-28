using Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace DemoX.Framework.AINPC
{
    public class AINPC : MonoBehaviour
    {
        [SerializeField] private string m_speakContent;
        [SerializeField] private Button m_StartSpeak;
        [SerializeField] private Button m_StopAI;

        private LLM m_Llm;

        private ARS m_Ars;

        private TTS m_Tts;

        public static AINPC Ins;

        private AiButton m_StartAiButton;
        private AiButton m_StopAiButton;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Ins = this;
            m_Llm = GetComponent<LLM>();
            m_Tts = GetComponent<TTS>();
            m_Ars = GetComponent<ARS>();

            m_Tts.StoppedAction += () =>
            {
                if (m_StartAiButton)
                {
                    m_StartAiButton.gameObject.SetActive(true);
                }
            };

            if (Application.platform is RuntimePlatform.WindowsServer or RuntimePlatform.LinuxServer)
            {
                EnableAI(false);
            }
            else
            {
                EnableAI(true);
            }
        }

        public void SetupAiButton(AiButton stopAiButton, AiButton startAiButton)
        {
            if (!stopAiButton || !startAiButton) return;

            m_StartAiButton = startAiButton;
            m_StopAiButton = stopAiButton;

            stopAiButton.gameObject.SetActive(false);
            startAiButton.OnClick.AddListener(() =>
            {
                Debug.Log($"AIBUTTON_TAG: m_StartAiButton ==== click");
                m_Ars.StartArsServer();
                startAiButton.gameObject.SetActive(false);
            });

            stopAiButton.OnClick.AddListener(() =>
            {
                Debug.Log($"AIBUTTON_TAG: m_StopAiButton !!!! click");

                m_Tts.StopAll();
                startAiButton.gameObject.SetActive(true);
            });
        }

        private void Update()
        {
            // if (!m_StartCheck) return;
            // if (m_StartAiButton)
            // {
            //     Debug.Log($"AIBUTTON_TAG: 1111111111 {m_Llm.IsFinished && m_Tts.IsFresh}");
            //
            //     m_StartAiButton.gameObject.SetActive(m_Llm.IsFinished && m_Tts.IsFresh);
            // }
            //
            if (m_StopAiButton)
            {
                m_StopAiButton.gameObject.SetActive(m_Tts.IsPlaying);
            }
        }

        public void EnableAI(bool enable)
        {
            m_Llm.enabled = enable;
            m_Tts.enabled = enable;
            m_Ars.enabled = enable;
        }
    }
}