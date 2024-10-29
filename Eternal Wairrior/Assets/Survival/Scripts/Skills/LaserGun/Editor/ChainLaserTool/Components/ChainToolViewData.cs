using UnityEngine;

namespace LaserSystem2D
{
    public class ChainToolViewData
    {
        public readonly Color SelectionColor = new(1, 0, 0, 0.75f);
        public readonly Color DefaultColor = new(0, 1, 0, 0.5f);
        public readonly Color HoverColor = new(1, 0, 1, 0.3f);
        public readonly Color GhostColor = new(0, 0.8f, 1, 0.3f);
    }
}