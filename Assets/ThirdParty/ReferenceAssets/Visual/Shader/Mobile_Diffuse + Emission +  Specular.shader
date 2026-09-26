Shader "Mobile/Diffuse + Emission +  Specular"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _EmissionColor ("Emission Color", Color) = (0,0,0,1)
        _EmissionMap ("Emission", 2D) = "black" {}
        _Gloss ("Gloss", Range(0,1)) = 0.078125
        _Shininess ("Shininess", Range(0,1)) = 0.078125
        [HideInInspector] _SrcBlend ("Src Blend", Float) = 1
        [HideInInspector] _DstBlend ("Dst Blend", Float) = 0
        [HideInInspector] _ZWrite ("ZWrite", Float) = 1
        [HideInInspector] _Cull ("Cull", Float) = 2
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_EmissionMap); SAMPLER(sampler_EmissionMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half4 _EmissionColor;
                half _Gloss;
                half _Shininess;
            CBUFFER_END

            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; half3 normalWS:TEXCOORD1; float2 uv:TEXCOORD2; half fogFactor:TEXCOORD3; };

            Varyings Vert(Attributes input)
            {
                Varyings o;
                VertexPositionInputs p = GetVertexPositionInputs(input.positionOS.xyz);
                o.positionCS = p.positionCS;
                o.positionWS = p.positionWS;
                o.normalWS = TransformObjectToWorldNormal(input.normalOS);
                o.uv = TRANSFORM_TEX(input.uv, _MainTex);
                o.fogFactor = ComputeFogFactor(p.positionCS.z);
                return o;
            }

            half4 Frag(Varyings input):SV_Target
            {
                half4 base = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;
                half3 n = normalize(input.normalWS);
                Light l = GetMainLight();
                half ndotl = saturate(dot(n, l.direction));
                half3 v = GetWorldSpaceNormalizeViewDir(input.positionWS);
                half3 h = normalize(l.direction + v);
                half specPow = lerp(8.0h, 128.0h, saturate(_Shininess));
                half spec = pow(saturate(dot(n, h)), specPow) * _Gloss;
                half3 lit = base.rgb * (SampleSH(n) + l.color * (0.35h + 0.65h * ndotl)) + l.color * spec;
                half3 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv).rgb * _EmissionColor.rgb;
                half3 rgb = MixFog(lit + emission, input.fogFactor);
                return half4(rgb, base.a);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
