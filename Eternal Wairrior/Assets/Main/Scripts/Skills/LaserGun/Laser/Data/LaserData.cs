using System;
using UnityEngine;

namespace LaserSystem2D
{
    [Serializable]
    public class LaserData
    {
        [SerializeField] private RaycastData _raycastData;
        [SerializeField] private CollisionData _collisionData;
        [SerializeField] private FullRectLine _line;
        [SerializeField] private ViewData _viewData;
        [SerializeField] private AudioSource _laserAudioSource;
        [SerializeField] private AudioSource _hitAudioSource;

        public float NonHitDistance => _raycastData.NonHitDistance;
        public float ShootingSpeed => _raycastData.ShootingSpeed;
        public LayerMask RaycastMask => _raycastData.Mask;
        public float CollisionPenetration => _collisionData.Penetration;
        public LayerMask CollisionMask => _collisionData.Mask;
        public float CollisionWidth => _collisionData.Width;
        public float DissolveTime => _viewData.DissolveTime;
        public int SortingOrder => _viewData.SortingOrder;
        public FullRectLine Line => _line;
        public ParticleSystem HitEffectPrefab => _viewData.HitEffectPrefab;
        public AudioSource LaserAudioSource => _laserAudioSource;
        public AudioSource HitAudioSource => _hitAudioSource;
        public bool EnableNonHitEffect => _viewData.EnableNonHitEffect;

        public RaycastData RaycastData => _raycastData;
        public CollisionData CollisionData => _collisionData;
        public ViewData ViewData => _viewData;
    }
}