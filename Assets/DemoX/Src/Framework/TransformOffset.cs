using UnityEngine;

namespace DemoX.Framework
{
    public class TransformOffset : MonoBehaviour
    {
        [SerializeField] private Vector3 _positionOffset = new(0.0f, -0.71f, 0.0f);

        private void Update()
        {
            transform.position += _positionOffset;
        }
    }
}