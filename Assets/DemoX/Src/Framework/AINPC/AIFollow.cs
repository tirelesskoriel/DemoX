using System;
using DemoX.Framework.Bridge.Event;
using TMPro;
using UnityEngine;

namespace DemoX.Framework.AINPC
{
    public class AIFollow : MonoBehaviour
    {
        [SerializeField] private Transform _player;
        [SerializeField] private Transform _target;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private float _speed;

        [SerializeField] private Transform _reponseUI;
        

        private void Update()
        {
            Motion();
            ToPlayer();
        }

        private void ToPlayer()
        {
            if (!_player || !_reponseUI) return;
            Vector3 direction = Vector3.Normalize(transform.position - _player.position);
            transform.forward = direction;
        }

        private void Motion()
        {
            if (!_player || !_target) return;

            Vector3 pos = transform.position;
            Vector3 targetPos = _target.position;
            Vector3 playerPos = _player.position;
            
            bool isHit = Physics.Raycast(playerPos,
                Vector3.Normalize(targetPos - playerPos),
                out RaycastHit hitInfo,
                10.0f,
                _layerMask);

            targetPos = isHit ? hitInfo.point : targetPos;
            
            if (Vector3.Distance(pos, targetPos) < 0.1f) return;

            Vector3 direction = Vector3.Normalize(targetPos - pos);
            transform.position = Time.deltaTime * _speed * direction + pos;
        }
    }
}