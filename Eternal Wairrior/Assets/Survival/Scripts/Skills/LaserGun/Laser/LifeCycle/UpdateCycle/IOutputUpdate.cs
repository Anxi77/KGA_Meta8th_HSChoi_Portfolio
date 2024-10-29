
namespace LaserSystem2D
{
    public interface IOutputUpdate<out TOutput>
    {
        TOutput Update();
    }
}