using UnityEngine;

namespace LaserSystem2D
{
    public class Utils
    {
        public static T[] FindObjects<T>() where T : MonoBehaviour
        {
            #if UNITY_2023_1_OR_NEWER
            return Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            #else
            return Object.FindObjectsOfType<T>();
            #endif
        }

        public static T FindObject<T>() where T : MonoBehaviour
        {
            #if UNITY_2023_1_OR_NEWER
            return Object.FindFirstObjectByType<T>();
            #else   
            return Object.FindObjectOfType<T>();
            #endif
        }
        
        public static int BoxCast(BoxCastData data, RaycastHit2D[] hits)
        {
            #if UNITY_2023_1_OR_NEWER
            return Physics2D.BoxCast(data.Origin, data.Size, data.Angle, data.Direction, new ContactFilter2D
            {
                layerMask = data.Mask,
                useLayerMask = true,
            }, hits, data.Distance);
            #else   
            return Physics2D.BoxCastNonAlloc(data.Origin, data.Size, data.Angle,data.Direction, hits, data.Distance, data.Mask);
            #endif
        }
    }
}