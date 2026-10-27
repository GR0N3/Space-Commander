Shader "SpaceCommander/LifePowerUp"
{
    Properties
    {
        _BaseColor ("Health Green Color", Color) = (0.05, 0.85, 0.25, 1.0)
        _RimColor ("Glow Color", Color) = (0.3, 1.0, 0.5, 1.0)
        _RimPower ("Glow Softness", Range(0.5, 8.0)) = 2.0
        _PulseSpeed ("Heartbeat Pulse Speed", Float) = 4.0
        _Intensity ("Glow Intensity", Range(0.5, 4.0)) = 1.8
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _RimColor;
                float _RimPower;
                float _PulseSpeed;
                float _Intensity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS);

                output.positionCS = posInputs.positionCS;
                output.positionWS = posInputs.positionWS;
                output.normalWS = normInputs.normalWS;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(GetCameraPositionWS() - input.positionWS);

                // Heartbeat pulse calculation
                float t = _Time.y * _PulseSpeed;
                float beat = pow(max(0.0, sin(t)), 4.0) * 0.35 + 0.75;

                // Fresnel rim
                float NdotV = saturate(dot(normalWS, viewDirWS));
                float rim = pow(1.0 - NdotV, _RimPower);

                // Vibrant green health composition
                float3 finalRGB = _BaseColor.rgb * beat;
                finalRGB += _RimColor.rgb * rim * _Intensity * beat;

                return half4(finalRGB, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
