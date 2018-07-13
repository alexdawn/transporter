Shader "Custom/WaterShore" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
	_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}
		SubShader{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
#pragma surface surf Standard alpha

		// Use shader model 3.0 target, to get nicer looking lighting
#pragma target 3.0

		sampler2D _MainTex;

	struct Input {
		float2 uv_MainTex;
		float3 worldPos;
	};

	half _Glossiness;
	half _Metallic;
	fixed4 _Color;

	// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
	// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
	// #pragma instancing_options assumeuniformscaling
	UNITY_INSTANCING_BUFFER_START(Props)
		// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

	void surf(Input IN, inout SurfaceOutputStandard o) {
		float shore = IN.uv_MainTex.y;
		shore = sqrt(shore);

		float2 noiseUV = IN.worldPos.xz + _Time.y * 0.025;
		float4 noise = tex2D(_MainTex, noiseUV * 0.15);

		float distortion1 = noise.x * (1 - shore);
		float foam1 = sin((shore + distortion1) * 10 - _Time.y * 0.5f);
		foam1 *= foam1;
		float distortion2 = noise.y * (1 - shore);
		float foam2 = sin((shore + distortion2) * 10 + (_Time.y * 0.5f) + 2);
		foam2 *= foam2 * 0.7;

		float foam = max(foam1, foam2) * shore;
		fixed4 c = saturate(_Color + foam);
		o.Albedo = c.rgb;
		o.Metallic = _Metallic;
		o.Smoothness = _Glossiness;
		o.Alpha = c.a;
	}
	ENDCG
	}
		FallBack "Diffuse"
}
