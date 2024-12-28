using System;
using DemoX.Framework.AINPC;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DemoX.Framework
{
    public class Timestamp : MonoBehaviour
    {
        [SerializeField] private string m_Tag;
        [SerializeField] private string m_LoggerHeader;
        [SerializeField] private Button m_Button;
        [SerializeField] private TMP_Text m_TimestampText;

        private void Awake()
        {
            m_Button.onClick.AddListener(() =>
            {
                // Debug.Log($"{m_Tag} => {m_LoggerHeader} : {Time.realtimeSinceStartup}");
                m_TimestampText.text = $"{m_Tag} => {m_LoggerHeader} : {Time.realtimeSinceStartup}";
                
                AiResponseDebuger.Log(AiResponseDebuger.EState.OnSpeechEnd);

            });
        }
        
    }
}