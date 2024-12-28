using UnityEngine;

namespace DemoX.Framework.Core
{
    public class Coordinate
    {
        private Matrix4x4 _worldToCoordinateMatrix;
        private Matrix4x4 _coordinateToWorldMatrix;

        public void InitializeMatrix(Vector3 origin, Vector3 x, Vector3 y, Vector3 z)
        {
            Vector3 newAxisX = x.normalized;
            Vector3 newAxisY = y.normalized;
            Vector3 newAxisZ = z.normalized;

            _worldToCoordinateMatrix = new Matrix4x4();
            _worldToCoordinateMatrix.SetColumn(0, new Vector4(newAxisX.x, newAxisX.y, newAxisX.z, 0.0f));
            _worldToCoordinateMatrix.SetColumn(1, new Vector4(newAxisY.x, newAxisY.y, newAxisY.z, 0.0f));
            _worldToCoordinateMatrix.SetColumn(2, new Vector4(newAxisZ.x, newAxisZ.y, newAxisZ.z, 0.0f));
            _worldToCoordinateMatrix.SetColumn(3, new Vector4(origin.x, origin.y, origin.z, 1.0f));

            _coordinateToWorldMatrix =
                Matrix4x4.TRS(origin, Quaternion.LookRotation(newAxisZ, newAxisY), Vector3.one).inverse;
        }

        public Vector3 WorldToCoordinate(Vector3 worldPoint)
        {
            return _worldToCoordinateMatrix.MultiplyPoint(worldPoint);
        }

        public Vector3 WorldVectorToCoordinate(Vector3 worldVector)
        {
            return _worldToCoordinateMatrix.MultiplyVector(worldVector);
        }

        public Vector3 WorldDirectionToCoordinate(Vector3 worldVector)
        {
            return WorldVectorToCoordinate(worldVector).normalized;
        }

        public Vector3 CoordinateToWorld(Vector3 localPoint)
        {
            return _coordinateToWorldMatrix.MultiplyPoint(localPoint);
        }

        public Vector3 CoordinateToWorldVector(Vector3 localVector)
        {
            return _coordinateToWorldMatrix.MultiplyVector(localVector);
        }
        
        public Vector3 CoordinateToWorldDirection(Vector3 localVector)
        {
            return CoordinateToWorldVector(localVector).normalized;
        }
    }
}