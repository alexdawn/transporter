float Foam(float shore, float2 worldXZ, sampler2D noiseTex) {
	shore = sqrt(shore);

	float2 noiseUV = worldXZ + _Time.y * 0.025;
	float4 noise = tex2D(noiseTex, noiseUV * 0.15);

	float distortion1 = noise.x * (1 - shore);
	float foam1 = sin((shore + distortion1) * 10 - _Time.y * 0.5f);
	foam1 *= foam1;
	float distortion2 = noise.y * (1 - shore);
	float foam2 = sin((shore + distortion2) * 10 + (_Time.y * 0.5f) + 2);
	foam2 *= foam2 * 0.7;

	return max(foam1, foam2) * shore;
}

float Waves(float2 worldXZ, sampler2D noiseTex) {
	float2 uv1 = worldXZ;
	uv1.y += _Time.y * 0.0625;
	float4 noise1 = tex2D(noiseTex, uv1 * 0.25);

	float2 uv2 = worldXZ;
	uv2.x += _Time.y * 0.0625;
	float4 noise2 = tex2D(noiseTex, uv2 * 0.25);

	float blendWave = sin(
		(worldXZ.x + worldXZ.y) +
		(noiseTex.y + noiseTex.y) + _Time.y * 0.1);
	blendWave *= blendWave;

	float waves = lerp(noise1.z, noise1.w, blendWave) +
		lerp(noise2.x, noise2.y, blendWave);
	return smoothstep(0.75, 2, waves);
}