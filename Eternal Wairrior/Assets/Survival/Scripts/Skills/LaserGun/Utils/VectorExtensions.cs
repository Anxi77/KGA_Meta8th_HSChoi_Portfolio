using System.Collections.Generic;
using UnityEngine;

namespace LaserSystem2D
{
    public static class VectorExtensions
    {
        public static Vector2 GetNormal(this Vector2 vector)
        {
            return new Vector2(-vector.y, vector.x).normalized;
        }

        public static bool TryProjectOntoSegment(this Vector2 point, out Vector2 result, Vector2 segmentStart, Vector2 segmentEnd)
        {
            Vector2 segmentDirection = segmentEnd - segmentStart;
            Vector2 fromStartToPoint = point - segmentStart;
            Vector2 projection = Vector2.Dot(fromStartToPoint, segmentDirection.normalized) * segmentDirection.normalized;
            float projectionRatio = Vector2.Dot(fromStartToPoint, segmentDirection) / segmentDirection.sqrMagnitude;
            
            result = segmentStart + projection;
            return projectionRatio is >= 0f and <= 1f;
        }

        public static T GetRandom<T>(this IList<T> collection)
        {
            return collection[Random.Range(0, collection.Count)];
        }
    }
}