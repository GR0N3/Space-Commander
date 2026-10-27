Shader "SpaceCommander/ShieldPowerUp"
{
    Properties
    {
        _BaseColor ("Shield Color", Color) = (0.1, 0.6, 1.0, 1.0)
        _RimColor ("Rim / Energy Color", Color) = (0.4, 0.9, 1.0, 1.0)
        _RimPower ("Rim Exponent", Range(0.5, 8.0)) = 2.0
        _PulseSpeed ("Pulsing Speed", Float) = 3.0
        _ScanlineFreq ("Energy Wave Frequency", Float) = 20.0
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
                float _ScanlineFreq;
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

                // Fresnel rim glow
                float NdotV = saturate(dot(normalWS, viewDirWS));
                float rim = pow(1.0 - NdotV, _RimPower);

                // Animated pulsing energy
                float pulse = sin(_Time.y * _PulseSpeed) * 0.2 + 0.8;

                // Animated energy scanlines along the sphere
                float wave = sin(input.positionWS.y * _ScanlineFreq + _Time.y * 4.0) * 0.15 + 0.85;

                // Combine core shield color with intense rim glow
                float3 finalRGB = _BaseColor.rgb * wave * pulse;
                finalRGB += _RimColor.rgb * rim * _Intensity;

                return half4(finalRGB, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
