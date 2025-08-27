Shader "Custom/SectionShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _SectionPlane ("Section Plane", Vector) = (0,1,0,0)
        _SectionColor ("Section Color", Color) = (1,0,0,1)
    }
    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert alpha:fade

        sampler2D _MainTex;
        float4 _SectionPlane;
        fixed4 _SectionColor;

        struct Input {
            float2 uv_MainTex;
            float3 worldPos;
        };

        void surf (Input IN, inout SurfaceOutput o) {
            // Check if point is on the cutting side of the plane
            float dist = dot(IN.worldPos, _SectionPlane.xyz) + _SectionPlane.w;
            
            if (dist < 0) {
                // Discard pixels on one side of the plane
                clip(dist);
            }

            // Normal texture sampling
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}