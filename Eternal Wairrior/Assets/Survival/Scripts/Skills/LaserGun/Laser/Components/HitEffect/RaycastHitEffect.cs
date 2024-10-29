using UnityEngine;

namespace LaserSystem2D
{
    public class RaycastHitEffect : HitEffect
    {
        private readonly LaserRaycast _raycast;
        private readonly ViewData _viewData;

        public RaycastHitEffect(ParticleSystem effectPrefab, LaserActiveHits activeHits, LaserRaycast raycast, ViewData viewData) 
            : base(effectPrefab, activeHits, raycast)
        {
            _raycast = raycast;
            _viewData = viewData;
        }

        protected override int GetEffectHits(int activeHits)
        {
            if (activeHits != 0)
            {
                Collider2D lastHit = _raycast.Hits[activeHits - 1].HitObject;
                bool isNonHit = lastHit == null;
                bool disableNonHitEffect = _viewData.EnableNonHitEffect == false;
                
                if (isNonHit && disableNonHitEffect)
                {
                    return activeHits - 1;
                }
            }
        
            return activeHits;
        }
    }
}