using System.Collections.Generic;
using UnityEngine;

namespace LaserSystem2D
{
    public class ReflectingColliders
    {
        private readonly HashSet<Collider2D> _reflectingColliders = new();
    
        public void Add(IReadOnlyCollection<Collider2D> reflectingColliders)
        {
            foreach (Collider2D reflectingCollider in reflectingColliders)
            {
                _reflectingColliders.Add(reflectingCollider);
            }
        }

        public bool Contains(Collider2D collider2d)
        {
            return _reflectingColliders.Contains(collider2d);
        }
    }
}