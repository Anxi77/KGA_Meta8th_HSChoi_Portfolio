
namespace LaserSystem2D
{
    public class InputUpdateLink<TInput, TOutput> : IInputUpdate<TInput>
    {
        private readonly IInputOutputUpdate<TInput, TOutput> _inputOutputUpdate;
        private readonly IInputUpdate<TOutput> _inputUpdate;

        public InputUpdateLink(IInputOutputUpdate<TInput, TOutput> inputOutputUpdate, IInputUpdate<TOutput> inputUpdate)
        {
            _inputOutputUpdate = inputOutputUpdate;
            _inputUpdate = inputUpdate;
        }

        public void Update(TInput input)
        {
            TOutput output = _inputOutputUpdate.Update(input);
            _inputUpdate.Update(output);
        }
    }
}