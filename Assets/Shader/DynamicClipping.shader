Shader "Custom/DynamicClipping"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _ClippingPlane ("Clipping Plane", Vector) = (0, 1, 0, 0) // 平面參數
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
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
            };

            float4 _ClippingPlane; // 裁切平面方程 (Ax + By + Cz + D = 0)

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz; // 世界座標
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 判斷該像素是否在裁切平面之上
                float d = dot(float4(i.worldPos, 1), _ClippingPlane);
                if (d > 0) // 如果在平面上方，丟棄
                    discard;

                return fixed4(1, 1, 1, 1); // 渲染平面下方部分
            }
            ENDCG
        }
    }
}
