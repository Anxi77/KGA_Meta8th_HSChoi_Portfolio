using UnityEngine;

namespace LaserSystem2D
{
    public class LaserLength : IEnable, IUpdate
    {
        private readonly RaycastData _raycastData;
        private readonly FullRectLine _line;
        
        public LaserLength(RaycastData raycastData, FullRectLine line)
        {
            _raycastData = raycastData;
            _line = line;
        }
        
        public float Fill { get; private set; }
        public float Current { get; protected set; }
        
        public void Enable()
        {
            SetToZero();
        }

        public void SetToZero()
        {
            Fill = 0;
            Current = 0;
        }
        
        public virtual void Update()
        {
            AddLength();
        }

        protected void AddLength()
        {
            Current = Mathf.Clamp(Current + _raycastData.ShootingSpeed * Time.deltaTime, 0, _line.Length);
            Fill = Current / _line.Length;
        }
    }
}