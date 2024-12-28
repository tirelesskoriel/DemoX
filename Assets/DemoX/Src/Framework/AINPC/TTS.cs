using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Best.HTTP;
using Best.HTTP.Shared.Compression.Zlib;
using Best.HTTP.Shared.PlatformSupport.Memory;
using Best.WebSockets;
using DemoX.Framework.Bridge.Event;
using DemoX.Framework.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DemoX.Framework.AINPC
{
    public class TTS : MonoBehaviour
    {
        [SerializeField] private ECAINPC _ecainpc;
        [SerializeField] private AudioSource m_Audio;
        [SerializeField] private string m_DebugText;
        private WebSocket m_WebSocket;

        private readonly byte[] m_HeaderBuffer = { 0x11, 0x10, 0x10, 0x00 };
        private readonly byte[] m_PayloadSizeBuffer = new byte[4];

        private readonly StringBuilder m_Sb = new();
        private readonly byte[] m_PayloadInfoBuffer = new byte[4];

        private bool m_IsPlayingLastFrame;
        private Queue<AudioClip> m_AudioClips = new();

        private bool m_Stopped = true;
        private bool m_IsSendEnd;
        private bool m_IsFresh;
        public bool IsFresh => m_IsFresh;
        public bool IsPlaying => m_Audio && m_Audio.isPlaying && m_AudioClips.Count > 0;

        private ARS m_Ars;

        public Action StoppedAction;

        private struct ResponseData
        {
            public int headerSize;
            public int messageType;
            public int messageTypeSpecificFlags;
            public int sequenceNumber;
            public uint payloadSize;
        }

        public void StopAll()
        {
            m_Stopped = true;

            if (m_IsSendEnd)
            {
                ReleaseWS();
                ReleaseCache();
            }
        }

        public void StartAll()
        {
        }
        
        private void ReleaseCache()
        {
            m_IsPlayingLastFrame = false;
            m_AudioClips.Clear();
            if (m_Audio.isPlaying)
            {
                m_Audio.Stop();
            }
        }

        private void Send()
        {
            Send(m_DebugText);
        }

        public void Send(string message, bool isLast = false)
        {
            m_IsSendEnd = isLast;
            if (m_Stopped)
            {
                StopAll();
            }
            AiResponseDebuger.Log(AiResponseDebuger.EState.OnSendToTTS);

            Game.Log($"AINPC_TTS_TAG: tts Send =>{m_Stopped} {message}");
            if (m_Stopped || string.IsNullOrEmpty(message) || m_WebSocket == null) return;
            _ecainpc.OnDebug.Invoke(2, "");

            TTSRequestData jsonData = TTSRequestConstant.GetTTSRequest();
            jsonData.request.text = message;
            jsonData.request.reqid = Guid.NewGuid().ToString();
            string payloadContent = jsonData.ToJson();

            Game.Log($"AINPC_TTS_TAG: tts req json => {Regex.Unescape(payloadContent)}");

            // version: b0001 (4 bits)
            // header size: b0001 (4 bits)
            // message type: b0001 (Full client request) (4bits)
            // message type specific flags: b0000 (none) (4bits)
            // message serialization method: b0001 (JSON) (4 bits)
            // message compression: b0001 (gzip) (4bits)
            // reserved data: 0x00 (1 byte)
            // default_header = bytearray(b'\x11\x10\x11\x00')
            int payloadSize = Encoding.UTF8.GetByteCount(payloadContent);
            int packageSize = m_HeaderBuffer.Length + m_PayloadSizeBuffer.Length + payloadSize;
            var packageBuffer = BufferPool.Get(packageSize, true);

            int infoOffset = m_HeaderBuffer.Length + m_PayloadSizeBuffer.Length;
            Encoding.UTF8.GetBytes(payloadContent, 0, payloadContent.Length, packageBuffer, infoOffset);

            Array.Copy(m_HeaderBuffer, 0, packageBuffer, 0, m_HeaderBuffer.Length);

            BitConverter.TryWriteBytes(m_PayloadSizeBuffer, payloadSize);
            Reverse(m_PayloadSizeBuffer);
            Array.Copy(
                m_PayloadSizeBuffer, 0,
                packageBuffer, m_HeaderBuffer.Length,
                m_PayloadSizeBuffer.Length);
            
            m_WebSocket.SendAsBinary(new BufferSegment(packageBuffer, 0, packageSize));
            m_IsFresh = false;
        }

        private void Reverse(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
        }

        private void OnEnable()
        {
            ReleaseCache();
            InitWS();
        }

        private void OnDisable()
        {
            ReleaseWS(true);
            ReleaseCache();
        }

        private void InitWS()
        {
            m_WebSocket = new WebSocket(new Uri(TTSRequestConstant.URL));
            m_WebSocket.OnOpen += OnWebSocketOpen;
            m_WebSocket.OnInternalRequestCreated += OnInternalRequestCreated;
            m_WebSocket.OnBinary += OnBinaryMessageReceived;
            m_WebSocket.OnMessage += OnMessageReceived;
            m_WebSocket.OnClosed -= OnWebSocketClosed;
            m_WebSocket.OnClosed += OnWebSocketClosed;

            m_WebSocket.Open();
        }

        private void ReleaseWS(bool removeCloseEvent = false)
        {
            m_WebSocket.OnOpen -= OnWebSocketOpen;
            m_WebSocket.OnInternalRequestCreated -= OnInternalRequestCreated;
            m_WebSocket.OnBinary -= OnBinaryMessageReceived;
            m_WebSocket.OnMessage -= OnMessageReceived;
            if (removeCloseEvent)
            {
                m_WebSocket.OnClosed -= OnWebSocketClosed;
            }
            m_WebSocket.Close();
            m_WebSocket = null;
        }

        private void OnWebSocketOpen(WebSocket webSocket)
        {
            Debug.Log("AINPC_TTS_TAG: WebSocket is now Open!");
            m_IsFresh = true;
            m_Stopped = false;
            m_IsSendEnd = false;
        }

        public void OnInternalRequestCreated(WebSocket webSocket, HTTPRequest httpRequest)
        {
            httpRequest.SetHeader("Authorization", $"Bearer;{TTSRequestConstant.Token}");
        }

        private void OnMessageReceived(WebSocket webSocket, string message)
        {
            Game.Log($"AINPC_TTS_TAG: OnMessageReceived => {message}");
        }

        private void OnBinaryMessageReceived(WebSocket webSocket, BufferSegment buffer)
        {
            AiResponseDebuger.Log(AiResponseDebuger.EState.OnReceivedFromTTS);

            Game.Log($"AINPC_TTS_TAG: OnBinaryMessageReceived => ");

            byte[] res = buffer.Data;
            RetrieveData(res, !Application.isMobilePlatform, out ResponseData responseData);

            int headerSize = responseData.headerSize;
            int messageType = responseData.messageType;
            int messageTypeSpecificFlags = responseData.messageTypeSpecificFlags;

            _ecainpc.OnDebug.Invoke(3, "");
            if (messageType == 0xb && messageTypeSpecificFlags != 0 && responseData.payloadSize > 0)
            {
                _ecainpc.OnDebug.Invoke(4, "");
                const int payloadInfoSize = 8;
                int infoSize = headerSize + payloadInfoSize;
                AudioClip audioClip = CacheAudio(res, infoSize, buffer.Count - infoSize);
                Play(audioClip);
            }
        }

        private void RetrieveData(byte[] res, bool showDebug, out ResponseData responseData)
        {
            responseData = new ResponseData();
            int protocolVersion = res[0] >> 4;
            int headerSize = (res[0] & 0x0f) * 4;
            int messageType = res[1] >> 4;
            int messageTypeSpecificFlags = res[1] & 0x0f;
            int serializationMethod = res[2] >> 4;
            int messageCompression = res[2] & 0x0f;
            int reserved = res[3];

            responseData.headerSize = headerSize;
            responseData.messageType = messageType;
            responseData.messageTypeSpecificFlags = messageTypeSpecificFlags;

            int sequenceNumber = 0;
            uint payloadSize = 0;

            if (messageType == 0xb) // audio-only server response
            {
                if (messageTypeSpecificFlags != 0) // no sequence number as ACK
                {
                    const int payloadOffset = 4;
                    Array.Copy(res, payloadOffset, m_PayloadInfoBuffer, 0, m_PayloadInfoBuffer.Length);
                    Reverse(m_PayloadInfoBuffer);
                    sequenceNumber = BitConverter.ToInt32(m_PayloadInfoBuffer);
                    responseData.sequenceNumber = sequenceNumber;

                    const int payloadSizeOffset = 4;
                    Array.Copy(res, payloadOffset + payloadSizeOffset, m_PayloadInfoBuffer, 0,
                        m_PayloadInfoBuffer.Length);
                    Reverse(m_PayloadInfoBuffer);
                    payloadSize = BitConverter.ToUInt32(m_PayloadInfoBuffer);
                    responseData.payloadSize = payloadSize;
                }
            }

            if (!showDebug) return;

            m_Sb.AppendLine("--------------------------- response ---------------------------");
            m_Sb.AppendLine($"Protocol version: " +
                            $"0x{protocolVersion:X} - version {protocolVersion}");
            m_Sb.AppendLine($"Header size: " +
                            $"0x{headerSize:X} - {headerSize} bytes");
            m_Sb.AppendLine($"Message type: " +
                            $"0x{messageType:X} - {TTSRequestConstant.MessageTypes[messageType]}");
            m_Sb.AppendLine($"Message type specific flags: " +
                            $"0x{messageTypeSpecificFlags:X} - {TTSRequestConstant.MessageTypeSpecificFlags[messageTypeSpecificFlags]}");
            m_Sb.AppendLine($"Message serialization method: " +
                            $"0x{serializationMethod:X} - {TTSRequestConstant.MessageSerializationMethods[serializationMethod]}");
            m_Sb.AppendLine($"Message compression: " +
                            $"0x{messageCompression:X} - {TTSRequestConstant.MessageCompressions[messageCompression]}");
            m_Sb.AppendLine($"Reserved: " +
                            $"0x{reserved:X4}");

            if (headerSize != 1)
            {
                byte[] headerExtensions = new byte[headerSize];
                Array.Copy(res, 4, headerExtensions, 0, headerExtensions.Length);
                m_Sb.AppendLine($"Header extensions: {BitConverter.ToString(headerExtensions)}");
            }

            m_Sb.AppendLine($"Sequence number: {sequenceNumber}");
            m_Sb.AppendLine($"Payload size: {payloadSize} bytes");

            if (messageType == 0xf)
            {
                int code = BitConverter.ToInt32(res, headerSize);
                int msgSize = BitConverter.ToInt32(res, headerSize + 4);
                byte[] errorMsg = new byte[msgSize];
                Array.Copy(res, headerSize + 8, errorMsg, 0, msgSize);

                if (messageCompression == 1)
                {
                    using (var compressedStream = new MemoryStream(errorMsg))
                    using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                    using (var resultStream = new MemoryStream())
                    {
                        gzipStream.CopyTo(resultStream);
                        errorMsg = resultStream.ToArray();
                    }
                }

                string errorMsgStr = Encoding.UTF8.GetString(errorMsg);
                m_Sb.AppendLine($"Error message code: {code}");
                m_Sb.AppendLine($"Error message size: {msgSize} bytes");
                m_Sb.AppendLine($"Error message: {errorMsgStr}");
            }
            else if (messageType == 0xc)
            {
                int msgSize = BitConverter.ToInt32(res, headerSize);
                byte[] frontendMsg = new byte[msgSize];
                Array.Copy(res, headerSize + 4, frontendMsg, 0, msgSize);

                if (messageCompression == 1)
                {
                    using (var compressedStream = new MemoryStream(frontendMsg))
                    using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                    using (var resultStream = new MemoryStream())
                    {
                        gzipStream.CopyTo(resultStream);
                        frontendMsg = resultStream.ToArray();
                    }
                }

                m_Sb.AppendLine($"Frontend message: {Encoding.UTF8.GetString(frontendMsg)}");
            }
            else if (messageType != 0xb)
            {
                m_Sb.AppendLine("undefined message type!");
            }

            Game.Log(m_Sb.ToString());
            m_Sb.Clear();
        }

        private void OnWebSocketClosed(WebSocket webSocket, WebSocketStatusCodes code, string message)
        {
            if (code == WebSocketStatusCodes.NormalClosure)
            {
                Game.Log($"AINPC_TTS_TAG: WebSocket is now Closed! {message}");
            }
            else
            {
                Game.Log($"AINPC_TTS_TAG: error cause closed! {code} {message}");
            }

            ReleaseCache();
            InitWS();
        }

        private void PrintBinary(byte[] data)
        {
            string dxString = BitConverter.ToString(data);
            Game.Log($"AINPC_TTS_TAG: dx => {dxString}");
            // foreach (byte b in data)
            // {
            //     // 将字节转换为二进制字符串，并用0填充到8位
            //     string binaryString = Convert.ToString(b, 2).PadLeft(8, '0');
            //     Debug.Log(binaryString);
            // }
        }

        public void Update()
        {
            if (Keyboard.current.gKey.wasPressedThisFrame)
            {
                Send();
            }

            Play(null);
        }


        private AudioClip CacheAudio(byte[] pcmData, int offset, int length)
        {
            int sampleRate = 24000;
            int channels = 1;
            int bitDepth = 16;

            return CreateAudioClipFromPCM(pcmData, offset, length, sampleRate, channels, bitDepth);
        }

        private void Play(AudioClip audioClip)
        {
            if (audioClip)
            {
                m_AudioClips.Enqueue(audioClip);
            }

            if (m_IsPlayingLastFrame && !m_Audio.isPlaying && m_AudioClips.Count <= 0)
            {
                m_IsPlayingLastFrame = false;
                StoppedAction?.Invoke();
                ReleaseWS();
                _ecainpc.HideUI.Invoke();
            }

            if (!m_Audio || m_Audio.isPlaying || m_AudioClips.Count <= 0)
            {
                return;
            }

            if (m_AudioClips.TryDequeue(out AudioClip currentAudioClip))
            {
                AiResponseDebuger.Log(AiResponseDebuger.EState.OnTTS_AudioPlay);

                m_IsPlayingLastFrame = true;
                m_Audio.clip = currentAudioClip;
                m_Audio.Play();
            }
        }

        AudioClip CreateAudioClipFromPCM(byte[] pcmData, int offset, int length, int sampleRate, int channels,
            int bitDepth)
        {
            int sampleSize = bitDepth / 8;
            int sampleCount = length / sampleSize;

            float[] samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                int sample = 0;
                int startIndex = i * sampleSize + offset;
                if (bitDepth == 16)
                {
                    sample = BitConverter.ToInt16(pcmData, startIndex);
                }
                else if (bitDepth == 24)
                {
                    sample = BitConverter.ToInt32(
                        new byte[]
                        {
                            0, pcmData[startIndex + 0], pcmData[startIndex + 1], pcmData[startIndex + 2]
                        }, 0);
                }
                else if (bitDepth == 32)
                {
                    sample = BitConverter.ToInt32(pcmData, startIndex);
                }

                samples[i] = sample / (float)short.MaxValue; // 归一化到 -1.0f 到 1.0f
            }

            AudioClip audioClip = AudioClip.Create("PCMClip", sampleCount / channels, channels, sampleRate, false);
            audioClip.SetData(samples, 0);
            return audioClip;
        }
    }
}