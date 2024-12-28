using UnityEngine;
using UnityEngine.InputSystem;

namespace DemoX.Framework
{
    public class HandMotion : MonoBehaviour
    {
        [SerializeField] private SODebugKeyboardKey _debugKeyboard;

        private int _startFlag;
        private int _startFlagH;

        private void Update()
        {
            if (Application.platform == RuntimePlatform.WindowsEditor && _debugKeyboard)
            {
                if (_debugKeyboard.HandMotionW.WasPressedThisFrame())
                {
                    _startFlag = _startFlag == 1 ? 0 : 1;
                }
                else if (_debugKeyboard.HandMotionS.WasPressedThisFrame())
                {
                    _startFlag = _startFlag == -1 ? 0 : -1;
                }

                if (_startFlag != 0)
                {
                    transform.RotateAround(transform.position, transform.right, _startFlag * 10 * Time.deltaTime);
                }

                if (_debugKeyboard.HandMotionA.WasPressedThisFrame())
                {
                    _startFlagH = _startFlagH == 1 ? 0 : 1;
                }
                else if (_debugKeyboard.HandMotionD.WasPressedThisFrame())
                {
                    _startFlagH = _startFlagH == -1 ? 0 : -1;
                }

                if (_startFlagH != 0)
                {
                    transform.RotateAround(transform.position, transform.up, _startFlagH * 10 * Time.deltaTime);
                }
            }
        }
    }
}