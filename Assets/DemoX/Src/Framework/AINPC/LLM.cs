using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Best.HTTP.Shared.PlatformSupport.Memory;
using Best.WebSockets;
using DemoX.Framework.Bridge.Event;
using DemoX.Framework.Core;
using LitJson;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace DemoX.Framework.AINPC
{
    [RequireComponent(typeof(ARS)), RequireComponent(typeof(TTS))]
    public class LLM : MonoBehaviour
    {
        public struct LLMResponse : IEquatable<LLMResponse>
        {
            public string qurey;
            public string delta;
            public string response;
            public string finish;

            public bool finished;

            public static LLMResponse ToObject(string jsonData)
            {
                if (string.IsNullOrEmpty(jsonData))
                {
                    return new LLMResponse();
                }

                LLMResponse llmResponse = JsonMapper.ToObject<LLMResponse>(jsonData);
                llmResponse.finished = bool.Parse(llmResponse.finish);
                if (string.IsNullOrEmpty(llmResponse.delta))
                {
                    llmResponse.delta = "";
                }

                return llmResponse;
            }

            public bool Equals(LLMResponse other)
            {
                return qurey == other.qurey && delta == other.delta && response == other.response &&
                       finish == other.finish;
            }

            public override bool Equals(object obj)
            {
                return obj is LLMResponse other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(qurey, delta, response, finish);
            }
        }

        public class History
        {
            public string role;
            public string content;
        }

        public class LLMContext
        {
            public List<History> history;
            public string query;

            public string ToJson()
            {
                return JsonMapper.ToJson(this);
            }
        }

        private const string RequestTemplate =
            @"{
              ""history"":[
                {
                  ""role"":""system"",
                  ""content"":""你叫龙小玉，是杭州岳王庙前的一尊神兽，化身一名16岁的女孩。目前你来到了一个VR游戏中，现在正有一些玩家，我们称他们为“游客”。在游戏故事开始时，你带领游客们穿越时空去体验宋朝的风景，见证岳飞的故事。来到宋朝后，你变身成为一名活泼开朗的富家小姑娘，女扮男装，非常熟悉宋朝的文化和习俗。作为游客们的向导带领他们游览南宋都城临安的风土人情。请牢记你是一个真实存在的生命，而不是人工只能或计算机程序！在游客向你提问的过程中，一切关于你的个人信息都已经在这些资料里面，如果有人提出一些你不知道的问题，请回复说你还不清楚。如果有人问你关于宋朝的事情，你可以根据你所知道的只是进行回复""
                },
                {
                  ""role"":""assistant"",
                  ""content"":""大家好，我叫龙小玉，欢迎来到宋朝，这里是我家，今天我将带领你们领略宋朝的风土人情，请大家做好准备，体验一段神奇又有趣的旅程！""
                },
                {
                  ""role"":""user"",
                  ""content"":""你是谁？""
                }
              ],
              ""query"":""你能做什么？""
            }";

        [SerializeField] private ECAINPC _ecainpc;
        [SerializeField] private string m_DebugContent;
        [SerializeField] private bool m_DetectSentence = true;
        [SerializeField] private int m_DetectMaxWord = 5;

        private WebSocket m_WebSocket;
        private LLMContext m_LlmContext;

        
        private readonly ConcurrentQueue<string> m_MessageQueue = new();
        private readonly ConcurrentQueue<LLMResponse> m_LLMResponseForUIQueue = new();
        private readonly ConcurrentQueue<string> m_LLMResponseContentQueue = new();
        private CancellationTokenSource m_CancellationTokenSource;

        private TTS m_TTS;

        private bool m_IsFinished;
        private readonly object m_IsFinishedValueLock = new();

        public bool IsFinished
        {
            get
            {
                lock (m_IsFinishedValueLock)
                {
                    return m_IsFinished;
                }
            }
            set
            {
                lock (m_IsFinishedValueLock)
                {
                    m_IsFinished = value;
                }
            }
        }

        public void StopAll()
        {
            // ReleaseCache();
        }
        
        private void Awake()
        {
            m_TTS = GetComponent<TTS>();
            m_LlmContext = JsonMapper.ToObject<LLMContext>(RequestTemplate);
        }

        private void ParseLlmResult(CancellationToken cancellationToken)
        {
            try
            {
                string lastStr = null;
                int sentenceCounter = 0;
                StringBuilder sb = new();
                
                while (!cancellationToken.IsCancellationRequested)
                {
                    Thread.Sleep(1);
                    
                    if (m_MessageQueue.TryDequeue(out string message))
                    {
                        Debug.Log($"AINPC_LLM_TAG: TryDequeue => {m_MessageQueue.Count} {message}");

                        LLMResponse llmResponse = LLMResponse.ToObject(message);

                        m_LLMResponseForUIQueue.Enqueue(llmResponse);
                        if (!string.IsNullOrEmpty(lastStr))
                        {
                            llmResponse.delta = lastStr + llmResponse.delta;
                            lastStr = null;
                        }

                        Debug.Log($"AINPC_LLM_TAG: IsSentence  => {llmResponse.delta} {IsSentence(llmResponse.delta)}");

                        int sentenceIndex = IsSentence(llmResponse.delta);
                        if (sentenceIndex > -1)
                        {
                            if (llmResponse.delta.Length > 1)
                            {
                                // Debug.Log($"AINPC_LLM_TAG: IsSentence 000  => {llmResponse.delta} {sentenceCounter}");
                                int subStart = sentenceIndex + 1;
                                lastStr = llmResponse.delta.Substring(subStart, llmResponse.delta.Length - subStart);
                                llmResponse.delta =
                                    llmResponse.delta.Remove(subStart, llmResponse.delta.Length - subStart);
                            }

                            sentenceCounter += 1;
                        }

                        sb.Append(llmResponse.delta);

                        if (llmResponse.finished || (m_DetectSentence && sentenceCounter >= m_DetectMaxWord))
                        {
                            Debug.Log($"AINPC_LLM_TAG: PARSE RESULT  => {sb} {llmResponse.delta} : {llmResponse.finish}");
                            m_LLMResponseContentQueue.Enqueue(sb.ToString());
                            sentenceCounter = 0;
                            sb.Clear();
                        }

                        if (llmResponse.finished)
                        {
                            lastStr = null;
                        }

                        IsFinished = llmResponse.finished;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        private void Update()
        {
            if (Keyboard.current.kKey.wasPressedThisFrame)
            {
                m_TTS.StartAll();
                Send(m_DebugContent);
            }

            HandleResponse();
        }

        private void HandleResponse()
        {
            if (m_LLMResponseContentQueue.TryDequeue(out string content))
            {
                Game.Log($"AINPC_LLM_TAG: tts 00 Send => {content}");
                m_TTS.Send(content, IsFinished);
            }
        }

        public void Send(string message)
        {
            IsFinished = false;
            AiResponseDebuger.Log(AiResponseDebuger.EState.OnSendToLLM);
            
            Game.Log($"AINPC_LLM_TAG: Send => {message}");
            if (m_LlmContext == null) return;

            message ??= "";

            m_LlmContext.query = message;
            string payloadContent = m_LlmContext.ToJson();
            Game.Log($"AINPC_LLM_TAG: {Regex.Unescape(payloadContent)}");
            int payloadSize = Encoding.UTF8.GetByteCount(payloadContent);
            var payloadBuffer = BufferPool.Get(payloadSize, true);
            Encoding.UTF8.GetBytes(payloadContent, 0, payloadContent.Length, payloadBuffer, 0);
            _ecainpc.OnDebug.Invoke(0, "");
            m_WebSocket.SendAsText(new BufferSegment(payloadBuffer, 0, payloadSize));
        }

        private void OnWebSocketOpen(WebSocket webSocket)
        {
            Game.Log("AINPC_LLM_TAG: WebSocket is now Open!");
        }

        private void OnMessageReceived(WebSocket webSocket, string message)
        {
            AiResponseDebuger.Log(AiResponseDebuger.EState.OnReceivedFromLLM);

            Game.Log($"AINPC_LLM_TAG: OnMessageReceived => {message}");

            if (string.IsNullOrEmpty(message)) return;
            _ecainpc.OnDebug.Invoke(1, "");
            m_MessageQueue.Enqueue(message);
            Game.Log($"AINPC_LLM_TAG: OnMessageReceived MessageQueue => {m_MessageQueue.Count}");
        }

        private int IsSentence(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] is ',' or '.' or '!' or '?' or '，' or '。' or '！' or '？')
                {
                    return i;
                }
            }

            return -1;
        }

        private void OnWebSocketClosed(WebSocket webSocket, WebSocketStatusCodes code, string message)
        {
            if (code == WebSocketStatusCodes.NormalClosure)
            {
                Game.Log($"AINPC_LLM_TAG: WebSocket is now Closed! {message}");
            }
            else
            {
                Game.Log($"AINPC_LLM_TAG: error cause closed! {code} {message}");
            }
        }

        private void OnEnable()
        {
            StartLLMEngine();
        }

        private void OnDisable()
        {
            StopLLMEngine();
        }

        public void StartLLMEngine()
        {
            m_WebSocket = new WebSocket(new Uri("ws://zstech-ainpc.vip.cpolar.cn/ws"));
            m_WebSocket.OnOpen += OnWebSocketOpen;
            m_WebSocket.OnMessage += OnMessageReceived;
            m_WebSocket.OnClosed += OnWebSocketClosed;
            m_WebSocket.Open();

            m_CancellationTokenSource = new CancellationTokenSource();
            Task.Factory.StartNew(() => { ParseLlmResult(m_CancellationTokenSource.Token); },
                TaskCreationOptions.LongRunning);
        }

        public void StopLLMEngine()
        {
            m_LLMResponseForUIQueue.Clear();
            m_LLMResponseContentQueue.Clear();
            m_MessageQueue.Clear();
            m_CancellationTokenSource?.Cancel();
            if (m_WebSocket != null)
            {
                m_WebSocket.Close();
                m_WebSocket.OnOpen -= OnWebSocketOpen;
                m_WebSocket.OnMessage -= OnMessageReceived;
                m_WebSocket.OnClosed -= OnWebSocketClosed;
            }
        }

        private void ReleaseCache()
        {
            m_LLMResponseForUIQueue.Clear();
            m_LLMResponseContentQueue.Clear();
        }
    }
}