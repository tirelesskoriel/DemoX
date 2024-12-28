using System.Collections.Generic;
using LitJson;

namespace DemoX.Framework.AINPC
{
    public static class TTSRequestConstant
    {
        public static readonly Dictionary<int, string> MessageTypes = new()
        {
            { 11, "audio-only server response" },
            { 12, "frontend server response" },
            { 15, "error message from server" }
        };

        public static readonly Dictionary<int, string> MessageTypeSpecificFlags = new()
        {
            { 0, "no sequence number" },
            { 1, "sequence number > 0" },
            { 2, "last message from server (seq < 0)" },
            { 3, "sequence number < 0" }
        };

        public static readonly Dictionary<int, string> MessageSerializationMethods = new()
        {
            { 0, "no serialization" }, { 1, "JSON" }, { 15, "custom type" }
        };

        public static readonly Dictionary<int, string> MessageCompressions = new()
        {
            { 0, "no compression" }, { 1, "gzip" }, { 15, "custom compression method" }
        };

        public static readonly string URL = "wss://openspeech.bytedance.com/api/v1/tts/ws_binary";
        public static readonly string AppId = "7195039993";
        public static readonly string Token = "te47Q5hnTa3QSFEiwuWzhXvq5lCNgEsq";
        public static readonly string Cluster = "volcano_tts";
        public static readonly string VoidType = "BV009_streaming";

        public static readonly string RequestJson = @"
        {
            ""app"": {
                ""appid"": ""7195039993"",
                ""token"": ""access_token"",
                ""cluster"": ""volcano_tts""
            },
            ""user"": {
                ""uid"": ""zstech_ainpc_test""
            },
            ""audio"": {
                ""voice_type"": ""BV009_streaming"",
                ""encoding"": ""pcm"",
                ""speed_ratio"": 1.0,
                ""volume_ratio"": 1.0,
                ""pitch_ratio"": 1.0
            },
            ""request"": {
                ""reqid"": ""xxx"",
                ""text"": ""欢迎来到再伸科技朋友们"",
                ""text_type"": ""plain"",
                ""operation"": ""submit""
            }
        }";

        private static TTSRequestData _ttsRequestData;

        public static TTSRequestData GetTTSRequest()
        {
            return _ttsRequestData ?? JsonMapper.ToObject<TTSRequestData>(RequestJson);
        }
    }

    public class TTSRequestData
    {
        public App app;
        public User user;
        public Audio audio;
        public Request request;

        public string ToJson()
        {
            return JsonMapper.ToJson(this);
        }
    }

    public class App
    {
        public string appid;
        public string token;
        public string cluster;
    }

    public class User
    {
        public string uid;
    }

    public class Audio
    {
        public string voice_type;
        public string encoding;
        public float speed_ratio;
        public float volume_ratio;
        public float pitch_ratio;
    }

    public class Request
    {
        public string reqid;
        public string text;
        public string text_type;
        public string operation;
    }
}