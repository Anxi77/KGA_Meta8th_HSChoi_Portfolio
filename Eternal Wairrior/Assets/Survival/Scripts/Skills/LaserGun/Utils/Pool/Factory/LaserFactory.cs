
namespace LaserSystem2D
{
    public class LaserFactory : IFactory<Laser>
    {
        private readonly ComponentFactory<Laser> _componentFactory;
        private readonly Laser _prefab;

        public LaserFactory(Laser prefab)
        {
            _componentFactory = new ComponentFactory<Laser>(prefab);
            _prefab = prefab;
        }

        public Laser Create()
        {
            Laser laser = _componentFactory.Create();
            laser.BranchLaser(_prefab);

            return laser;
        }
    }
}