using System.Collections.Generic;
using UnityEngine;

namespace LaserSystem2D
{
    public class LaserInteraction : IInputUpdate<Dictionary<Collider2D, List<RaycastHit2D>>>, IDisable
    {
        private readonly Dictionary<Collider2D, InteractedObject> _interactedObjects = new();
        private readonly List<Collider2D> _objectsToExit = new();
        private readonly LaserBase _laserBase;

        public LaserInteraction(LaserBase laserBase)
        {
            _laserBase = laserBase;
        }

        public void Update(Dictionary<Collider2D, List<RaycastHit2D>> hits)
        {
            Enter(hits);
            Stay(hits);
            Exit();
        }

        public void Disable()
        {
            foreach (InteractedObject interactedObject in _interactedObjects.Values)
            {
                interactedObject.Exit();
            }
        
            _interactedObjects.Clear();
        }

        private void Enter(Dictionary<Collider2D, List<RaycastHit2D>> hits)
        {
            foreach (Collider2D hit in hits.Keys)
            {
                if (_interactedObjects.ContainsKey(hit) == false)
                {
                    _interactedObjects[hit] = new InteractedObject(hit.transform, _laserBase);
                    _interactedObjects[hit].Enter(hits[hit]);
                }
            }
        }

        private void Stay(Dictionary<Collider2D, List<RaycastHit2D>> hits)
        {
            _objectsToExit.Clear();
        
            foreach (Collider2D overlappingObject in _interactedObjects.Keys)
            {
                if (hits.TryGetValue(overlappingObject, out List<RaycastHit2D> value))
                {
                    _interactedObjects[overlappingObject]?.Update(value);
                }
                else
                {
                    _objectsToExit.Add(overlappingObject);
                }
            }
        }

        private void Exit()
        {
            foreach (Collider2D objectToExit in _objectsToExit)
            {
                _interactedObjects[objectToExit].Exit();
                _interactedObjects.Remove(objectToExit);
            }
        }
    }
}