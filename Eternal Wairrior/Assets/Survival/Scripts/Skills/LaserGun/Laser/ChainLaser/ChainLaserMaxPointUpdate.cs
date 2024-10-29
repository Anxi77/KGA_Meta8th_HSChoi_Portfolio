
namespace LaserSystem2D
{
    public class ChainLaserMaxPointUpdate : IUpdate
    {
        private readonly ILaserKeyPointProvider _keyPointProvider;
        private readonly FullRectLine _fullRectLine;

        public ChainLaserMaxPointUpdate(ILaserKeyPointProvider keyPointProvider, FullRectLine fullRectLine)
        {
            _keyPointProvider = keyPointProvider;
            _fullRectLine = fullRectLine;
        }

        public void Update()
        {
            _fullRectLine.MaxPoints = _keyPointProvider.Count;
        }
    }
}