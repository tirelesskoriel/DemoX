using Mirror;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.Serialization;

namespace DemoX.Framework
{
    public class HandRayLine : NetworkBehaviour
    {
        [SerializeField] private HandType _handType;
        [SerializeField] private Transform _indicator;
        [Header("RayLine")] [SerializeField] private int _rayLinePointCount = 5;

        [FormerlySerializedAs("_rayLineStartOffset")] [SerializeField]
        private float _rayLineStartOffsetRatio = 0.5f;

        [SerializeField] private float _rayLineDefaultLength = 0.5f;
        [SerializeField] private AnimationCurve _rayLineCurve;

        private LineRenderer _line;
        private HandRayHandInteractor _hand;

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (isOwned)
            {
                _line = GetComponent<LineRenderer>();
                _hand = GetComponent<HandRayHandInteractor>();
            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            _line = GetComponent<LineRenderer>();
            _hand = GetComponent<HandRayHandInteractor>();
        }

        private void LateUpdate()
        {
            DrawLine();
        }

        private void DrawLine()
        {
            if (!_line || !_hand || !_hand.IsRayEnable) return;

            if (_hand.HandState == HandRayHandInteractor.EHandState.Pinch)
            {
                Transform rayStartPoint = _hand.GetRayStartPoint;
                Vector3 rayStartPosition = rayStartPoint.position;
                Vector3 rayStartForward = rayStartPoint.forward;

                Vector3 destination = rayStartPosition + rayStartForward * _hand.DistanceFromHit;
                Vector3 offsetToTarget = _hand.HandleableAttachingPosition - destination;

                _line.positionCount = _rayLinePointCount + 1;
                _line.SetPosition(0, rayStartPosition);

                float distanceFromHit = _hand.DistanceFromHit;
                rayStartPosition += rayStartForward * (_rayLineStartOffsetRatio * distanceFromHit);

                float lastDistance = distanceFromHit * (1 - _rayLineStartOffsetRatio);
                for (int i = 0; i < _rayLinePointCount; i++)
                {
                    float progress = i / (_rayLinePointCount - 1.0f);
                    float distance = progress * lastDistance;
                    float toAnchorIntensity = _rayLineCurve.Evaluate(progress);
                    Vector3 toAnchorOffset = toAnchorIntensity * offsetToTarget;
                    Vector3 point = rayStartPosition + rayStartForward * distance + toAnchorOffset;
                    _line.SetPosition(i + 1, point);
                }
            }
            else if (_hand.HandState == HandRayHandInteractor.EHandState.None)
            {
                Transform rayStartPoint = _hand.GetRayStartPoint;
                float distanceFromHit = _hand.DistanceFromHit;

                _line.positionCount = 2;
                Vector3 head = rayStartPoint.position;
                _line.SetPosition(0, head);

                float defaultLength = Mathf.Max(_rayLineDefaultLength, distanceFromHit);
                _line.SetPosition(1, head + rayStartPoint.forward * defaultLength);
            }
            else
            {
                _line.positionCount = 0;
            }

            _indicator.gameObject.SetActive(_line.positionCount > 0);
            if (_line.positionCount > 0)
            {
                _indicator.position = _line.GetPosition(_line.positionCount - 1);
            }
        }
    }
}