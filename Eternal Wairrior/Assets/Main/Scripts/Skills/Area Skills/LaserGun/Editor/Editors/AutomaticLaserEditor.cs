using EditorWrapper;
using UnityEditor;

namespace LaserSystem2D
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AutomaticLaser))]
    public class AutomaticLaserEditor : GenericEditor<AutomaticLaser>
    {
        protected override IEditorFactory<AutomaticLaser> EditorFactory { get; } = new AutomaticLaserEditorFactory();
    }
}