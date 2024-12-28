using System;
using Unity.XR.PXR;
using UnityEngine;

namespace DemoX.Framework
{
    [Serializable]
    public class HandModel
    {
        public SkinnedMeshRenderer HandRenderer;
        public HandType HandType;
        public HandFixedController HandFixedController;
    }
}