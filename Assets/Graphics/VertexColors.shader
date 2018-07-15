Shader "Custom/VertexColors" {
	Properties {
		_Color ("Color1", Color) = (1,1,1,1)
		_NoiseChannel("Noise Channel", Color) = (1,1,1,1)
		_Target1("Target Colour 1", Color) = (1,1,1,1)
		_Target2("Target Colour 2", Color) = (1,1,1,1)
		_Target3("Target Colour 3", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_NoiseTex("Noise (RGB)", 2D) = "white" {}
		_NoiseCut("Noise Cutoff", Range(0, 1)) = 0.75
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _NoiseTex;

		struct Input {
			float2 uv_MainTex;
			float4 color : COLOR;
			float3 worldPos;
		};

		half _Glossiness;
		half _Metallic;
		half _NoiseCut;
		fixed4 _Color;
		fixed4 _NoiseChannel;
		fixed4 _Target1;
		fixed4 _Target2;
		fixed4 _Target3;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			float2 uv1 = IN.worldPos.xz;
			float4 noise1 = tex2D(_NoiseTex, uv1 * 0.1) * _NoiseChannel;
			float4 colorMask = 0;
			if (noise1.r >= noise1.g && noise1.r >= noise1.b)
			{
				colorMask.r = noise1.r;
			}
			else if (noise1.g > noise1.r && noise1.g > noise1.b)
			{
				colorMask.g = noise1.g;
			}
			else if (noise1.b > noise1.g && noise1.b > noise1.r)
			{
				colorMask.b = noise1.b;
			}
			else
			{
				colorMask = 1;
			}
			fixed4 mask1 = step(1 - colorMask.r, _NoiseCut);
			fixed4 mask2 = step(1 - colorMask.g, _NoiseCut);
			fixed4 mask3 = step(1 - colorMask.b, _NoiseCut);
			fixed4 coveringTextures = mask1 + mask2 + mask3;
			fixed4 coveringColor = mask1 * _Target1 + mask2 * _Target2 + mask3 * _Target3;
			float mask = max(max(coveringTextures.r, coveringTextures.g), coveringTextures.b);
			// Albedo comes from a texture tinted by color
			fixed4 mainTexture = tex2D(_MainTex, IN.uv_MainTex) * _Color * IN.color * step(mask, _NoiseCut);
			fixed4 c = mainTexture + coveringColor * (1-step(mask, _NoiseCut));
			//fixed4 c = mainTexture;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
