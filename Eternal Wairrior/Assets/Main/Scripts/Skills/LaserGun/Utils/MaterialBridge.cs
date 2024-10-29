using System.Collections.Generic;
using UnityEngine;

namespace LaserSystem2D
{
    public class MaterialBridge
    {
        private readonly Dictionary<string, int> _properties = new();

        public MaterialBridge()
        {
            PropertyBlock = new MaterialPropertyBlock();
        }

        public MaterialPropertyBlock PropertyBlock { get; }

        public void SetVector(string name, Vector4 value)
        {
            PropertyBlock.SetVector(GetPropertyId(name), value);
        }
    
        public void SetFloat(string name, float value)
        {
            PropertyBlock.SetFloat(GetPropertyId(name), value);
        }

        public void SetFloatArray(string name, float[] value)
        {
            PropertyBlock.SetFloatArray(GetPropertyId(name), value);
        }

        public void SetInt(string name, int value)
        {
            PropertyBlock.SetInt(GetPropertyId(name), value);
        }

        public float GetFloat(string name)
        {
            return PropertyBlock.GetFloat(GetPropertyId(name));
        }

        private int GetPropertyId(string name)
        {
            if (_properties.ContainsKey(name) == false)
            {
                int id = Shader.PropertyToID(name);
                _properties[name] = id;
            }

            return _properties[name];
        }
    }
}