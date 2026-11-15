Shader "Goblfin/ChestShadow"
{
    Properties
    {
        _ShadowColor ("Shadow Color", Color) = (0, 0, 0, 1)
        _EdgeSoftness ("Edge Softness", Range(0.001, 0.5)) = 0.15
        [HDR] _GlowColor ("Glow Color", Color) = (1, 0.6, 0.1, 1)
        _GlowRadius ("Glow Radius", Range(0.05, 1)) = 0.35
        _GlowCenter ("Glow Center (XY)", Vector) = (0.5, 0.5, 0, 0)
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _ShadowColor;
                half _EdgeSoftness;
                half4 _GlowColor;
                half _GlowRadius;
                float4 _GlowCenter;
            CBUFFER_END

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float2 uv = i.uv;

                float2 border = min(uv, 1 - uv);
                half edge = smoothstep(0, _EdgeSoftness, min(border.x, border.y));

                half glow = 1 - smoothstep(0, _GlowRadius, distance(uv, _GlowCenter.xy));

                half3 col = _ShadowColor.rgb + _GlowColor.rgb * glow * glow;
                return half4(col, _ShadowColor.a * edge);
            }
            ENDHLSL
        }
    }
}
