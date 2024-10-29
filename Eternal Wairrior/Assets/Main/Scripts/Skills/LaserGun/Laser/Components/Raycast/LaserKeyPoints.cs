using System.Collections.Generic;

namespace LaserSystem2D
{
    public readonly struct LaserKeyPoints 
    {
        public readonly List<LaserHit> Hits;
        public readonly int Count;

        public LaserKeyPoints(List<LaserHit> hits, int count)
        {
            Hits = hits;
            Count = count;
        }
    }
}