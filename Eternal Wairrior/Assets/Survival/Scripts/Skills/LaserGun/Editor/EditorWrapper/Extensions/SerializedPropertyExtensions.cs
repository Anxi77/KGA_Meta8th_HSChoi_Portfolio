
using UnityEditor;

namespace EditorWrapper
{
    public static class SerializedPropertyExtensions
    {
        public static SerializedProperty FindPropertyRelative(this SerializedProperty property, params string[] tree)
        {
            for (int i = 0; i < tree.Length; ++i)
            {
                property = property.FindPropertyRelative(tree[i]);
            }

            return property;
        }
    }
}