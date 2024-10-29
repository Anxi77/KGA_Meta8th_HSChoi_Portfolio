
namespace LaserSystem2D
{
    public interface IInputOutputUpdate<in TInput, out TOutput>
    {
        TOutput Update(TInput input);
    }
}