
namespace LaserSystem2D
{
    public interface IFactory<out T>
    {
        T Create();
    }
}