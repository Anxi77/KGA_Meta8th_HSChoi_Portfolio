using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace LaserSystem2D
{
    [EditorTool("Chain laser editing", typeof(ChainLaser))]
    public class ChainLaserTool : EditorTool
    {
        private ChainToolUpdateCycle _updateCycle;
        
        public override GUIContent toolbarIcon => EditorGUIUtility.IconContent("d_EdgeCollider2D Icon");

        private void OnEnable()
        {
            ChainLaser chainLaser = (ChainLaser)target;
            _updateCycle = new ChainToolUpdateCycle(chainLaser);
        }

        [Shortcut("Chain laser editing", KeyCode.U)]
        private static void PlatformToolShortcut()
        {
            if (Selection.GetFiltered<ChainLaser>(SelectionMode.TopLevel).Length > 0)
            {
                ToolManager.SetActiveTool<ChainLaserTool>();
            }
        }

        public override void OnToolGUI(EditorWindow window)
        {
            if (window is SceneView)
            {
                _updateCycle.Update();
                AddDefaultControl();
            }
        }

        private void AddDefaultControl()
        {
            if (Event.current.type == EventType.Layout)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            }
        }
    }
}