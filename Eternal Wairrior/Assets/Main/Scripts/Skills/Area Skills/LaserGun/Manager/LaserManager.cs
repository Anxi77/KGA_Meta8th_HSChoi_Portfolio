using System;
using UnityEngine;

namespace LaserSystem2D
{
    [DefaultExecutionOrder(-20)]
    public class LaserManager : MonoBehaviour
    {
        public readonly ReflectingColliders ReflectingColliders = new();
        public readonly HitEffectsPool HitPool = new();
        public readonly LaserPool LaserPool = new();
        private static LaserManager _instance;
        
        public static LaserManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new Exception("Laser manager wasn't initiated");
                }

                return _instance;
            }
        }

        private void Awake()
        {
            _instance = this;
        }

        private void Update()
        {
            LaserPool.Update();
            HitPool.Update();
        }
    }
}