using UnityEngine;

namespace DemoX.Framework
{
    public class MotionTest : MonoBehaviour
    {
        [SerializeField] private float _speed;
        private Animator _animator;
        private static readonly int Speed = Animator.StringToHash("Speed");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        void Update()
        {
            _animator.SetFloat(Speed, _speed);
        }
    }
}