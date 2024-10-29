using UnityEngine;

namespace LaserSystem2D
{
    public class PoolableParticleSystem : IPoolable
    {
        public readonly ParticleSystem Value;
        public bool IsActive => Value.isPlaying;

        public PoolableParticleSystem(ParticleSystem value)
        {
            Value = value;
        }
    
        public void Reset()
        {
            Value.gameObject.SetActive(true);
        }

        public void ReturnToPool()
        {
            Value.gameObject.SetActive(false);
        }
    }
}