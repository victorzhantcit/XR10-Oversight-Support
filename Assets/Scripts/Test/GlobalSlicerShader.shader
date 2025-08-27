Shader "Custom/GlobalSlicerShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (1, 1, 1, 1) // �D�C���ݩ�
        _SliceColor ("Slice Color", Color) = (0, 1, 0, 1) // �孱�C��
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
            float4 _Color; // �D�����C��
            float4 _SliceColor; // �孱�C��

            // ���������Ѽ�
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
                // �P�_�����O�_�b���������W��
                if (i.worldPos.y > _GlobalSliceValue)
                    discard; // ���W�蹳��

                // �p�G�孱��n�b�����W�A��V�孱�C��
                if (abs(i.worldPos.y - _GlobalSliceValue) < 0.01)
                {
                    return _SliceColor;
                }

                // ��V���`���誺�C��M�K��
                fixed4 texColor = tex2D(_MainTex, i.uv);
                return texColor * _Color; // �V�X�K�ϻP�D�C��
            }
            ENDCG
        }
    }
}
