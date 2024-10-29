using System;
using UnityEngine;

namespace LaserSystem2D
{
    [Serializable]
    public class CollisionData
    {
        [SerializeField] [Range(0, 0.5f)] private float _penetration = 0.03f;
        [SerializeField] [Range(0, 2)] private float _width = 0.15f;
        [SerializeField] private LayerMask _mask = 1 << 0;

        public float Penetration => _penetration;
        public LayerMask Mask => _mask;
        public float Width => _width;
    }
}