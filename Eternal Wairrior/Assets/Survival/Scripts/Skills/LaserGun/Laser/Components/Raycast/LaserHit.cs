using UnityEngine;

namespace LaserSystem2D
{
    public class LaserHit
    {
        public Collider2D HitObject { get; private set; }
        public Vector2 HitPoint { get; private set; }
        public Vector2 Normal { get; private set; }
        public Vector2 Direction { get; private set; }
        public float Distance { get; private set; }
        public Vector2 ReflectedDirection => Vector2.Reflect(Direction, Normal);

        public void Update(LaserHit hit)
        {
            SetValue(hit.HitObject, hit.HitPoint, hit.Normal, hit.Direction, hit.Distance);
        }
    
        public void Update(ref RaycastHit2D raycastHit2D, Vector2 startPoint, Vector2 direction)
        {
            float hitDistance = Vector2.Distance(raycastHit2D.point, startPoint);
            SetValue(raycastHit2D.collider, raycastHit2D.point, raycastHit2D.normal, direction, hitDistance);
        }

        public void SetValue(Collider2D hitObject, Vector2 hitPoint, Vector2 normal, Vector2 direction, float distance)
        {
            HitObject = hitObject;
            HitPoint = hitPoint;
            Normal = normal;
            Direction = direction;
            Distance = distance;
        }
    }
}