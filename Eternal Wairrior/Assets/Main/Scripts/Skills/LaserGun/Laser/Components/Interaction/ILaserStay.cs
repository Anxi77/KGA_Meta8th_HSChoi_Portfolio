using System.Collections.Generic;
using UnityEngine;

namespace LaserSystem2D
{
    public interface ILaserStay 
    {
        void OnLaserStay(LaserBase laserBase, List<RaycastHit2D> hits);
    }
}