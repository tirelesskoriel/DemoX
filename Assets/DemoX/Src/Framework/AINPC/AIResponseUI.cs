using DemoX.Framework.Bridge.Event;
using TMPro;
using UnityEngine;

namespace DemoX.Framework.AINPC
{
    public class AIResponseUI : MonoBehaviour
    {
        [SerializeField] private ECAINPC _ecainpc;
        [SerializeField] private Transform _uiRoot;
        [SerializeField] private TMP_Text _resultText;
        [SerializeField] private TMP_Text _timeDebugText;
        [SerializeField] private ARSTrigger _arsTrigger;


        private int m_DebugMsgCounter;
        private string m_DebugMsg;

        private bool _llmStartFlag;
        private string m_OutputMsg;

        private void OnArs(string text)
        {
            ShowUI();
            _resultText.text = $"\n{text}";
            CancelInvoke(nameof(HideUI));
        }

        private void OnLLM(string text)
        {
            if (!_llmStartFlag)
            {
                _llmStartFlag = true;
                m_OutputMsg = "";
            }

            if (_resultText.text.Length > 200)
            {
                m_OutputMsg = "";
            }

            m_OutputMsg += text;
            _resultText.text = $"{m_OutputMsg}";
        }

        public void OnLLMEnd()
        {
            CancelInvoke();
            _llmStartFlag = false;
            _arsTrigger.gameObject.SetActive(true);
            Invoke(nameof(HideUI), 2.0f);
        }

        private void HideUI()
        {
            UIVisible(false);
        }

        private void ShowUI()
        {
            CancelInvoke();
            UIVisible(true);
        }

        private void UIVisible(bool visible)
        {
            if (_uiRoot)
            {
                _uiRoot.gameObject.SetActive(visible);
            }
        }

        private void OnARSStart()
        {
            // if (AINPC.Ins.Ars.StartArsServer())
            // {
            //     _arsTrigger.gameObject.SetActive(false);
            // }
            
        }

        private void OnARSEnd()
        {
            _arsTrigger.gameObject.SetActive(true);
        }

        private void OnEnable()
        {
            _arsTrigger.ARSTriggerEvent.AddListener(OnARSStart);
            _ecainpc.HideArsTriggerBtn.AddListener(OnARSEnd);
            _ecainpc.ShowArsToUI.AddListener(OnArs);
            _ecainpc.ShowLlmResultToUI.AddListener(OnLLM);
            _ecainpc.HideUI.AddListener(OnLLMEnd);
            _ecainpc.OnDebug.AddListener(OnDebug);
        }

        private void OnDisable()
        {
            _arsTrigger.ARSTriggerEvent.RemoveListener(OnARSStart);

            _ecainpc.HideArsTriggerBtn.RemoveListener(OnARSEnd);
            _ecainpc.ShowArsToUI.RemoveListener(OnArs);
            _ecainpc.ShowLlmResultToUI.RemoveListener(OnLLM);
            _ecainpc.HideUI.RemoveListener(OnLLMEnd);
            _ecainpc.OnDebug.RemoveListener(OnDebug);

        }

        private void OnDebug(int stage, string debugMsg)
        {
            //
            // if (stage == 0)
            // {
            //     m_DebugMsg = "";
            //     m_DebugMsgCounter = 0;
            // }
            //
            // if (m_DebugMsgCounter > stage) return;
            // m_DebugMsgCounter += 1;
            //
            // float time = Time.realtimeSinceStartup;
            // switch (stage)
            // {
            //     case 0:
            //         m_DebugMsg += $"Speak End: {time}\n";
            //         break;
            //     case 1:
            //         m_DebugMsg += $"First LLM: {time}\n";
            //         break;
            //     case 2:
            //         m_DebugMsg += $"First Req TTS: {time}\n";
            //         break;
            //     case 3:
            //         m_DebugMsg += $"First Resp TTS: {time}\n";
            //         break;
            //     case 4:
            //         m_DebugMsg += $"First Valid Resp TTS: {time}\n";
            //         break;
            // }
            //
            // _timeDebugText.text = m_DebugMsg;
        }
    }
}