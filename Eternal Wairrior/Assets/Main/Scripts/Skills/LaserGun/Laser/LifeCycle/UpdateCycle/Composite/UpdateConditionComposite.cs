using System.Linq;

namespace LaserSystem2D
{
    public class UpdateConditionComposite : IUpdateCondition
    {
        private readonly IUpdateCondition[] _updateConditions;

        public UpdateConditionComposite(IUpdateCondition[] updateConditions)
        {
            _updateConditions = updateConditions;
        }

        public bool IsTrue()
        {
            return _updateConditions.All(updateCondition => updateCondition.IsTrue());
        }
    }
}