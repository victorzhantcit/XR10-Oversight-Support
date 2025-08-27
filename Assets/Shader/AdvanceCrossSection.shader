Shader "Custom/AdvancedCrossSection"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SectionPlane ("Section Plane", Vector) = (0, 1, 0, 0) // 平面方程
        _InteriorColor ("Interior Color", Color) = (1, 0, 0, 1) // 裁切面顏色
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
            };

            float4 _SectionPlane; // 平面方程：Ax + By + Cz + D = 0
            fixed4 _InteriorColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 計算點是否在裁切平面上方
                float d = dot(float4(i.worldPos, 1), _SectionPlane);
                if (d > 0)
                    discard; // 丟棄平面上方的片元

                // 渲染裁切面
                if (abs(d) < 0.01) // 裁切面上的片元
                {
                    return _InteriorColor; // 顯示內部結構顏色
                }

                // 渲染平面下方的內部結構
                return fixed4(1, 1, 1, 1); // 保留原材質顏色
            }
            ENDCG
        }
    }
}
