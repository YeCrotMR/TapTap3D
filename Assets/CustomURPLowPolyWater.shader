Shader "Custom/URP/LowPolyWater"
{
    Properties
    {
        _Color("Base Color", Color) = (0.2,0.4,0.8,0.5)
        _NormalMap("Normal Map", 2D) = "bump" {}
        _NormalScale("Normal Strength", Range(0,1)) = 0.3
        _WaveSpeed("Wave Speed", Range(0,5)) = 1.0
        _WaveHeight("Wave Height", Range(0,1)) = 0.1
        _FresnelPower("Fresnel Power", Range(1,8)) = 4
        _ReflectionStrength("Reflection Strength", Range(0,1)) = 0.6
        _Transparency("Transparency", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags{ "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalRenderPipeline"}
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            Name "ForwardLit"
            Tags{"LightMode"="UniversalForward"}

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 worldPos   : TEXCOORD0;
                float3 worldNormal: TEXCOORD1;
                float2 uv         : TEXCOORD2;
            };

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            half4 _Color;
            half _NormalScale;
            half _WaveSpeed;
            half _WaveHeight;
            half _FresnelPower;
            half _ReflectionStrength;
            half _Transparency;

            Varyings vert(Attributes v)
            {
                Varyings o;
                float3 pos = v.positionOS.xyz;
                pos.y += sin(_Time.y * _WaveSpeed + pos.x) * _WaveHeight;
                pos.y += cos(_Time.y * _WaveSpeed + pos.z) * _WaveHeight;

                o.worldPos = TransformObjectToWorld(pos);
                o.positionCS = TransformWorldToHClip(o.worldPos);
                o.worldNormal = TransformObjectToWorldNormal(v.normalOS);
                o.uv = v.uv;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                // 法线扰动
                half3 nmap = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, i.uv));
                nmap.xy *= _NormalScale;

                half3 normal = normalize(i.worldNormal + nmap);
                half3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

                // Fresnel
                half fresnel = pow(1 - saturate(dot(viewDir, normal)), _FresnelPower);

                // 采样 Reflection Probe
                half3 reflDir = reflect(-viewDir, normal);
                half4 refl = SAMPLE_TEXTURECUBE(unity_SpecCube0, samplerunity_SpecCube0, reflDir);
                refl.rgb = DecodeHDREnvironment(refl, unity_SpecCube0_HDR);

                half4 baseCol = _Color;
                baseCol.a = _Transparency;

                half4 finalCol = lerp(baseCol, refl, _ReflectionStrength * fresnel);
                finalCol.a = baseCol.a;

                return finalCol;
            }
            ENDHLSL
        }
    }
}
