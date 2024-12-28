using DemoX.Framework.Bridge.Event;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.InputSystem;

namespace DemoX.Framework.AINPC
{
    public class ARS : MonoBehaviour
    {
        [SerializeField] private ECAINPC m_UiController;

        [SerializeField] private bool m_AutoStop = true;
        [SerializeField] private bool m_ShowPunctual = true;
        [SerializeField] private int m_MaxDuration = 55;
        [SerializeField] private SpeechToText m_SpeechToText;

        private bool m_Inited;

        private LLM m_LLM;
        private TTS m_TTS;

        private string m_ResultStr;

        private void Awake()
        {
            Debug.Log($"AINPC_ARS_TAG: Awake");

            m_LLM = GetComponent<LLM>();
            m_TTS = GetComponent<TTS>();
        }

        // Start is called before the first frame update
        private void OnEnable()
        {
            Debug.Log($"AINPC_ARS_TAG: OnEnable");

            if (Application.platform != RuntimePlatform.Android) return;
            if (m_Inited) return;

            // CoreService.Initialize();
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Permission.RequestUserPermission(Permission.Microphone);
            }

            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);
            }

            m_SpeechToText.SpeechEvent.AddListener((result) =>
            {
                m_ResultStr = result;
                m_UiController.ShowArsToUI.Invoke(result);
            });

            m_SpeechToText.SpeechEndEvent.AddListener(() =>
            {
                AiResponseDebuger.Log(AiResponseDebuger.EState.OnArsEndEvent);

                Debug.Log($"AINPC_ARS_TAG: text={m_ResultStr} isFinal");
                m_TTS.StartAll();

                m_UiController.ShowArsToUI.Invoke(m_ResultStr);
                m_LLM.Send(m_ResultStr);

                m_ResultStr = "";
                Invoke(nameof(HideArsTriggerBtn), 0.5f);
                m_SpeechToText.StopSpeech();
            });

            m_SpeechToText.SpeechInitEvent.AddListener((code) =>
            {
                if (code == 0)
                {
                    m_Inited = true;

                    Debug.Log("AINPC_ARS_TAG: Init engine successfully.");
                    m_UiController.ShowArsToUI.Invoke($"Init engine successfully.");
                }
                else
                {
                    m_Inited = false;
                    Debug.Log($"AINPC_ARS_TAG: Init ASR Engine failed :{code}");
                    m_UiController.ShowArsToUI.Invoke($"Init ASR Engine failed :{code}");
                }
            });

            m_SpeechToText.Init();
        }

        private void OnDisable()
        {
            if (Application.platform != RuntimePlatform.Android) return;
            // SpeechService.SetOnAsrResultCallback(null);
            // SpeechService.SetOnSpeechErrorCallback(null);
            // SpeechService.StopAsr();
            m_SpeechToText.StopSpeech();
            m_SpeechToText.StopEngine();
            m_Inited = false;
        }

        private void Update()
        {
            if (Keyboard.current.vKey.wasPressedThisFrame)
            {
                m_TTS.StopAll();
                m_LLM.StopAll();
            }
        }

        public bool StartArsServer()
        {
            if (!CanWork()) return false;

            m_SpeechToText.StartSpeech();
            Debug.Log($"AINPC_ARS_TAG: engine started, {m_AutoStop}, {m_ShowPunctual}, {m_MaxDuration}");
            m_UiController.ShowArsToUI.Invoke($"engine started, {m_AutoStop}, {m_ShowPunctual}, {m_MaxDuration}");
            return true;
        }

        public void HideArsTriggerBtn()
        {
            m_UiController.HideArsTriggerBtn.Invoke();
        }

        private void StopArs()
        {
            if (!CanWork()) return;
            // SpeechService.StopAsr();
            m_SpeechToText.StopSpeech();
            Debug.Log("AINPC_ARS_TAG: engine stopped");
        }

        private bool CanWork()
        {
            if (!m_Inited)
            {
                Debug.Log($"AINPC_ARS_TAG: Please init before start ASR");
                m_UiController.ShowArsToUI.Invoke($"Please init before start ASR");
                return false;
            }

            return true;
        }
    }
}