Shader "Custom/Skybox/GradientFog"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (0.3, 0.4, 0.45, 1)
        _HorizonColor ("Horizon Color", Color) = (0.6, 0.65, 0.6, 1)
        _BottomColor ("Bottom Color", Color) = (0.4, 0.5, 0.45, 1)

        _GradientPower ("Gradient Power", Range(0.1, 5)) = 1.5

        _SunColor ("Sun Color", Color) = (1, 0.7, 0.4, 1)
        _SunSize ("Sun Size", Range(0.001, 0.1)) = 0.02
        _SunSoftness ("Sun Softness", Range(0.1, 10)) = 4

        _FogStrength ("Fog Strength", Range(0, 2)) = 0.8
    }

    SubShader
    {
        Tags
        {
            "Queue"="Background" "RenderType"="Opaque"
        }
        Cull Off ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 dir : TEXCOORD0;
            };

            float4 _TopColor;
            float4 _HorizonColor;
            float4 _BottomColor;
            float _GradientPower;

            float4 _SunColor;
            float _SunSize;
            float _SunSoftness;

            float _FogStrength;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.dir = normalize(v.vertex.xyz);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 dir = normalize(i.dir);

                float h = dir.y * 0.5 + 0.5;
                h = pow(saturate(h), _GradientPower);

                float3 col = lerp(_BottomColor.rgb, _HorizonColor.rgb, h);
                col = lerp(col, _TopColor.rgb, h * h);

                Light mainLight = GetMainLight();
                float3 sunDir = normalize(mainLight.direction);

                float sunDot = dot(dir, sunDir);
                float sun = smoothstep(1 - _SunSize, 1, sunDot);
                sun = pow(sun, _SunSoftness);

                col += _SunColor.rgb * sun;

                float fog = saturate(1 - abs(dir.y));
                col = lerp(col, _HorizonColor.rgb, fog * _FogStrength);

                return float4(col, 1);
            }
            ENDHLSL
        }
    }
}