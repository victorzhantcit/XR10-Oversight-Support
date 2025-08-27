Shader "Custom/AdvancedCrossSection"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SectionPlane ("Section Plane", Vector) = (0, 1, 0, 0) // ������{
        _InteriorColor ("Interior Color", Color) = (1, 0, 0, 1) // �������C��
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

            float4 _SectionPlane; // ������{�GAx + By + Cz + D = 0
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
                // �p���I�O�_�b���������W��
                float d = dot(float4(i.worldPos, 1), _SectionPlane);
                if (d > 0)
                    discard; // ��󥭭��W�誺����

                // ��V������
                if (abs(d) < 0.01) // �������W������
                {
                    return _InteriorColor; // ��ܤ������c�C��
                }

                // ��V�����U�誺�������c
                return fixed4(1, 1, 1, 1); // �O�d������C��
            }
            ENDCG
        }
    }
}
