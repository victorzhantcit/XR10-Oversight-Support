Shader "Custom/AdvancedClipping"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _SectionPlane ("Clipping Plane", Vector) = (0, 1, 0, 0) // �������
        _InteriorColor ("Interior Color", Color) = (1, 0, 0, 1) // �孱�C��
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
                float2 uv : TEXCOORD0; // UV �y��
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _SectionPlane;
            fixed4 _InteriorColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz; // �@�ɮy��
                o.worldNormal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // �P�_�O�_�b���������W��
                float d = dot(float4(i.worldPos, 1), _SectionPlane);
                if (d > 0)
                    discard; // ��󥭭��W�賡��

                // ��V�孱
                if (abs(d) < 0.01) // �孱�d��
                {
                    return _InteriorColor; // ��ܭ孱�C��
                }

                // ��V�����U�誺���`����
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
