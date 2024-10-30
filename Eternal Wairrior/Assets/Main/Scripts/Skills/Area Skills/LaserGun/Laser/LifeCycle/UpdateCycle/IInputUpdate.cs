
namespace LaserSystem2D
{
    public interface IInputUpdate<in TInput>
    {
        void Update(TInput input);
    }
}