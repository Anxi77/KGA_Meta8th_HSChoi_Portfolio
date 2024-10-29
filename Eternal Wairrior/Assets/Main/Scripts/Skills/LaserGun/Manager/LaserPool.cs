using System;
using System.Collections.Generic;

namespace LaserSystem2D
{
    public class LaserPool
    {
        private readonly Dictionary<Guid, ObjectPool<Laser>> _pool = new();

        public Laser GetLaser(Laser instanceToCopy)
        {
            if (_pool.ContainsKey(instanceToCopy.Id) == false)
            {
                LaserFactory factory = new(instanceToCopy);
                _pool[instanceToCopy.Id] = new ObjectPool<Laser>(factory);
                _pool[instanceToCopy.Id].Push(instanceToCopy);
            }
        
            return _pool[instanceToCopy.Id].Create();
        }
    
        public void Update()
        {
            foreach (ObjectPool<Laser> laserPool in _pool.Values)
            {
                laserPool.Update();
            }
        }
    }
}