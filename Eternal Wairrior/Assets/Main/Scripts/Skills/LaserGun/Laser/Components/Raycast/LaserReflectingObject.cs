using UnityEngine;

namespace LaserSystem2D
{
    [RequireComponent(typeof(Collider2D))]
    public class LaserReflectingObject : MonoBehaviour
    {
        private void Start()
        {
            Collider2D[] colliders = GetComponents<Collider2D>();
            LaserManager.Instance.ReflectingColliders.Add(colliders);
        }
    }
}