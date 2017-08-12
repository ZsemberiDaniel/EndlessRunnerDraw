Shader "MyShader" {
    SubShader {
		Tags { "RenderType" = "Opaque" }
		CGPROGRAM
		// Uses the Labertian lighting model
		#pragma surace surf Lambert

        struct Input {
            float4 color : COLOR;
        };
        void surf(Input IN, inout SurfaceOutput o) {
			o.Albedo = 1;
        }
		ENDCG
    }
    Fallback "Diffuse"
}
