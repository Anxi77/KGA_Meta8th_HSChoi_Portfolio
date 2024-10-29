using System.Collections.Generic;
using UnityEngine;

namespace LaserSystem2D
{
    public interface ILaserEntered 
    {
        void OnLaserEntered(LaserBase laserBase, List<RaycastHit2D> hits);
    }
}