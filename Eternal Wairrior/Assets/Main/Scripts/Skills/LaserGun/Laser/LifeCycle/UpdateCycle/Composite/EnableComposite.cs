
namespace LaserSystem2D
{
    public class EnableComposite : IEnable
    {
        private readonly IEnable[] _elements;

        public EnableComposite(params IEnable[] elements)
        {
            _elements = elements;
        }

        public void Enable()
        {
            foreach (IEnable element in _elements)
            {
                element.Enable();
            }
        }
    }
}