Shader "Custom/ClippingShaderWithPlane"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {} // 聲明主紋理
        _SectionPlane ("Section Plane", Vector) = (0, 1, 0, 0) // 裁切平面參數
        _SectionColor ("Section Color", Color) = (1, 0, 0, 1)  // 剖面顏色
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
                float2 uv : TEXCOORD0; // 添加 UV 屬性
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float2 uv : TEXCOORD2; // 傳遞 UV 給片段着色器
            };

            sampler2D _MainTex; // 添加紋理取樣器
            float4 _SectionPlane;
            fixed4 _SectionColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
                o.uv = v.uv; // 傳遞 UV 座標
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 判斷該像素是否位於裁切平面上方
                float d = dot(float4(i.worldPos, 1), _SectionPlane);
                if (d > 0)
                    discard; // 丟棄上方像素

                // 判斷是否為剖面
                if (abs(d) < 0.01)
                {
                    return _SectionColor; // 渲染剖面
                }

                // 渲染正常表面，取樣紋理貼圖
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
