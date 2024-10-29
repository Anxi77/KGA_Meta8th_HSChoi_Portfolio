using UnityEngine;

namespace LaserSystem2D
{
    public class LaserMaterial
    {
        private readonly MaterialBridge _bridge = new();
        private readonly ResizableArray<float> _uvToPass = new();
    
        public MaterialPropertyBlock PropertyBlock => _bridge.PropertyBlock;

        public void Clear()
        {
            _bridge.PropertyBlock.Clear();
        }
    
        public void SetShape(float width, float length, Vector2[] uv, float fill, float dissolve)
        {
            Update(uv);
            _bridge.SetFloat("_Fill", fill);
            _bridge.SetFloat("_Dissolve", Mathf.Clamp(dissolve, 0, 1));
            _bridge.SetVector("_QuadSize", new Vector4(width, length));
            _bridge.SetFloatArray("_LineUV", _uvToPass.Items);
            _bridge.SetInt("_Points", _uvToPass.Items.Length);
        }

        private void Update(Vector2[] uv)
        {
            _uvToPass.Clear();
        
            for (int i = 0; i < uv.Length; i += 4)
            {
                _uvToPass.Add(uv[i].y);
            }
        
            _uvToPass.Add(1);
        }
    }
}