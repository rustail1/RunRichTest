Shader "Voodoo_LaunchOps/SliderRampSpecRimNormal"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _HColor ("Highlight Color", Color) = (0.785,0.785,0.785,1)
        _SColor ("Shadow Color", Color) = (0.195,0.195,0.195,1)
        _MainTex ("Main Texture", 2D) = "white" {}
        _RampThreshold ("Ramp Threshold", Range(0,1)) = 0.5
        _RampSmooth ("Ramp Smoothing", Range(0.001,1)) = 0.1
        _BumpMap ("Normal map (RGB)", 2D) = "bump" {}
        _SpecColor ("Specular Color", Color) = (0.5,0.5,0.5,1)
        _Smoothness ("Size", Range(0,1)) = 0.2
        _RimColor ("Rim Color", Color) = (0,0,0,0)
        _RimMin ("Rim Min", Range(0,1)) = 1
        _RimMax ("Rim Max", Range(0,5)) = 1
        [HideInInspector] _EmissionColor ("Emission", Color) = (0,0,0,0)
        [HideInInspector] _EmissionMap ("Emission Map", 2D) = "black" {}
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
                half4 _HColor;
                half4 _SColor;
                half _RampThreshold;
                half _RampSmooth;
                half4 _SpecColor;
                half _Smoothness;
                half4 _RimColor;
                half _RimMin;
                half _RimMax;
                half4 _EmissionColor;
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
                half lo = saturate(_RampThreshold - _RampSmooth * 0.5h);
                half hi = saturate(_RampThreshold + _RampSmooth * 0.5h);
                half ramp = smoothstep(lo, max(hi, lo + 0.001h), ndotl);
                half3 toon = lerp(_SColor.rgb, _HColor.rgb, ramp);

                half3 v = GetWorldSpaceNormalizeViewDir(input.positionWS);
                half3 h = normalize(l.direction + v);
                half specPow = lerp(8.0h, 128.0h, saturate(_Smoothness));
                half spec = pow(saturate(dot(n, h)), specPow) * saturate(_Smoothness * 2.0h);

                half fresnel = 1.0h - saturate(dot(n, v));
                half rimDen = max(0.001h, _RimMax - _RimMin);
                half rim = saturate((fresnel - _RimMin) / rimDen);

                half3 rgb = base.rgb * toon * (0.35h + 0.65h * l.color);
                rgb += base.rgb * SampleSH(n) * 0.3h;
                rgb += _SpecColor.rgb * l.color * spec;
                rgb += _RimColor.rgb * rim * _RimColor.a;
                rgb += SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv).rgb * _EmissionColor.rgb;
                rgb = MixFog(rgb, input.fogFactor);
                return half4(rgb, base.a);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
