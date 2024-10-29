using UnityEditor;

namespace EditorWrapper
{
    public interface IEditorFactory<in T>
    {
        IDrawable Create(SerializedObject serializedObject, T target);
    }
}