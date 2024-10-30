using UnityEngine;

namespace LaserSystem2D
{
    public class ChainToolPointRemoval
    {
        private readonly ObservableList<Vector2> _keyPoints;
        private readonly SelectionInfo _selectionInfo;
        private readonly MouseState _mouseState;

        public ChainToolPointRemoval(ObservableList<Vector2> keyPoints, MouseState mouseState, SelectionInfo selectionInfo)
        {
            _selectionInfo = selectionInfo;
            _mouseState = mouseState;
            _keyPoints = keyPoints;
        }

        public void TryRemove()
        {
            if (_mouseState.CheckRightClick() && _selectionInfo.IsHovered)
            {
                _keyPoints.RemoveAt(_selectionInfo.Index);
            }
        }
    }
}