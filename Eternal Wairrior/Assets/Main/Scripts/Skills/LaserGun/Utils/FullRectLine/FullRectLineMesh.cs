using System;
using System.Collections.Generic;
using UnityEngine;

namespace LaserSystem2D
{
    public class FullRectLineMesh
    {
        private readonly List<Vector2> _points;
        private Vector3[] _vertices = Array.Empty<Vector3>();
        private int[] _tris = Array.Empty<int>();
        private Vector2[] _uv = Array.Empty<Vector2>();
        private readonly Mesh _mesh;
    
        public FullRectLineMesh(List<Vector2> points)
        {
            _points = points;
            _mesh = new Mesh();
        }

        public Mesh Create(float width, float length)
        {
            _mesh.Clear();
            _mesh.vertices = GenerateVertices(width);
            _mesh.triangles = GenerateTris();
            _mesh.uv = GenerateUV(length);

            return _mesh;
        }
    
        private Vector3[] GenerateVertices(float width)
        {
            TryResize(ref _vertices, (_points.Count - 1) * 4);

            for (int i = 0, j = _points.Count - 1; j > 0; ++i, --j)
            {
                Vector3 start = _points[j];
                Vector3 end = _points[j - 1];
                Vector3 direction = end - start;
                Vector3 normal = new Vector3(-direction.y, direction.x, 0).normalized;

                _vertices[i * 4 + 0] = start - normal * width;
                _vertices[i * 4 + 1] = start + normal * width;
                _vertices[i * 4 + 2] = start - normal * width + direction;
                _vertices[i * 4 + 3] = start + normal * width + direction;
            }

            return _vertices;
        }

        private int[] GenerateTris()
        {
            TryResize(ref _tris, (_points.Count - 1) * 6);

            for (int i = 0; i < _points.Count - 1; ++i)
            {
                _tris[i * 6 + 0] = i * 4 + 0;
                _tris[i * 6 + 1] = i * 4 + 2;
                _tris[i * 6 + 2] = i * 4 + 1;
                 
                _tris[i * 6 + 3] = i * 4 + 2;
                _tris[i * 6 + 4] = i * 4 + 3;
                _tris[i * 6 + 5] = i * 4 + 1;
            }
        
            return _tris;
        }

        private Vector2[] GenerateUV(float length)
        {
            float current = 0;
            TryResize(ref _uv, (_points.Count - 1) * 4);

            for (int i = 0, j = _points.Count - 1; j > 0; ++i, --j)
            {
                float subQuadLength = (_points[j - 1] - _points[j]).magnitude;
            
                _uv[i * 4 + 0] = new Vector2(0, current / length);
                _uv[i * 4 + 1] = new Vector2(1, current / length);
                _uv[i * 4 + 2] = new Vector2(0, (current + subQuadLength) / length);
                _uv[i * 4 + 3] = new Vector2(1, (current + subQuadLength) / length);
            
                current += subQuadLength;
            }
        
            return _uv;
        }

        private void TryResize<T>(ref T[] array, int length)
        {
            if (array.Length != length)
            {
                array = new T[length];
            }
        }
    }
}