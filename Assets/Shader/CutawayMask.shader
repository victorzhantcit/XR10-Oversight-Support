Shader "Custom/CutawayMask"
{
    SubShader
    {
        Tags { "Queue" = "Geometry-1" } // 確保它比普通物件先渲染
        Stencil
        {
            Ref 1          // 設置參考值
            Comp always    // 總是寫入
            Pass replace   // 替換 Stencil 值
        }
        ColorMask 0        // 不渲染顏色
        ZWrite On          // 寫入深度
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
                return fixed4(0, 0, 0, 0); // 不渲染任何顏色
            }
            ENDCG
        }
    }
}
