Shader "Custom/GlobalSlicerShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (1, 1, 1, 1) // 主顏色屬性
        _SliceColor ("Slice Color", Color) = (0, 1, 0, 1) // 剖面顏色
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

            sampler2D _MainTex;
            float4 _Color; // 主材質顏色
            float4 _SliceColor; // 剖面顏色

            // 全局裁切參數
            float _GlobalSliceValue;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 判斷像素是否在裁切平面上方
                if (i.worldPos.y > _GlobalSliceValue)
                    discard; // 丟棄上方像素

                // 如果剖面剛好在切面上，渲染剖面顏色
                if (abs(i.worldPos.y - _GlobalSliceValue) < 0.01)
                {
                    return _SliceColor;
                }

                // 渲染正常材質的顏色和貼圖
                fixed4 texColor = tex2D(_MainTex, i.uv);
                return texColor * _Color; // 混合貼圖與主顏色
            }
            ENDCG
        }
    }
}
