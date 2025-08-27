Shader "Custom/DynamicClipping"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _ClippingPlane ("Clipping Plane", Vector) = (0, 1, 0, 0) // �����Ѽ�
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

            float4 _ClippingPlane; // ����������{ (Ax + By + Cz + D = 0)

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz; // �@�ɮy��
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // �P�_�ӹ����O�_�b�����������W
                float d = dot(float4(i.worldPos, 1), _ClippingPlane);
                if (d > 0) // �p�G�b�����W��A���
                    discard;

                return fixed4(1, 1, 1, 1); // ��V�����U�賡��
            }
            ENDCG
        }
    }
}
