
namespace LaserSystem2D
{
    public class LineRebuilding : IUpdate
    {
        private readonly ILaserKeyPointProvider _keyPointProvider;
        private readonly FullRectLine _fullRectLine;

        public LineRebuilding(FullRectLine fullRectLine, ILaserKeyPointProvider keyPointProvider)
        {
            _keyPointProvider = keyPointProvider;
            _fullRectLine = fullRectLine;
        }

        public void Update()
        {
            _fullRectLine.ClearPoints();

            for (int i = 0; i < _keyPointProvider.Count; ++i)
            {
                _fullRectLine.AddPoint(_keyPointProvider[i]);
            }
        
            _fullRectLine.Regenerate();
        }
    }
}