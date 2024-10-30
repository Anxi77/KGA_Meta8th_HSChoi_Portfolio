using UnityEngine;

namespace LaserSystem2D
{
    public interface ILaserKeyPointProvider
    {
        Vector2 this[int index] { get; }
        int Count { get; }
    }
}