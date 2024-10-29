
namespace LaserSystem2D
{
    public class Switcher : IUpdateCondition
    {
        public bool Enabled { get; private set; }
        
        public bool TryEnable()
        {
            return TrySetState(true);
        }

        public bool TryDisable()
        {
            return TrySetState(false);
        }

        private bool TrySetState(bool targetState)
        {
            bool stateChanged = Enabled != targetState;
            
            if (Enabled != targetState)
            {
                Enabled = targetState;
            }

            return stateChanged;
        }

        public bool IsTrue() => Enabled;
    }
}