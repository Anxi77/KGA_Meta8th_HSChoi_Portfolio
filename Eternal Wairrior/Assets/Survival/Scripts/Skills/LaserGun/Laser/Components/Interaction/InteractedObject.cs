using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LaserSystem2D
{
    public class InteractedObject
    {
        private readonly IEnumerable<ILaserStay> _onStay;
        private readonly IEnumerable<ILaserExited> _onExited;
        private readonly IEnumerable<ILaserEntered> _onEntered;
        private readonly LaserBase _laserBase;

        public InteractedObject(Transform context, LaserBase laserBase)
        {
            _laserBase = laserBase;
            _onStay = GetComponents<ILaserStay>(context);
            _onExited = GetComponents<ILaserExited>(context);
            _onEntered = GetComponents<ILaserEntered>(context);
        }

        public void Enter(List<RaycastHit2D> hits)
        {
            foreach (ILaserEntered entered in _onEntered)
            {
                entered?.OnLaserEntered(_laserBase, hits);
            }
        }

        public void Update(List<RaycastHit2D> hits)
        {
            foreach (ILaserStay stay in _onStay)
            {
                stay?.OnLaserStay(_laserBase, hits);
            }
        }

        public void Exit()
        {
            foreach (ILaserExited exited in _onExited)
            {
                exited?.OnLaserExited(_laserBase);
            }
        }

        private IEnumerable<T> GetComponents<T>(Transform context)
        {
            MonoBehaviour[] monoBehaviours = context.GetComponents<MonoBehaviour>();

            return monoBehaviours.OfType<T>();
        }
    }
}