Shader "Custom/LowPolyWater"
{
    Properties
    {
        _Color ("Base Color", Color) = (0.0,0.4,0.6,0.5)
        _MainTex ("Albedo (unused)", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalScale ("Normal Scale", Float) = 1.0
        _WaveAmplitude ("Wave Amplitude", Float) = 0.1
        _WaveFrequency ("Wave Frequency", Float) = 1.0
        _WaveSpeed ("Wave Speed", Float) = 0.3
        _Transparency ("Transparency", Range(0,1)) = 0.5
        _ReflectionStrength ("Reflection Strength", Range(0,1)) = 0.6
        _FresnelPower ("Fresnel Power", Float) = 3.0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
        LOD 200

        // GrabPass to capture screen for simple reflection
        //GrabPass { }

        Pass
        {
            CGPROGRAM
            #pragma vertex verta
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float4 grabPos : TEXCOORD3; // for GrabPass
            };

            sampler2D _MainTex;
            sampler2D _NormalMap;
            float4 _Color;
            float _NormalScale;
            float _WaveAmplitude;
            float _WaveFrequency;
            float _WaveSpeed;
            float _Transparency;
            float _ReflectionStrength;
            float _FresnelPower;

            v2f vert(appdata v)
            {
                v2f o;

                // 使用 object space 顶点位置
                float3 objPos = v.vertex.xyz;

                // 还是用世界坐标算波浪（相位更稳定）
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float t = _Time.y * _WaveSpeed;
                float wave1 = sin((worldPos.x * _WaveFrequency) + t);
                float wave2 = cos((worldPos.z * _WaveFrequency * 0.7) + t * 1.2);
                float height = (wave1 + wave2) * (_WaveAmplitude * 0.5);

                // 在模型空间里加位移
                objPos.y += height;

                // 转裁剪坐标
                o.pos = UnityObjectToClipPos(float4(objPos,1));

                o.uv = v.uv + float2(t * 0.05, t * 0.02);
                o.worldPos = mul(unity_ObjectToWorld, float4(objPos,1)).xyz;

                // normal 还是按原逻辑
                float3 n = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
                o.worldNormal = n;

                o.grabPos = ComputeGrabScreenPos(o.pos);

                return o;
            }


            sampler2D _GrabTexture;

            fixed4 frag(v2f i) : SV_Target
            {
                // 采样法线贴图
                float3 nmap = tex2D(_NormalMap, i.uv).xyz * 2 - 1;
                nmap.xy *= _NormalScale;
                nmap = normalize(nmap);

                // Fresnel
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 n = normalize(i.worldNormal + nmap);
                float fresnel = pow(1 - saturate(dot(viewDir, n)), _FresnelPower);

                // 反射探针
                float3 reflDir = reflect(-viewDir, n);
                fixed4 refl = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, reflDir);
                refl.rgb = DecodeHDR(refl, unity_SpecCube0_HDR);

                // 基础色
                fixed4 baseCol = _Color;
                baseCol.a = _Transparency;

                // 混合颜色
                fixed4 outCol = lerp(baseCol, refl, _ReflectionStrength * fresnel);
                outCol.a = baseCol.a;

                UNITY_APPLY_FOG(i.pos, outCol);
                return outCol;
            }

            ENDCG
        }
    }
    FallBack "Transparent/VertexLit"
}