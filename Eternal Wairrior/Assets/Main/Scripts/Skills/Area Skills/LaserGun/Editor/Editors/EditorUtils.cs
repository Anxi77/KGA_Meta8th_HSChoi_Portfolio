using UnityEditor;
using UnityEngine;

namespace LaserSystem2D
{
    public static class EditorUtils
    {
        public static Vector3 GetSceneWorldMousePosition()
        {
            Vector3 worldPosition = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
            worldPosition.z = 0;

            return worldPosition;
        }
    }
}