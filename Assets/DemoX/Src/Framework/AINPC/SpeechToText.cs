using System.Text;
using System.Threading;
using LitJson;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;

namespace DemoX.Framework.AINPC
{
    public class SpeechToText : MonoBehaviour
    {
        [SerializeField] private UnityEvent<string> m_SpeechEvent;
        [SerializeField] private UnityEvent m_SpeechEndEvent;
        [SerializeField] private UnityEvent<int> m_SpeechErrorEvent;
        [SerializeField] private UnityEvent<int> m_SpeechInitEvent;

        public UnityEvent<string> SpeechEvent => m_SpeechEvent;
        public UnityEvent SpeechEndEvent => m_SpeechEndEvent;
        public UnityEvent<int> SpeechInitEvent => m_SpeechInitEvent;
        public UnityEvent<int> SpeechErrorEvent => m_SpeechErrorEvent;

        private AndroidJavaObject m_SpeechEngine;
        private AndroidJavaClass m_SpeechEngineDefines;
        private long m_EngineHandler;

        private SynchronizationContext m_MainContext;

        private CallbackHandler m_CallbackHandler;

        private void Awake()
        {
            AiResponseDebuger.Init();
            m_MainContext = SynchronizationContext.Current;
            m_CallbackHandler = new CallbackHandler(m_MainContext, m_SpeechEvent, m_SpeechEndEvent, m_SpeechInitEvent,
                m_SpeechErrorEvent);
        }

        private static readonly int MESSAGE_TYPE_RECORDER_END = 1701;
        private static readonly int MESSAGE_TYPE_PARTIAL_RESULT = 1201;
        private static readonly int MESSAGE_TYPE_FINAL_RESULT = 1204;

        class CallbackHandler : AndroidJavaProxy
        {
            public UnityEvent<string> speechEvent;
            public UnityEvent<int> speechInitEvent;
            public UnityEvent speechEndEvent;
            public UnityEvent<int> speechErrorEvent;

            private SynchronizationContext Context;

            private string m_LastResult;
            private int m_SameResultCounter;
            private string m_FinishReqId;

            public CallbackHandler(SynchronizationContext context, UnityEvent<string> resultEvent, UnityEvent endEvent,
                UnityEvent<int> initEvent,
                UnityEvent<int> errorEvent) : base(
                "com.zstech.speedlibrary.SpeechHelper$SpeechCallback")
            {
                Context = context;
                speechEvent = resultEvent;
                speechEndEvent = endEvent;
                speechInitEvent = initEvent;
                speechErrorEvent = errorEvent;
            }

            public void OnInit(int code)
            {
                Context.Post((_) => { speechInitEvent.Invoke(code); }, null);
            }

            public void OnResult(int type, byte[] data, int len)
            {
                // Debug.Log($"ARS_RESULT_TTAG: {type} ====================== {Encoding.UTF8.GetString(data)}");

                string finalResult = "";
                string reqID = "";

                if (type == MESSAGE_TYPE_RECORDER_END)
                {
                    AiResponseDebuger.Log(AiResponseDebuger.EState.OnArsEnd);
                }
                else if (type == MESSAGE_TYPE_PARTIAL_RESULT || type == MESSAGE_TYPE_FINAL_RESULT)
                {
                    AiResponseDebuger.Log(AiResponseDebuger.EState.OnArsRunning);

                    if (data == null || data.Length <= 0) return;
                    string result = Encoding.UTF8.GetString(data);
                    if (string.IsNullOrEmpty(result)) return;
                    JsonData jsonData = JsonMapper.ToObject(result);
                    
                    if (jsonData.ContainsKey("result") && jsonData["result"].IsArray)
                    {
                        finalResult = jsonData["result"][0]["text"].ToString();
                    }
                    
                    if (jsonData.ContainsKey("reqid") && jsonData["reqid"].IsString)
                    {
                        reqID = jsonData["reqid"].ToString();
                    }
                }
                else
                {
                    AiResponseDebuger.Log(AiResponseDebuger.EState.OnArsOther);
                }

                Context.Post((_) =>
                {
                    if (type == MESSAGE_TYPE_RECORDER_END)
                    {
                        if (!string.Equals(m_FinishReqId, reqID))
                        {
                            m_LastResult = null;
                            m_SameResultCounter = 0;
                            speechEndEvent.Invoke();
                        }
                    }
                    else if (type == MESSAGE_TYPE_PARTIAL_RESULT || type == MESSAGE_TYPE_FINAL_RESULT)
                    {
                        if (!string.IsNullOrEmpty(m_FinishReqId) && string.Equals(m_FinishReqId, reqID))
                        {
                            return;
                        }
                        
                        speechEvent.Invoke(finalResult);
                        if (!string.IsNullOrEmpty(finalResult) && string.Equals(m_LastResult, finalResult))
                        {
                            m_SameResultCounter++;
                        }
                        else
                        {
                            m_SameResultCounter = 0;
                        }

                        m_LastResult = finalResult;

                        if (m_SameResultCounter >= 5)
                        {
                            m_FinishReqId = reqID;

                            m_LastResult = null;
                            m_SameResultCounter = 0;
                            speechEndEvent.Invoke();
                        }
                    }
                    else
                    {
                        speechErrorEvent.Invoke(type);
                    }
                }, null);
            }
        }

        public void Init()
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Permission.RequestUserPermission(Permission.Microphone);
            }

            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);
            }

            using AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            m_SpeechEngine = new AndroidJavaObject("com.zstech.speedlibrary.SpeechHelper");
            m_SpeechEngine.Call("Prepare", currentActivity);
            m_SpeechEngine.Call("Setup",
                "7195039993",
                "te47Q5hnTa3QSFEiwuWzhXvq5lCNgEsq",
                "volcengine_streaming",
                "uid_1234",
                "did_5678",
                true,
                false,
                false,
                true,
                m_CallbackHandler);
        }

        public void StartSpeech()
        {
            m_SpeechEngine.Call("StartSpeech");
        }

        public void StopSpeech()
        {
            m_SpeechEngine.Call("StopSpeech");
        }

        public void StopEngine()
        {
            m_SpeechEngine.Call("StopEngine");
            Release();
        }

        private void Release()
        {
            m_SpeechEngine.Dispose();
        }
    }
}