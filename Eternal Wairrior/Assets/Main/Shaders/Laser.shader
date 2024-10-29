Shader "Dunno/Laser"
{
    Properties
    {
        _Fill ("Fill", Range(0, 1.0)) = 1
        _Size ("Size", Range(0.0, 1.0)) = 0.3
        _OutlineSize("Outline size", Range(0, 1)) = 0.6
        [HDR] _MiddleColor ("Middle color", Color) = (1,1,0,1)
        [HDR] _OutlineColor("Outline color", Color) = (1,0.01,0,1)
        _OutlineFeather("Outline feather", Range(0.001, 0.5)) = 0.1
        _MiddleFeather("Middle feather", Range(0.001, 0.5)) = 0.01
        _Speed("Speed", float) = 10
        _Amplitude("Amplitude", Range(0, 0.4)) = 0.1
        _Frequency("Frequency", float) = 2
        _StartDistance("Start point distance", Range(0, 10)) = 1
        _EndDistance("End point distance", Range(0, 10)) = 1
        _Radius("Radius", Range(0, 1)) = 1
        [NoScaleOffset] _NoiseTexture("Outline noise texture", 2D) = "" {}
        _NoiseIntensity("Noise intensity", float) = 1.5
        _Dissolve("Dissolve", Range(0, 1)) = 0
        _DissolveWidth("Dissolve width", Range(0, 0.2)) = 0.1
        [HDR] _DissolveColor("Dissolve color", Color) = (1,0.5,0,1)
        [HideInInspector] _MainTex("Needed for disabling sprite renderer warning", 2D) = "" {}
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType"="Transparent"}

        Pass
        {
            ZTest Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct vertexInput
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct vertexOutput
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            float _Fill;
            float _Size;
            float _OutlineSize;
            half4 _MiddleColor;
            half4 _OutlineColor;
            float _NoiseIntensity;
            float _OutlineFeather;
            float _MiddleFeather;
            float _Speed;
            float _Frequency;
            float _Amplitude;
            float _StartDistance;
            float _EndDistance;
            sampler2D _NoiseTexture;
            float _Radius;
            float _Dissolve;
            float _DissolveWidth;
            half4 _DissolveColor;
            float2 _QuadSize;
            float _LineUV[256];
            int _Points;
            
            vertexOutput vert (vertexInput v)
            {
                vertexOutput o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float2 getLaserSize(float2 quadSize)
            {
                return float2(lerp(0, quadSize.x, _Size),
                              lerp(0, quadSize.y, _Fill));
            }

            float getSignedDistanceRoundedBox(float2 position, float2 size, float radius)
            {
                float2 q = abs(position) - size + radius;
                
                return min(max(q.x,q.y), 0) + length(max(q, 0)) - radius;
            }

            float getRadius(float2 laserSize)
            {
                const float max_radius = min(laserSize.x, laserSize.y);
                
                return lerp(0, max_radius, _Radius);
            }

            float2 uvToWorldSpace(float2 uv, float2 quadSize)
            {
                const float y_offset = _Fill - 1;
                uv = uv * 2 - 1;
                uv.x *= quadSize.x;
                uv.y = quadSize.y * (uv.y + y_offset);

                return uv;
            }

            half4 evaluateColor(const float distance, const float outline_size)
            {
                if (distance <= -outline_size)
                {
                    const float lerp_value = smoothstep(-outline_size - _MiddleFeather, -outline_size, distance);
                    half4 color = lerp(_MiddleColor, _OutlineColor, lerp_value);
                    
                    return color;
                }
                
                if (distance <= 0)
                {
                    half4 color = _OutlineColor;
                    color.a = smoothstep(0, -_OutlineFeather, distance);
                    
                    return color;
                }
                
                return half4(0, 0, 0, 0);
            }

            float evaluateParabola(const float x, const float strength)
            {
                return pow(4.0 * x * (1.0 - x), strength);
            }

            float invLerp(float from, float to, float value)
            {
                return (value - from) / (to - from);
            }

            float remap(float origFrom, float origTo, float targetFrom, float targetTo, float value)
            {
                const float inversed = invLerp(origFrom, origTo, value);
                return lerp(targetFrom, targetTo, inversed);
            }

            float getRemappedUV(float uv_y)
            {
                float remapped_uv = 0;
                
                for (int i = 0; i < _Points - 1; i++)
                {
                    if (uv_y >= _LineUV[i] && uv_y <= _LineUV[i + 1])
                    {
                        remapped_uv = remap(_LineUV[i], _LineUV[i + 1], 0, 1, uv_y);
                    }
                }

                return remapped_uv;
            }
            
            float getUVOffset(float2 uv, float2 quadSize)
            {
                const float remapped_uv = getRemappedUV(uv.y);
                const float strength = remapped_uv < 0.5 ? _EndDistance : _StartDistance;
                const float amplitude_strength = evaluateParabola(remapped_uv, strength);
                const float amplitude = _Amplitude * amplitude_strength;
                const float frequency = _Frequency * quadSize.y;
                const float sin_offset = sin(uv.y * frequency + _Speed * _Time.y) * amplitude;
                
                return sin_offset;
            }

            half4 applyDissolve(half4 laser_color, float2 uv)
            {
                if (laser_color.a == 0)
                    return laser_color;

                const float noise_value = tex2D(_NoiseTexture, uv).r * _NoiseIntensity;

                if (noise_value <= _Dissolve)
                {
                    laser_color = _DissolveColor;
                    laser_color.a = smoothstep(_DissolveWidth, 0, _Dissolve - noise_value);
                }
                
                return laser_color;
            }
            
            half4 frag (vertexOutput i) : SV_Target
            {
                float2 uv = i.uv;
                const float2 quad_size = _QuadSize;
                uv.x += getUVOffset(uv, quad_size);
                float2 laser_size = getLaserSize(quad_size);
                const float2 global_uv = uvToWorldSpace(uv, quad_size);
                const float radius = getRadius(laser_size);
                const float distance = getSignedDistanceRoundedBox(global_uv, laser_size, radius);
                const float outline_size = lerp(0, laser_size.x, _OutlineSize);
                const half4 laser_color = evaluateColor(distance, outline_size);
                
                return applyDissolve(laser_color, uv);
                //return half4(i.uv.xy, 0, 1);
            }
            ENDCG
        }
    }
}