
namespace LaserSystem2D
{
    public class DisableComposite : IDisable
    {
        private readonly IDisable[] _elements;

        public DisableComposite(params IDisable[] elements)
        {
            _elements = elements;
        }

        public void Disable()
        {
            foreach (IDisable element in _elements)
            {
                element.Disable();
            }
        }
    }
}