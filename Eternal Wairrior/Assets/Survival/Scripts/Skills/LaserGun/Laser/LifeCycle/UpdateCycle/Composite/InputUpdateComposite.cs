
namespace LaserSystem2D
{
    public class InputUpdateComposite<T> : IInputUpdate<T>
    {
        private readonly IInputUpdate<T>[] _inputUpdates;

        public InputUpdateComposite(params IInputUpdate<T>[] inputUpdates)
        {
            _inputUpdates = inputUpdates;
        }

        public void Update(T input)
        {
            foreach (IInputUpdate<T> inputUpdate in _inputUpdates)
            {
                inputUpdate.Update(input);
            }
        }
    }
}