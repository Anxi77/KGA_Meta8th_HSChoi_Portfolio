using UnityEditor;
using UnityEngine;

namespace LaserSystem2D
{
    public class ChainToolGhostPosition
    {
        private readonly float _minDistanceToLine = 1f;
        private readonly SelectionInfo _selectionInfo;
        private readonly ChainLaser _laser;

        public ChainToolGhostPosition(ChainLaser laser, SelectionInfo selectionInfo)
        {
            _selectionInfo = selectionInfo;
            _laser = laser;
        }

        public bool TryHover(out Vector2 ghostPointPosition, out int segmentIndex)
        {
            ghostPointPosition = Vector2.zero;
            segmentIndex = -1;
            
            return _selectionInfo.IsNotSelected &&
                   IsMouseCloseToSegment(out segmentIndex) && 
                   IsProjectionWithinSegment(segmentIndex, out ghostPointPosition);
        }   

        private bool IsProjectionWithinSegment(int index, out Vector2 projection)
        {
            Vector2 mousePosition = EditorUtils.GetSceneWorldMousePosition();
            
            return mousePosition.TryProjectOntoSegment(out projection, _laser.KeyPoints[index - 1], _laser.KeyPoints[index]);
        }

        private bool IsMouseCloseToSegment(out int segmentIndex)
        {
            float minDistanceToLine = float.MaxValue;
            segmentIndex = -1;
            
            for (int i = 1; i < _laser.KeyPoints.Count; ++i)
            {
                Vector2 firstPoint = _laser.KeyPoints[i - 1];
                Vector2 secondPoint = _laser.KeyPoints[i];
                Vector3 mousePosition = EditorUtils.GetSceneWorldMousePosition();
                float distanceToLine = HandleUtility.DistancePointToLineSegment(mousePosition, firstPoint, secondPoint);

                if (distanceToLine < minDistanceToLine)
                {
                    minDistanceToLine = distanceToLine;
                    segmentIndex = i;
                }
            }

            if (minDistanceToLine > _minDistanceToLine)
            {
                segmentIndex = -1;
            }
            
            return minDistanceToLine <= _minDistanceToLine;
        }
    }
}