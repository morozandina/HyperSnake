// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/AlwaysVisible"
{
    Properties {
        
    }
    SubShader {
        Tags {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent+0"
        }
        
        Pass {
            Cull Off
            ZWrite Off
            ZTest Always
 
            HLSLPROGRAM
            #pragma vertex vert2
            #pragma fragment frag2
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
            };
     
            struct v2f
            {
                float4 vertex : SV_POSITION;
            };
     
            v2f vert2(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }
     
            fixed4 frag2(v2f i) : SV_Target
            {
                return fixed4(1, 0.1462264, 0.1, 1);
            }
     
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}
