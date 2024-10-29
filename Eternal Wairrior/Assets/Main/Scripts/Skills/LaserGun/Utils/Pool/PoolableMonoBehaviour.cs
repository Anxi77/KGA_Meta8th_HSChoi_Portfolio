using UnityEngine;

namespace LaserSystem2D
{
    public abstract class PoolableMonoBehaviour : MonoBehaviour, IPoolable
    {
        public abstract bool IsActive { get; }
        
        public void Reset()
        {
            gameObject.SetActive(true);
        }

        public void ReturnToPool()
        {
            gameObject.SetActive(false);
        }
    }
}