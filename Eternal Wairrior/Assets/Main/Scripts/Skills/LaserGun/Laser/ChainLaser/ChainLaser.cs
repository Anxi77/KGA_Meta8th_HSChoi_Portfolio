using UnityEngine;

namespace LaserSystem2D
{
    public class ChainLaser : LaserBase
    {
        [SerializeField] private ChainLaserKeyPointsProvider _keyPointsProvider;
        [SerializeField] [Min(0)] private float _gizmoRadius = 0.25f;

        public float GizmoHandlesRadius => _gizmoRadius;
        public ObservableList<Vector2> KeyPoints => _keyPointsProvider.List;
        
        protected override ILaserLifeCycleFactory CreateLifeCycleFactory() => 
            new ChainLaserLifeCycleFactory(Switcher, this, _keyPointsProvider);
    }
}