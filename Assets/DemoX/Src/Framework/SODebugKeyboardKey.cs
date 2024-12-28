using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace DemoX.Framework
{
    [CreateAssetMenu(menuName = "Settings/DebugKeyboardKey", fileName = "SO_DebugKeyboardKey")]
    public class SODebugKeyboardKey : ScriptableObject
    {
        // hand pose
        public Key DirectPickScene;
        public Key LeftWatchPose;
        public Key Pinching;
        public Key DoubleTap;
        public Key Fisting;
        
        // scene manager
        public Key ShowItems;
        public Key StartGame;
        public Key Reset;
        
        // hand motion
        public Key HandMotionW;
        public Key HandMotionA;
        public Key HandMotionS;
        public Key HandMotionD;
    }

    public static class KeyExt
    {
        public static bool WasPressedThisFrame(this Key key)
        {
            return Keyboard.current[key].wasPressedThisFrame;
        }
    }
}