Shader "Custom/AdvancedSectionShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _SectionPlane ("Section Plane", Vector) = (0,1,0,0)
        _SectionColor ("Section Color", Color) = (1,0,0,1)
        _SectionEdgeWidth ("Section Edge Width", Float) = 0.05
        _SectionEdgeColor ("Section Edge Color", Color) = (1,1,1,1)
    }
    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert alpha:fade

        sampler2D _MainTex;
        float4 _SectionPlane;
        fixed4 _SectionColor;
        float _SectionEdgeWidth;
        fixed4 _SectionEdgeColor;

        struct Input {
            float2 uv_MainTex;
            float3 worldPos;
        };

        void surf (Input IN, inout SurfaceOutput o) {
            // Calculate distance from section plane
            float dist = dot(IN.worldPos, _SectionPlane.xyz) + _SectionPlane.w;
            
            // Check if point is near the cutting plane for edge rendering
            float edgeDist = abs(dist);

            // Discard pixels on one side of the plane
            clip(dist);

            // Normal texture sampling
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            
            // Apply section color and edge effects
            if (edgeDist < _SectionEdgeWidth) {
                o.Albedo = _SectionEdgeColor.rgb;
                o.Alpha = _SectionEdgeColor.a;
            } else {
                o.Albedo = _SectionColor.rgb;
                o.Alpha = _SectionColor.a;
            }
        }
        ENDCG
    }
    FallBack "Diffuse"
}