using System;
using UnityEngine;

namespace DemoX.Framework
{
    public class RotateWarp : MonoBehaviour
    {
        private void Update()
        {
            Vector3 angle = transform.localRotation.eulerAngles;
            Vector3 newAngle = new Vector3(-angle.y, angle.z, -angle.x - 16.0f);
            transform.localRotation = Quaternion.Euler(newAngle);
        }
    }
}