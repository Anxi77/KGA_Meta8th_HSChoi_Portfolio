using UnityEngine;

namespace LaserSystem2D
{
    public readonly struct BoxCastData
    {
        public readonly Vector2 Direction;
        public readonly float Distance;
        public readonly Vector2 Origin;
        public readonly LayerMask Mask;
        public readonly float Width;
        private readonly float _height;

        public BoxCastData(Vector2 direction, float distance, Vector2 origin, LayerMask mask, float width, float height = 0.01f)
        {
            Direction = direction;
            Distance = distance;
            _height = height;
            Origin = origin;
            Width = width;
            Mask = mask;
        }
        
        public float Angle => Mathf.Atan(Direction.y / Direction.x) * Mathf.Rad2Deg + 90;
        public Vector2 Size => new(Width, _height);
    }
}