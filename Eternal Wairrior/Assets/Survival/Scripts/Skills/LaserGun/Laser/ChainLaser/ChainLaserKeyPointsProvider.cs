using System;
using System.Collections.Generic;
using UnityEngine;

namespace LaserSystem2D
{
    [Serializable]
    public class ChainLaserKeyPointsProvider : ILaserKeyPointProvider
    {
        [SerializeField] private ObservableList<Vector2> _keyPoints = new(new List<Vector2>()
        {
            Vector2.zero,
            Vector2.one
        });

        public ObservableList<Vector2> List => _keyPoints;
        public Vector2 this[int index] => _keyPoints[index];
        public int Count => _keyPoints.Count;
    }
}