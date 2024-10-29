using System;

namespace LaserSystem2D
{
    public class RaycastHitEvent : HitEvent
    {
        private readonly LaserRaycast _raycast;
        private Action<LaserHit> _raycastHit;

        public RaycastHitEvent(LaserActiveHits activeHits, LaserRaycast raycast) : base(activeHits)
        {
            _raycast = raycast;
        }

        public void AddListener(Action<LaserHit> handler)
        {
            _raycastHit += handler;
        }

        public void RemoveListener(Action<LaserHit> handler)
        {
            _raycastHit -= handler;
        }

        protected override void OnHit(int hitIndex)
        {
            _raycastHit?.Invoke(_raycast.Hits[hitIndex - 1]);
        }
        
        public void InheritEvents(RaycastHitEvent raycastHitEvent)
        {
            _raycastHit = raycastHitEvent._raycastHit;
        }
    }
}