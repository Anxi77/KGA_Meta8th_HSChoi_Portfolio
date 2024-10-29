using System.Collections.Generic;
using UnityEngine;

namespace LaserSystem2D
{
    public class LaserRaycast : ILaserKeyPointProvider, IUpdate
    {
        private readonly List<LaserHit> _hits = new();
        private readonly FullRectLine _fullRectLine;
        private readonly RaycastData _raycastData;
        private readonly Transform _transform;
        private int _depth;

        public LaserRaycast(Transform transform, RaycastData raycastData, FullRectLine fullRectLine)
        {
            _fullRectLine = fullRectLine;
            _raycastData = raycastData;
            _transform = transform;
        }

        public IReadOnlyList<LaserHit> Hits => _hits;
        public Vector2 this[int index] => index == 0 ? _transform.position : _hits[index - 1].HitPoint;
        public int Count => _depth + 1;
        
        void IUpdate.Update()
        {
            ShootRecursive();
        }

        private void ShootRecursive()
        {
            bool queriesStartInColliders = Physics2D.queriesStartInColliders;
            Physics2D.queriesStartInColliders = false;
            
            _depth = 0;
            ShootRecursive(_transform.position, _transform.right, _raycastData.Mask, _fullRectLine.MaxPoints);

            Physics2D.queriesStartInColliders = queriesStartInColliders;
        }

        private void ShootRecursive(Vector2 startPoint, Vector2 direction, LayerMask mask, int maxPoints)
        {
            RaycastHit2D hit = Physics2D.Raycast(startPoint, direction, _raycastData.NonHitDistance, mask);
        
            if (hit.collider != null)
            {
                HandleHit(startPoint, direction, ref hit);
            
                if (LaserManager.Instance.ReflectingColliders.Contains(hit.collider) && _depth < maxPoints)
                {
                    ShootRecursive(hit.point, _hits[_depth - 1].ReflectedDirection, mask, maxPoints);
                }
            }
            else
            {
                hit.point = startPoint + direction * _raycastData.NonHitDistance;
                HandleHit(startPoint, direction, ref hit);
            }
        }

        private void HandleHit(Vector2 startPoint, Vector2 direction, ref RaycastHit2D hit)
        {
            while (_hits.Count <= _depth)
            {
                _hits.Add(new LaserHit());
            }
        
            _hits[_depth].Update(ref hit, startPoint, direction);
            ++_depth;
        }
    }
}