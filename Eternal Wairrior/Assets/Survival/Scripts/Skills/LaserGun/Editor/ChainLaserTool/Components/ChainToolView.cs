using UnityEditor;
using UnityEngine;

namespace LaserSystem2D
{
    public class ChainToolView
    {
        private readonly ChainToolViewData _viewData = new();
        private readonly SelectionInfo _selectionInfo;
        private readonly ChainLaser _chainLaser;

        public ChainToolView(SelectionInfo selectionInfo, ChainLaser chainLaser)
        {
            _selectionInfo = selectionInfo;
            _chainLaser = chainLaser;
        }
        
        public void DrawKeyPoints()
        {
            for (int i = 0; i < _chainLaser.KeyPoints.Count; i++)
            {
                DrawCircle(GetKeyPointColor(i), _chainLaser.KeyPoints[i]);
            }
        }

        public void DrawGhost(Vector2 position)
        {
            DrawCircle(_viewData.GhostColor, position);
        }
    
        public void DrawDottedLines()
        {
            Handles.color = Color.black;
            for (int i = 1; i < _chainLaser.KeyPoints.Count; ++i)
            {
                Vector2 keyPoint = _chainLaser.KeyPoints[i - 1];
                Vector2 nextKeyPoint = _chainLaser.KeyPoints[i];

                Handles.DrawDottedLine(keyPoint, nextKeyPoint, 4f);
            }
        }

        private void DrawCircle(Color color, Vector2 position)
        {
            Handles.color = color;
            Handles.DrawSolidArc(position, Vector3.forward, Vector3.up, 360, _chainLaser.GizmoHandlesRadius);
        }
    
        private Color GetKeyPointColor(int index)
        {
            if (index != _selectionInfo.Index)
            {
                return _viewData.DefaultColor;
            }

            if (_selectionInfo.IsHovered)
            {
                return _viewData.HoverColor;
            }

            if (_selectionInfo.IsSelected)
            {
                return _viewData.SelectionColor;
            }

            return _viewData.DefaultColor;
        }
    }
}