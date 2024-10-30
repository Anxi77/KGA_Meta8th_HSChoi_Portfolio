using UnityEditor;
using UnityEngine;

namespace LaserSystem2D
{
    public class ChainToolPointSelection
    {
        private readonly SelectionInfo _selectionInfo;
        private readonly MouseState _mouseState;
        private readonly ChainLaser _chainLaser;

        public ChainToolPointSelection(SelectionInfo selectionInfo, MouseState mouseState, ChainLaser chainLaser)
        {
            _selectionInfo = selectionInfo;
            _mouseState = mouseState;
            _chainLaser = chainLaser;
        }

        public void TryUpdateSelectedPointPosition()
        {
            if (_selectionInfo.IsSelected)
            {
                _chainLaser.KeyPoints[_selectionInfo.Index] = EditorUtils.GetSceneWorldMousePosition();
                HandleUtility.Repaint();
            }
        }

        public void TryUnselect()
        {
            if (_mouseState.IsMouseUp)
            {
                _selectionInfo.Unselect();
            }

            _selectionInfo.IsHovered = false;
        }

        public void UpdateSelectionInfo()
        {
            Vector2 mousePosition = EditorUtils.GetSceneWorldMousePosition();

            for (int i = 0; i < _chainLaser.KeyPoints.Count; ++i)
            {
                if (Vector2.Distance(mousePosition, _chainLaser.KeyPoints[i]) < _chainLaser.GizmoHandlesRadius)
                {
                    _selectionInfo.Index = i;

                    if (_mouseState.IsMouseDown)
                    {
                        _selectionInfo.IsSelected = true;
                    }
                    else if (_mouseState.IsHolding == false)
                    {
                        _selectionInfo.IsHovered = true;
                    }
                }
            }
        }
    }
}