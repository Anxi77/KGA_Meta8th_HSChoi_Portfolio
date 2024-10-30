using UnityEngine;

namespace LaserSystem2D
{
    public class ChainToolPointInserting
    {
        private readonly ObservableList<Vector2> _keyPoints;
        private readonly SelectionInfo _selectionInfo;
        private readonly MouseState _mouseState;

        public ChainToolPointInserting(ObservableList<Vector2> keyPoints, MouseState mouseState, SelectionInfo selectionInfo)
        {
            _selectionInfo = selectionInfo;
            _mouseState = mouseState;
            _keyPoints = keyPoints;
        }

        public void TryInsert(Vector2 ghostPosition, int segmentIndex)
        {
            if (_mouseState.IsMouseDown)
            {
                InsertPoint(ghostPosition, segmentIndex);
                _selectionInfo.IsSelected = true;
                _selectionInfo.Index = segmentIndex;
            }
        }

        private void InsertPoint(Vector2 ghostPosition, int segmentIndex)
        {
            _keyPoints.Add(Vector2.zero);

            for (int i = _keyPoints.Count - 1; i >= segmentIndex; --i)
            {
                _keyPoints[i] = _keyPoints[i - 1];
            }

            _keyPoints[segmentIndex] = ghostPosition;
        }
    }
}