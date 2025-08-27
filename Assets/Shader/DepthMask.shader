Shader "Custom/DepthMask"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        ColorMask 0 // ����V�����C��
        ZWrite On   // �ҥβ`�׼g�J
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return fixed4(0, 0, 0, 0); // ����V�����C��
            }
            ENDCG
        }
    }
}

