using System;
using System.Collections.Generic;
using UnityEngine;

namespace LaserSystem2D
{
    [Serializable]
    public class FullRectLine
    {
        [SerializeField] [Range(2, 100)] public int MaxPoints = 15;
        [SerializeField] [Range(0, 2)] private float _width = 0.6f;
        private List<Vector2> _points = new();
        private FullRectLineMesh _meshProvider;
        private MeshFilter _meshFilter;

        public Mesh GeneratedMesh => _meshFilter.sharedMesh;
        public float Length { get; private set; }
        public float Width => _width / 2;

        public void Initialize(MeshFilter meshFilter)
        {
            _meshProvider = new FullRectLineMesh(_points);
            _meshFilter = meshFilter;
            Regenerate();
        }

        public void AddPoint(Vector2 point)
        {
            if (_points.Count < MaxPoints)
            {
                Vector4 worldPoint = _meshFilter.transform.worldToLocalMatrix * new Vector4(point.x, point.y, 0, 1);
                _points.Add(worldPoint);
            }
        }

        public void ClearPoints()
        {
            _points.Clear();
        }

        public void Regenerate()
        {
            if (_points.Count < 1)
            {
                _points.Add(Vector2.zero);
            }
        
            Length = EvaluateLength();
            _meshFilter.sharedMesh = _meshProvider.Create(-Width, Length);
        }

        private float EvaluateLength()
        {
            float length = 0;

            for (int i = 0; i < _points.Count - 1; ++i)
            {
                length += (_points[i + 1] - _points[i]).magnitude;
            }

            return length;
        }
    }
}