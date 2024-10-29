using System.Collections.Generic;

namespace LaserSystem2D
{
    public class RaycastLaserLength : LaserLength
    {
        private readonly List<LaserHit> _previousHits = new();
        private readonly LaserRaycast _raycast;

        public RaycastLaserLength(RaycastData raycastData, FullRectLine line, LaserRaycast raycast) : base(raycastData, line)
        {
            _raycast = raycast;
        }

        public override void Update()
        {
            TryExpandPreviousHits();
            AlignLineLength();
            AddLength();
            UpdatePreviousHits();
        }

        private void TryExpandPreviousHits()
        {
            for (int i = 0; i < _raycast.Count - 1; ++i)
            {
                if (_previousHits.Count <= i)
                {
                    _previousHits.Add(new LaserHit());
                }
            }
        }

        private void UpdatePreviousHits()
        {
            for (int i = 0; i < _raycast.Count - 1; ++i)
            {
                _previousHits[i].Update(_raycast.Hits[i]);
            }
        }

        private void AlignLineLength()
        {
            float previousDistance = 0;
            float distance = 0;
        
            for (int i = 0; i < _raycast.Count - 1; ++i)
            {
                AddHitDistance(i, ref previousDistance, ref distance);

                if (IsNewHit(i))
                {
                    AlignLineLength(previousDistance, distance);
                }
            }
        }

        private bool IsNewHit(int hitIndex)
        {
            return _previousHits[hitIndex].HitObject != _raycast.Hits[hitIndex].HitObject ||
                   _previousHits[hitIndex].Normal != _raycast.Hits[hitIndex].Normal;
        }

        private void AlignLineLength(float previousDistance, float currentDistance)
        {
            if (Current >= previousDistance)
            {
                Current = previousDistance;
            }

            if (Current >= currentDistance)
            {
                Current = currentDistance;
            }
        }

        private void AddHitDistance(int hitIndex, ref float previousDistance, ref float distance)
        {
            previousDistance += _previousHits[hitIndex].Distance;
            distance += _raycast.Hits[hitIndex].Distance;
        }
    }
}