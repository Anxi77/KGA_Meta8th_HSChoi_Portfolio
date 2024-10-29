using UnityEngine;

namespace LaserSystem2D
{
    public class IsPlayingCondition : IUpdateCondition
    {
        public bool IsTrue() => Application.isPlaying;
    }
}