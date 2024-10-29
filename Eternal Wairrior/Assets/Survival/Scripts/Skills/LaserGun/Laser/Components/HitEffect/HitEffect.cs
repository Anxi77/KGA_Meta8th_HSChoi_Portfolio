using System.Collections.Generic;
using UnityEngine;

namespace LaserSystem2D
{
    public abstract class HitEffect : IUpdate, IDisable
    {
        private readonly ILaserKeyPointProvider _keyPointProvider;
        private readonly List<ParticleSystem> _effects = new();
        private readonly ParticleSystem _effectPrefab;
        private readonly LaserActiveHits _activeHits;

        protected HitEffect(ParticleSystem effectPrefab, LaserActiveHits activeHits, ILaserKeyPointProvider keyPointProvider)
        {
            _keyPointProvider = keyPointProvider;
            _effectPrefab = effectPrefab;
            _activeHits = activeHits;
        }

        public void Disable()
        {
            foreach (ParticleSystem particleSystem in _effects)
            {
                if (particleSystem != null)
                {
                    particleSystem.Stop();
                }
            }

            _effects.Clear();
        }

        public void Update()
        {
            if (_effectPrefab != null)
            {
                int effectHits = GetEffectHits(_activeHits.Value);
                AlignSizeToHits(effectHits);
                UpdateHitEffects();
            }
        }

        private void AlignSizeToHits(int activeHits)
        {
            while (_effects.Count < activeHits)
            {
                ParticleSystem updatingEffect = LaserManager.Instance.HitPool.GetHitEffect(_effectPrefab);
                updatingEffect.Play();
                _effects.Add(updatingEffect);
            }
        
            while (_effects.Count > activeHits)
            {
                _effects[_effects.Count - 1].Stop();
                _effects.RemoveAt(_effects.Count - 1);
            }
        }

        private void UpdateHitEffects()
        {
            for (int i = 0; i < _effects.Count; ++i)
            {
                if (_effects[i].isPlaying == false)
                {
                    _effects[i].Play();
                }
                
                _effects[i].transform.position = _keyPointProvider[i + 1];
            }
        }

        protected abstract int GetEffectHits(int activeHits);
    }
}