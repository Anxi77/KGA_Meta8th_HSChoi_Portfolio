using EditorWrapper;
using UnityEditor;

namespace LaserSystem2D
{
    public class ChainLaserEditorFactory : BaseLaserEditorFactory
    {
        protected override IDrawable CreateFirstElement()
        {
            SerializedProperty keyPoints = SerializedObject
                .FindProperty("_keyPointsProvider")
                .FindPropertyRelative("_keyPoints", "_list");

            return new Section("Key points", Space, new DrawableComposite(new IDrawable[]
            {
                new Property(keyPoints),
                new Property(SerializedObject.FindProperty("_gizmoRadius")),
                new Property(FindPropertyInData("_raycastData", "_shootingSpeed"))
            }));
        }
    }
}