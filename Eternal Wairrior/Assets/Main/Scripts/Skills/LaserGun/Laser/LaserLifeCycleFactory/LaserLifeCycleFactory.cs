using System.Collections.Generic;
using UnityEngine;

namespace LaserSystem2D
{
    public class LaserLifeCycleFactory : ILaserLifeCycleFactory
    {
        private readonly IUpdateCondition _enabledCondition;
        private readonly LaserComponents _components;
        private readonly LaserDissolve _dissolve;

        public LaserLifeCycleFactory(IUpdateCondition enabledCondition, LaserDissolve dissolve, LaserComponents components) 
        {
            _enabledCondition = enabledCondition;
            _components = components;
            _dissolve = dissolve;
        }

        public LifeCycle Create()
        {
            IUpdate update = new UpdateComposite(
                _components.TransformMapper, 
                _components.Raycast, 
                _components.LineRebuilding, 
                _components.Length, 
                _components.ActiveHits, 
                _components.HitEvent,
                _components.HitEffect,
                new UpdateLink<Dictionary<Collider2D, List<RaycastHit2D>>>(_components.Collision, _components.Interaction));

            IDisable disableAction = new DisableComposite(_components.HitEffect, _components.Audio, _components.Interaction, _components.DissolveShutdown);
            IEnable enableAction = new EnableComposite(_components.Length, _dissolve, _components.Audio, _components.DissolveShutdown);
            IUpdate updateCycle = new UpdateComposite(new ConditionalUpdate(_enabledCondition, update), _components.View);
            
            return new LifeCycle(enableAction, disableAction, updateCycle);
        }
    }
}