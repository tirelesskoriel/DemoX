namespace DemoX.Framework.AINPC
{
    public static class ARSRequestConstant
    {
        public static readonly string URL = "wss://openspeech.bytedance.com/api/v1/tts/ws_binary";
        public static readonly string AppId = "7195039993";
        public static readonly string Token = "te47Q5hnTa3QSFEiwuWzhXvq5lCNgEsq";
        public static readonly string Cluster = "volcano_tts";
        public static readonly string VoidType = "BV009_streaming";

        public static readonly string RequestJson = @"
{
    ""app"": {
        ""appid"": """",
        ""token"": """",
        ""cluster"": """"
    },
    ""user"": {
        ""uid"": ""388808088185088""
    },
    ""audio"": {
        ""format"": ""wav"",
        ""rate"": 16000,
        ""bits"": 16,
        ""channel"": 1,
        ""language"": ""zh-CN""
    },
    ""request"": {
        ""reqid"": ""a3273f8ee3db11e7bf2ff3223ff33638"",
        ""workflow"": ""audio_in,resample,partition,vad,fe,decode"",
        ""sequence"": 1,
        ""nbest"": 1,
        ""show_utterances"": true
    }
}";
    }
}