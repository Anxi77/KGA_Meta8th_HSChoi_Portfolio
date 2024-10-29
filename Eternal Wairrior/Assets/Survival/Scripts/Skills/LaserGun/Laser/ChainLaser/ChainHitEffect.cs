using UnityEngine;

namespace LaserSystem2D
{
    public class ChainHitEffect : HitEffect
    {
        public ChainHitEffect(ParticleSystem effectPrefab, LaserActiveHits activeHits, ILaserKeyPointProvider keyPointProvider) : base(effectPrefab, activeHits, keyPointProvider)
        {
        }

        protected override int GetEffectHits(int activeHits) => activeHits;
    }
}