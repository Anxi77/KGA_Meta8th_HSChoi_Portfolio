
namespace LaserSystem2D
{
    public class LaserActiveHits : IUpdate
    {
        private readonly float _hitCalibration = 0.01f;
        private readonly ILaserKeyPointProvider _keyPointProvider;
        private readonly LaserLength _length;

        public LaserActiveHits(LaserLength length, ILaserKeyPointProvider keyPointProvider)
        {
            _keyPointProvider = keyPointProvider;
            _length = length;
        }

        public int Value { get; private set; }

        public void Update()
        {
            float hitDistanceSum = 0;
            Value = 0;

            for (int i = 1; i < _keyPointProvider.Count && _length.Current > hitDistanceSum; ++i)
            {
                float segmentDistance = (_keyPointProvider[i] - _keyPointProvider[i - 1]).magnitude;

                if (IsLaserReachedHitPoint(segmentDistance, hitDistanceSum))
                {
                    ++Value;
                }
                
                hitDistanceSum += segmentDistance;
            }
        }

        private bool IsLaserReachedHitPoint(float segmentDistance, float hitDistanceSum)
        {
            return _length.Current > hitDistanceSum + segmentDistance - _hitCalibration;
        }
    }
}