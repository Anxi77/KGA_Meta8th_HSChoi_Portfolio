using EditorWrapper;
using UnityEditor;

namespace LaserSystem2D
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Laser))]
    public class LaserEditor : GenericEditor<Laser>
    {
        protected override IEditorFactory<Laser> EditorFactory { get; } = new LaserEditorFactory();
    }
}