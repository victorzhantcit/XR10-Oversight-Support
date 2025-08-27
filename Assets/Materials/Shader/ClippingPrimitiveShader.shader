Shader "Custom/ClippingPrimitiveShader"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("MainTex", 2D) = "white" {}
        _ClipBoxSide("_ClipBoxSide", Float) = 1
        _FadeAlpha("Fade Alpha", Range(0,1)) = 0.1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent-1" }
        ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Back

        Stencil
        {
            Ref 1
            Comp Always
            Pass Replace
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Color;
            float4x4 _ClipBoxInverseTransform;
            float _ClipBoxSide;
            float _FadeAlpha;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 worldPos : TEXCOORD1;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 localPos = mul(_ClipBoxInverseTransform, float4(i.worldPos, 1)).xyz;
                bool isInside = all(abs(localPos) <= 0.5);

                fixed4 col = tex2D(_MainTex, i.uv) * _Color;

                if ((isInside ? 1 : -1) == _ClipBoxSide)
                {
                    // 被裁剪區 (透明) 並寫入 stencil
                    col.rgb = 1;
                    col.a = _FadeAlpha;
                    return col;
                }

                // 專注區 → 只在 stencil != 1 時畫
                if (_ClipBoxSide == 1)
                {
                    // 內部才顯示 → skip 若 stencil = 1
                    discard;
                }

                return col;
            }
            ENDCG
        }
    }

    Fallback "Diffuse"
}
