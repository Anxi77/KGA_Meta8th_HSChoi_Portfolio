
namespace LaserSystem2D
{
    public class UpdateComposite : IUpdate
    {
        private readonly IUpdate[] _updates;

        public UpdateComposite(params IUpdate[] updates)
        {
            _updates = updates;
        }
        
        public void Update()
        {
            foreach (IUpdate update in _updates)
            {
                update.Update();
            }
        }
    }
}