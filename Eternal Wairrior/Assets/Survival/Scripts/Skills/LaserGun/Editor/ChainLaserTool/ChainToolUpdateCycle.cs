using UnityEngine;

namespace LaserSystem2D
{
    public class ChainToolUpdateCycle
    {
        private readonly ChainToolPointSelection _pointSelection;
        private readonly ChainToolPointInserting _pointInserting;
        private readonly ChainToolGhostPosition _ghostPosition;
        private readonly ChainToolPointRemoval _pointRemoval;
        private readonly MouseState _mouseState;
        private readonly ChainToolView _view;

        public ChainToolUpdateCycle(ChainLaser chainLaser)
        {
            SelectionInfo selectionInfo = new();
            _mouseState = new MouseState();
            _view = new ChainToolView(selectionInfo, chainLaser);
            _ghostPosition = new ChainToolGhostPosition(chainLaser, selectionInfo);
            _pointSelection = new ChainToolPointSelection(selectionInfo, _mouseState, chainLaser);
            _pointRemoval = new ChainToolPointRemoval(chainLaser.KeyPoints, _mouseState, selectionInfo);
            _pointInserting = new ChainToolPointInserting(chainLaser.KeyPoints, _mouseState, selectionInfo);
        }

        public void Update()
        {
            _mouseState.Update();
            _pointSelection.TryUnselect();
            _pointSelection.UpdateSelectionInfo();
            _pointSelection.TryUpdateSelectedPointPosition();
            _pointRemoval.TryRemove();
            _view.DrawDottedLines();
            _view.DrawKeyPoints();

            if (_ghostPosition.TryHover(out Vector2 ghostPosition, out int segmentIndex))
            {
                _view.DrawGhost(ghostPosition);
                _pointInserting.TryInsert(ghostPosition, segmentIndex);
            }
        }
    }
}