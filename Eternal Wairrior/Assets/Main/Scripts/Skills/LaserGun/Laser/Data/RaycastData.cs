using System;
using UnityEngine;

namespace LaserSystem2D
{
    [Serializable]
    public class RaycastData
    {
        [SerializeField] private LayerMask _mask = 1 << 0;
        [SerializeField] [Min(0)] private float _nonHitDistance = 20;
        [SerializeField] [Min(0)] private float _shootingSpeed = 15;
        
        public LayerMask Mask => _mask;
        public float NonHitDistance => _nonHitDistance;
        public float ShootingSpeed => _shootingSpeed;
    }
}