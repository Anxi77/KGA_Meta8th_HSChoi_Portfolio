using System;
using System.Collections.Generic;
using UnityEngine;

namespace LaserSystem2D
{
    public class LaserCollision : IOutputUpdate<Dictionary<Collider2D, List<RaycastHit2D>>>
    {
        private readonly ILaserKeyPointProvider _keyPointProvider;
        private readonly CollisionData _collisionData;
        private readonly BoxCast2D _boxCast = new();
        private readonly LaserLength _length;

        public LaserCollision(CollisionData collisionData, LaserLength length, ILaserKeyPointProvider keyPointProvider)
        {
            _keyPointProvider = keyPointProvider;
            _collisionData = collisionData;
            _length = length;
        }

        public Dictionary<Collider2D, List<RaycastHit2D>> Update()
        {
            int maxCastCount = _keyPointProvider.Count == 0 ? 0 : _keyPointProvider.Count - 1;
            
            Span<BoxCastData> boxCastData = stackalloc BoxCastData[maxCastCount];
            int castCount = GatherBoxCastData(ref boxCastData);
            DrawGizmo(boxCastData, castCount);
            
            return _boxCast.Update(boxCastData, castCount);
        }

        private int GatherBoxCastData(ref Span<BoxCastData> boxCastData)
        {
            int castCount = 0;
            float sumDistance = 0;
            
            for (int i = 0; i < boxCastData.Length && sumDistance < _length.Current; ++i, ++castCount)
            {
                Vector2 firstPoint = _keyPointProvider[i];
                Vector2 secondPoint = _keyPointProvider[i + 1];
                Vector2 direction = secondPoint - firstPoint;
                float segmentDistance = direction.magnitude;
                sumDistance += segmentDistance;
                
                if (sumDistance > _length.Current)
                {
                    segmentDistance -= sumDistance - _length.Current;
                }

                boxCastData[i] = new BoxCastData(direction.normalized, segmentDistance, firstPoint, _collisionData.Mask, _collisionData.Width);
            }

            return castCount;
        }

        public void DrawGizmo(in ReadOnlySpan<BoxCastData> data, int count)
        {
            for (int i = 0; i < count; ++i)
            {
                Vector2 normal = data[i].Direction.GetNormal();
                Vector2 leftBottom = data[i].Origin - normal * data[i].Width;
                Vector2 rightBottom = data[i].Origin + normal * data[i].Width;
                Vector2 leftTop = leftBottom + data[i].Direction * data[i].Distance;
                Vector2 rightTop = rightBottom + data[i].Direction * data[i].Distance;
                
                Debug.DrawLine(leftBottom, leftTop, Color.green);
                Debug.DrawLine(rightBottom, rightTop, Color.green);
                Debug.DrawLine(leftBottom, rightBottom, Color.green);
                Debug.DrawLine(leftTop, rightTop, Color.green);
                Debug.DrawLine(Vector3.zero, Vector3.one * 2);
            }
        }
    }
}