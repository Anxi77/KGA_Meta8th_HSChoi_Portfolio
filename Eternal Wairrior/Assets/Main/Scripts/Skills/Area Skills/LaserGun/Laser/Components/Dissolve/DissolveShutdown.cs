
namespace LaserSystem2D
{
    public class DissolveShutdown : IDisable, IEnable
    {
        private readonly ICoroutineRunner _context;
        private readonly LaserDissolve _dissolve;
        private readonly LaserLength _length;

        public DissolveShutdown(ICoroutineRunner context, LaserDissolve dissolve, LaserLength length)
        {
            _context = context;
            _dissolve = dissolve;
            _length = length;
        }

        public void Enable()
        {
            _context.StopAllCoroutines();
        }

        public void Disable()
        {
            if (_context.isActiveAndEnabled)
            {
                _context.StartCoroutine(_dissolve.Disappear());
            }
            else
            {
                _dissolve.SetToZero();
                _length.SetToZero();
            }
        }
    }
}