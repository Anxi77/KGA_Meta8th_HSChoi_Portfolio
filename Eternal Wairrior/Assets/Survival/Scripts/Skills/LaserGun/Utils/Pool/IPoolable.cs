
namespace LaserSystem2D
{
    public interface IPoolable
    {
        bool IsActive { get; }
        void Reset();
        void ReturnToPool();
    }
}