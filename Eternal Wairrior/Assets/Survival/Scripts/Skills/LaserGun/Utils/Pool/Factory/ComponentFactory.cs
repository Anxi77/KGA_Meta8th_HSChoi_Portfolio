using UnityEngine;

namespace LaserSystem2D
{
    public class ComponentFactory<T> : IFactory<T> where T : Component
    {
        private readonly T _prefab;
        private int _count;
    
        public ComponentFactory(T prefab)
        {
            _prefab = prefab;
        }
    
        public T Create()
        {
            T result = Object.Instantiate(_prefab, LaserManager.Instance.transform);
            result.transform.localPosition = Vector3.zero;
            result.transform.localRotation = Quaternion.identity;
            result.gameObject.name = $"{_prefab.gameObject.name} {++_count}";
        
            return result;
        }
    }
}