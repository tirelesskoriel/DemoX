using DemoX.Framework.Core;
using DemoX.Framework.Level;
using Mirror;
using UnityEngine;

namespace DemoX.Framework
{
    public class MenuAnim : NetworkBehaviour
    {
        [SerializeField] private Animator _animator;

        [SerializeField] private Transform _leftMenu;
        [SerializeField] private Transform _rightMenu;

        private bool _bNext;
        private static readonly int NextAnimKey = Animator.StringToHash("Next");

        public void Next(bool next)
        {
            XRLogger.Log($"MenuAnimNext client: {_bNext}");
            CmdNext(next);
        }

        [Command(requiresAuthority = false)]
        public void CmdNext(bool next)
        {
            Game.Log($"MenuAnimNext: {_bNext}");
            Transform enableMenu = next ? _rightMenu : _leftMenu;
            Transform disableMenu = next ? _leftMenu : _rightMenu;
            foreach (var handleable in enableMenu.GetComponentsInChildren<Handleable>())
            {
                handleable.enabled = true;
            }

            foreach (var handleable in disableMenu.GetComponentsInChildren<Handleable>())
            {
                handleable.enabled = false;
            }

            if (next != _bNext)
            {
                _bNext = next;
                _animator.SetBool(NextAnimKey, _bNext);
            }
        }
    }
}