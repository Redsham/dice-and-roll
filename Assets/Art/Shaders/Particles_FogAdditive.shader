Shader "Custom/Particles/FogAdditive"
{
    Properties
    {
        [HDR]_Color ("HDR Color", Color) = (1,1,1,1)
        _BaseMap ("Texture", 2D) = "white" {}

        _Intensity ("Intensity", Range(0,10)) = 1

        // Soft Particles
        [Toggle(_SOFTPARTICLES_ON)] _SoftParticles ("Enable Soft Particles", Float) = 1
        _SoftParticleDistance ("Soft Particle Distance", Range(0.01, 5)) = 0.5
        _SoftParticleMin ("Soft Particle Min Fade", Range(0,1)) = 0.15

        // Fog
        _FogInfluence ("Fog Influence", Range(0,1)) = 1
        _FogMin ("Fog Min Attenuation", Range(0,1)) = 0.2

        // Flipbook
        [Toggle(_FLIPBOOK_BLENDING)] _FlipbookBlending ("Flipbook Blending", Float) = 0

        // Alpha Mode
        [KeywordEnum(TextureAlpha, ParticleAlpha, Multiply)] _AlphaMode ("Alpha Mode", Float) = 0
        _AlphaBoost ("Alpha Boost", Range(0,5)) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }

        Blend One One
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

            #pragma shader_feature _ALPHAMODE_TEXTUREALPHA _ALPHAMODE_PARTICLEALPHA _ALPHAMODE_MULTIPLY

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0; // xy = current, zw = next (flipbook)
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

            float4 _Color;
            float _Intensity;

            float _SoftParticleDistance;
            float _SoftParticleMin;

            float _FogInfluence;
            float _FogMin;

            float _AlphaBoost;

            TEXTURE2D_X(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);

            v2f vert (appdata v)
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

            float LinearEyeDepthSafe(float depth)
            {
                return LinearEyeDepth(depth, _ZBufferParams);
            }

            float4 frag (v2f i) : SV_Target
            {
                // ===== FLIPBOOK =====
                float4 tex;

                #if defined(_FLIPBOOK_BLENDING)
                {
                    float4 tex1 = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
                    float4 tex2 = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv2);

                    float blend = frac(i.color.a * 8); // простая эвристика
                    tex = lerp(tex1, tex2, blend);
                }
                #else
                {
                    tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
                }
                #endif

                // ===== COLOR =====
                float3 col = tex.rgb * _Color.rgb * i.color.rgb;

                // ===== ALPHA MODE =====
                float alphaTex = tex.a;
                float alphaParticle = i.color.a;

                float alpha = 1.0;

                #if defined(_ALPHAMODE_TEXTUREALPHA)
                    alpha = alphaTex * alphaParticle;
                #elif defined(_ALPHAMODE_PARTICLEALPHA)
                    alpha = alphaParticle;
                #elif defined(_ALPHAMODE_MULTIPLY)
                    alpha = alphaTex * alphaParticle * _AlphaBoost;
                #endif

                alpha = max(alpha, 0.01);
                col *= alpha;

                // ===== SOFT PARTICLES =====
                #if defined(_SOFTPARTICLES_ON)
                {
                    float2 uv = i.screenPos.xy / i.screenPos.w;

                    float sceneRawDepth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, uv).r;
                    float sceneDepth = LinearEyeDepthSafe(sceneRawDepth);

                    float particleDepth = LinearEyeDepthSafe(i.screenPos.z / i.screenPos.w);

                    float depthDiff = max(sceneDepth - particleDepth, 0.0001);

                    float fade = smoothstep(0, _SoftParticleDistance, depthDiff);
                    fade = max(fade, _SoftParticleMin);

                    col *= fade;
                }
                #endif

                // ===== UNITY FOG =====
                float fogFactor = ComputeFogIntensity(i.fogCoord);
                float fogAtten = lerp(1.0, 1.0 - fogFactor, _FogInfluence);
                fogAtten = max(fogAtten, _FogMin);

                col *= fogAtten;

                // ===== FINAL =====
                col *= _Intensity;

                return float4(col, 1);
            }
            ENDHLSL
        }
    }
}