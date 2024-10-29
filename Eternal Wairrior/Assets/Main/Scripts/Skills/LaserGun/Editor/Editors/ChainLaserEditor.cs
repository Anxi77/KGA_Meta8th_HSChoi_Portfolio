using EditorWrapper;
using UnityEditor;

namespace LaserSystem2D
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ChainLaser))]
    public class ChainLaserEditor : GenericEditor<ChainLaser>
    {
        protected override IEditorFactory<ChainLaser> EditorFactory { get; } = new ChainLaserEditorFactory();
    }
}