using UnityEditor;
using UnityEngine;

namespace EditorWrapper
{
    public abstract class GenericEditor<T> : Editor where T : Object
    {
        private IDrawable _editor;

        private void OnEnable()
        {
            _editor = EditorFactory.Create(serializedObject, (T)target);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            _editor.Draw();

            serializedObject.ApplyModifiedProperties();
        }

        protected abstract IEditorFactory<T> EditorFactory { get; }
    }
}