using System.Collections.Generic;
using UnityEngine;

namespace DemoX.Framework
{
    public class Border : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float _sideLengthHalf = 0.5f;
        [SerializeField] private Vector2 _range;

        private const float SPACIAL_DIAGONAL_HALF = 0.866025f;
        private const float DIAGONAL_HALF = 0.707107f;

        Vector3 pointA;
        Vector3 pointB;
        Vector3 pointC;
        Vector3 pointD;
        Vector3 pointE;
        Vector3 pointF;
        Vector3 pointG;
        Vector3 pointH;

        Vector3 pointAB;
        Vector3 pointBC;
        Vector3 pointCD;
        Vector3 pointDA;

        Vector3 pointEF;
        Vector3 pointFG;
        Vector3 pointGH;
        Vector3 pointHE;

        Vector3 pointAF;
        Vector3 pointDE;
        Vector3 pointBG;
        Vector3 pointCH;

        Vector3 startPoint;

        private class Line
        {
            public Vector3 Direction;
            public bool bVisible;
        }

        private List<Line> _lines = new();

        private Camera _camera;

        private void Awake()
        {
            for (int i = 0; i < 20; i++)
            {
                _lines.Add(new Line());
            }

            _camera = Camera.main;
        }

        private void Update()
        {
            Vector3 right = _target.right;
            Vector3 up = _target.up;
            Vector3 forward = _target.forward;

            // _lines[0].Direction = (up + right + forward) * _sideLengthHalf;
            // _lines[1].Direction = (up - right + forward) * _sideLengthHalf;
            // _lines[2].Direction = (-up - right + forward) * _sideLengthHalf;
            // _lines[3].Direction = (-up + right + forward) * _sideLengthHalf;
            //
            // _lines[4].Direction = (up + right - forward) * _sideLengthHalf;
            // _lines[5].Direction = (up - right - forward) * _sideLengthHalf;
            // _lines[6].Direction = (-up - right - forward) * _sideLengthHalf;
            // _lines[7].Direction = (-up + right - forward) * _sideLengthHalf;

            _lines[8].Direction = (up + forward) * _sideLengthHalf;
            _lines[9].Direction = (-right + forward) * _sideLengthHalf;
            _lines[10].Direction = (-up + forward) * _sideLengthHalf;
            _lines[11].Direction = (right + forward) * _sideLengthHalf;

            _lines[12].Direction = (up - forward) * _sideLengthHalf;
            _lines[13].Direction = (-right - forward) * _sideLengthHalf;
            _lines[14].Direction = (-up - forward) * _sideLengthHalf;
            _lines[15].Direction = (right - forward) * _sideLengthHalf;

            _lines[16].Direction = (right + up) * _sideLengthHalf;
            _lines[17].Direction = (right - up) * _sideLengthHalf;
            _lines[18].Direction = (-right + up) * _sideLengthHalf;
            _lines[19].Direction = (-right - up) * _sideLengthHalf;

            foreach (var line in _lines)
            {
                float dotVal = Vector3.Dot(-_camera.transform.forward, line.Direction.normalized);
                line.bVisible = dotVal > _range.x && dotVal < _range.y;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            if (_camera)
            {
                Gizmos.DrawLine(transform.position, transform.position - _camera.transform.forward);
            }

            Gizmos.color = Color.red;
            foreach (var line in _lines)
            {
                if (line.bVisible)
                {
                    Gizmos.DrawLine(transform.position, line.Direction.normalized + transform.position);
                }
            }
        }
    }
}