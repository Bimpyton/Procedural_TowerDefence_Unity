Shader "Custom/LitOutline3D_Fixed"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
        _OutlineThickness("Outline Thickness", Range(0.0,0.05)) = 0.02
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        // --- BASE PASS ---
        Pass
        {
            Name "BASE"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vertBase
            #pragma fragment fragBase
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            float4 _BaseColor;

            Varyings vertBase(Attributes IN)
            {
                Varyings OUT;
                float3 posWS = TransformObjectToWorld(IN.positionOS);
                OUT.positionWS = posWS;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.positionCS = TransformWorldToHClip(posWS);
                return OUT;
            }

            float4 fragBase(Varyings IN) : SV_Target
            {
                float3 normal = normalize(IN.normalWS);
                float3 viewDir = normalize(_WorldSpaceCameraPos - IN.positionWS);
                float3 lighting = ShadeSurfaceLambert(normal, float3(1,1,1));
                return float4(_BaseColor.rgb * lighting, _BaseColor.a);
            }
            ENDHLSL
        }

        // --- OUTLINE PASS ---
        Pass
        {
            Name "OUTLINE"
            Tags { "LightMode"="UniversalForward" }
            Cull Front
            ZWrite On
            ZTest LEqual
            HLSLPROGRAM
            #pragma vertex vertOutline
            #pragma fragment fragOutline
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            float _OutlineThickness;
            float4 _OutlineColor;

            Varyings vertOutline(Attributes IN)
            {
                Varyings OUT;
                float3 posWS = TransformObjectToWorld(IN.positionOS);
                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);

                // Screen-space consistent thickness
                float3 viewDir = normalize(_WorldSpaceCameraPos - posWS);
                float ndotv = dot(normalWS, viewDir);
                float thickness = _OutlineThickness / max(abs(ndotv), 0.1);

                float3 extrudedPosWS = posWS + normalWS * thickness;
                OUT.positionWS = extrudedPosWS;
                OUT.normalWS = normalWS;
                OUT.positionCS = TransformWorldToHClip(extrudedPosWS);
                return OUT;
            }

            float4 fragOutline(Varyings IN) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Forward"
}
