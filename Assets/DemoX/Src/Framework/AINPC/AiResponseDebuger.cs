using System;
using System.Diagnostics;
using Unity.VisualScripting;
using Debug = UnityEngine.Debug;

namespace DemoX.Framework.AINPC
{
    public static class AiResponseDebuger
    {
        public enum EState
        {
            OnArsRunning = 0,
            OnArsEndEvent,
            OnArsEnd,
            OnArsOther,
            OnSpeechEnd,
            OnSendToLLM,
            OnReceivedFromLLM,
            OnSendToTTS,
            OnReceivedFromTTS,
            OnTTS_AudioPlay
        }

        private static float m_OnSpeechEndTime;
        private static float m_OnTTS_AudioPlayTime;
        private static float m_OnArsEndTime;

        private static float m_OnSendToLLMTime;
        private static float m_OnReceivedFromLLMTime;

        private static float m_OnSendToTTS_Time;
        private static float m_OnReceivedFromTTS_Time;
        private static float m_OnTTS_AudioPlayTTS_Time;

        private static Stopwatch m_Stopwatch;

        public static void Init()
        {
            m_Stopwatch = Stopwatch.StartNew();
        }
        public static void Log(EState state)
        {
            // float time = Time.realtimeSinceStartup;
            float time = (float)m_Stopwatch.Elapsed.TotalSeconds;
            string logger = $"TIME_TO_MEASURE_TAG => {Enum.GetName(typeof(EState), state)} : {time}";
            Debug.Log(logger);

            if (state == EState.OnSpeechEnd)
            {
                m_OnSpeechEndTime = 0.0f;
                m_OnTTS_AudioPlayTime = 0.0f;
                m_OnArsEndTime = 0.0f;
                m_OnSendToLLMTime = 0.0f;
                m_OnReceivedFromLLMTime = 0.0f;
                m_OnSendToTTS_Time = 0.0f;
                m_OnReceivedFromTTS_Time = 0.0f;
            }

            if (m_OnSpeechEndTime == 0 && state == EState.OnSpeechEnd)
            {
                m_OnSpeechEndTime = time;
            }

            if (m_OnTTS_AudioPlayTime == 0 && state == EState.OnTTS_AudioPlay)
            {
                m_OnTTS_AudioPlayTime = time;
            }

            if (m_OnArsEndTime == 0 && state == EState.OnArsEnd)
            {
                m_OnArsEndTime = time;
            }

            if (m_OnSendToLLMTime == 0 && state == EState.OnSendToLLM)
            {
                m_OnSendToLLMTime = time;
            }

            if (m_OnReceivedFromLLMTime == 0 && state == EState.OnReceivedFromLLM)
            {
                m_OnReceivedFromLLMTime = time;
            }

            if (m_OnSendToTTS_Time == 0 && state == EState.OnSendToTTS)
            {
                m_OnSendToTTS_Time = time;
            }

            if (m_OnReceivedFromTTS_Time == 0 && state == EState.OnReceivedFromTTS)
            {
                m_OnReceivedFromTTS_Time = time;
            }

            if (m_OnTTS_AudioPlayTime == 0 && state == EState.OnTTS_AudioPlay)
            {
                m_OnTTS_AudioPlayTime = time;
            }

            if (m_OnSpeechEndTime != 0.0f && m_OnTTS_AudioPlayTime != 0.0f)
            {
                
                Debug.Log(
                    $"TIME_TO_MEASURE_TAG_Final => ars_time:{m_OnArsEndTime - m_OnSpeechEndTime} llm_time:{m_OnReceivedFromLLMTime - m_OnSendToLLMTime} tts_time:{m_OnReceivedFromTTS_Time - m_OnSendToTTS_Time} audio_time:{m_OnTTS_AudioPlayTime - m_OnReceivedFromTTS_Time} final_time:{m_OnTTS_AudioPlayTime - m_OnSpeechEndTime}");
                m_OnSpeechEndTime = 0.0f;
                m_OnTTS_AudioPlayTime = 0.0f;
            }
        }
    }
}