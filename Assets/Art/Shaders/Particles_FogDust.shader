Shader "Custom/Particles/FogDust"
{
    Properties
    {
        [HDR]_Color ("Color", Color) = (0.25,0.25,0.25,1)
        _BaseMap ("Texture", 2D) = "white" {}
        _Intensity ("Intensity", Range(0,10)) = 1

        [Toggle(_SOFTPARTICLES_ON)] _SoftParticles ("Enable Soft Particles", Float) = 1
        _SoftParticleDistance ("Soft Particle Distance", Range(0.01, 5)) = 0.5
        _SoftParticleMin ("Soft Particle Min Fade", Range(0,1)) = 0.15

        _FogInfluence ("Fog Influence", Range(0,1)) = 1
        _FogMin ("Fog Min Attenuation", Range(0,1)) = 0.2

        [Toggle(_FLIPBOOK_BLENDING)] _FlipbookBlending ("Flipbook Blending", Float) = 0
        [KeywordEnum(Luminance, Alpha, Multiply)] _DensitySource ("Density Source", Float) = 0
        _DenseDarkening ("Dense Darkening", Range(0,1)) = 0.35
        _DenseExponent ("Dense Exponent", Range(0.5,4)) = 1.75
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }

        Blend One OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_fog
            #pragma shader_feature _SOFTPARTICLES_ON
            #pragma shader_feature _FLIPBOOK_BLENDING
            #pragma shader_feature _DENSITYSOURCE_LUMINANCE _DENSITYSOURCE_ALPHA _DENSITYSOURCE_MULTIPLY

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float4 color : COLOR;
                float4 screenPos : TEXCOORD2;
                float fogCoord : TEXCOORD3;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            TEXTURE2D_X(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);

            float4 _Color;
            float _Intensity;
            float _SoftParticleDistance;
            float _SoftParticleMin;
            float _FogInfluence;
            float _FogMin;
            float _DenseDarkening;
            float _DenseExponent;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv.xy;
                o.uv2 = v.uv.zw;
                o.color = v.color;
                o.screenPos = ComputeScreenPos(o.pos);
                o.fogCoord = ComputeFogFactor(o.pos.z);
                return o;
            }

            float GetLuminance(float3 color)
            {
                return dot(color, float3(0.2126, 0.7152, 0.0722));
            }

            float SampleDensity(float4 tex)
            {
                #if defined(_DENSITYSOURCE_ALPHA)
                    return tex.a;
                #elif defined(_DENSITYSOURCE_MULTIPLY)
                    return GetLuminance(tex.rgb) * tex.a;
                #else
                    return GetLuminance(tex.rgb);
                #endif
            }

            float LinearEyeDepthSafe(float depth)
            {
                return LinearEyeDepth(depth, _ZBufferParams);
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 tex;

                #if defined(_FLIPBOOK_BLENDING)
                {
                    float4 tex1 = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
                    float4 tex2 = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv2);
                    float blend = frac(i.color.a * 8);
                    tex = lerp(tex1, tex2, blend);
                }
                #else
                {
                    tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
                }
                #endif

                float density = saturate(SampleDensity(tex));
                float alpha = density * _Color.a * i.color.a;

                #if defined(_SOFTPARTICLES_ON)
                {
                    float2 uv = i.screenPos.xy / i.screenPos.w;
                    float sceneRawDepth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, uv).r;
                    float sceneDepth = LinearEyeDepthSafe(sceneRawDepth);
                    float particleDepth = LinearEyeDepthSafe(i.screenPos.z / i.screenPos.w);
                    float depthDiff = max(sceneDepth - particleDepth, 0.0001);

                    float fade = smoothstep(0, _SoftParticleDistance, depthDiff);
                    alpha *= max(fade, _SoftParticleMin);
                }
                #endif

                float fogFactor = ComputeFogIntensity(i.fogCoord);
                float fogAtten = lerp(1.0, 1.0 - fogFactor, _FogInfluence);
                alpha *= max(fogAtten, _FogMin);

                float3 rgb = _Color.rgb * i.color.rgb * _Intensity;
                float denseMask = pow(saturate(alpha), _DenseExponent);
                rgb *= lerp(1.0, 1.0 - _DenseDarkening, denseMask);
                rgb *= alpha;

                return float4(rgb, alpha);
            }
            ENDHLSL
        }
    }
}
