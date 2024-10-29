using System;
using UnityEngine;

namespace LaserSystem2D
{
    public class Laser : LaserBase 
    {
        private LaserComponents _components;
        
        public Guid Id { get; private set; } = Guid.NewGuid();
        public RaycastHitEvent HitEvent => _components.HitEvent;

        protected override ILaserLifeCycleFactory CreateLifeCycleFactory()
        {
            _components = new LaserComponents(Data, Dissolve, this);
            Data.Line.Initialize(GetComponent<MeshFilter>());
            
            return new LaserLifeCycleFactory(Switcher, Dissolve, _components);
        }

        public void BranchLaser(Laser parent)
        {
            TryInitialize();
            Id = parent.Id;
            _components.HitEvent.InheritEvents(parent._components.HitEvent);
        }

        public void Enable(Transform laserPoint)
        {
            if (Switcher.Enabled == false)
            {
                Enable();
                _components.TransformMapper.SetSource(laserPoint);
            }
        }
    }
}