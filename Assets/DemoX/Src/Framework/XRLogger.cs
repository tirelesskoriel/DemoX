using System.Text;
using DemoX.Framework.Core;
using Mirror;
using TMPro;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.UI;

namespace DemoX.Framework
{
    public class XRLogger : MonoBehaviour
    {
        public static XRLogger Ins;
        private static XRLogger _internalIns;

        [SerializeField] private TextMeshProUGUI _loggerConstText;
        [SerializeField] private TextMeshProUGUI _loggerText;
        [SerializeField] private ScrollRect _scrollView;

        [SerializeField] private TextMeshProUGUI _netStatisticsText;

        [SerializeField] private TextMeshProUGUI _pinchStrengthTextR;
        [SerializeField] private TextMeshProUGUI _pinchStrengthTextL;

        [SerializeField] private Image _leftPinchFlag;
        [SerializeField] private Image _rightPinchFlag;

        private readonly StringBuilder _loggerSb = new();

        private Image _lastPinchFlag;

        public static void Init()
        {
            Ins = _internalIns;
        }

        private void Awake()
        {
            if (!_internalIns)
            {
                _internalIns = this;
            }
        }

        private void Update()
        {
            if (_netStatisticsText)
            {
                _netStatisticsText.text = NetworkStatistics.GetSentPacketsInfo() + "\n" +
                                          NetworkStatistics.GetReceivedPacketsInfo();
            }
        }

        public static void Pinch(bool val, HandType handType)
        {
            if (!Ins) return;
            Ins.PinchInternal(val, handType);
        }

        public static void Pinch(float val, HandType handType)
        {
            if (!Ins) return;
            Ins.PinchInternal(val, handType);
        }

        public static void Log(string msg)
        {
            if (!Ins) return;
            Ins.LogInternal(msg);
        }

        public void PinchInternal(bool val, HandType handType)
        {
            Image image = handType == HandType.HandRight ? _rightPinchFlag : _leftPinchFlag;
            if (image)
            {
                image.enabled = val;
            }
        }

        public void PinchInternal(float val, HandType handType)
        {
            TextMeshProUGUI textUI = handType == HandType.HandRight ? _pinchStrengthTextR : _pinchStrengthTextL;
            if (textUI)
            {
                textUI.text = val.ToString();
            }
        }

        public void LogInternal(string msg)
        {
            Game.Log(msg);
            if (!_loggerText) return;
            _loggerSb.AppendLine($"[{Time.frameCount}]: {msg}");
            if (_loggerSb.Length > 600)
            {
                _loggerSb.Remove(0, 300);
            }

            _loggerText.SetText(_loggerSb);
            _scrollView.velocity = new Vector2(0.0f, 1000.0f);
        }

        public void LogConstant(string msg)
        {
            if (!_loggerConstText) return;

            _loggerConstText.text = msg;
        }
    }
}