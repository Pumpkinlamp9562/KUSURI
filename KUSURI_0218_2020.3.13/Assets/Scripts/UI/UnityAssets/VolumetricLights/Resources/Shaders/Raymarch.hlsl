
TEXTURE2D(_BlueNoise);
SAMPLER(sampler_PointRepeat);

float4 _BlueNoise_TexelSize;

void SetJitter(float4 scrPos) {
    float2 uv = (scrPos.xy / scrPos.w) * _ScreenParams.xy;
#if VL_BLUENOISE
    float2 noiseUV = uv * _BlueNoise_TexelSize.xy + _WindDirection.ww;
    jitter = SAMPLE_TEXTURE2D(_BlueNoise, sampler_PointRepeat, noiseUV).r;
#else
    const float3 magic = float3( 0.06711056, 0.00583715, 52.9829189 );
    jitter = frac( magic.z * frac( dot( uv, magic.xy ) ) );
#endif

}


inline float3 ProjectOnPlane(float3 v, float3 planeNormal) {
    float sqrMag = dot(planeNormal, planeNormal);
    float dt = dot(v, planeNormal);
	return v - planeNormal * dt / sqrMag;
}

inline float3 GetRayStart(float3 wpos) {
    float3 cameraPosition = GetCameraPositionWS();
    #if defined(ORTHO_SUPPORT)
	    float3 cameraForward = UNITY_MATRIX_V[2].xyz;
	    float3 rayStart = ProjectOnPlane(wpos - cameraPosition, cameraForward) + cameraPosition;
        return lerp(cameraPosition, rayStart, unity_OrthoParams.w);
    #else
        return cameraPosition;
    #endif
}


half SampleDensity(float3 wpos) {

#if VL_NOISE
    half density = tex3Dlod(_NoiseTex, float4(wpos * _NoiseScale - _WindDirection.xyz, 0)).r;
    density = saturate( (1.0 - density * _NoiseStrength) * _NoiseFinalMultiplier);
#else
    half density = 1.0;
#endif

    return density * DistanceAttenuation(wpos);
}

void AddFog(float3 wpos, float4 scrPos, float energyStep, half3 baseColor, inout half4 sum) {

   half density = SampleDensity(wpos);


   if (density > 0) {
        half4 fgCol = half4(baseColor, density);
        fgCol.rgb *= fgCol.aaa;
        fgCol.a = min(1.0, fgCol.a);

        fgCol *= energyStep;
        sum += fgCol * (1.0 - sum.a);
   }
}



half4 Raymarch(float3 rayStart, float3 rayDir, float4 scrPos, float t0, float t1) {

    #if VL_DIFFUSION
        #if VL_POINT
            half spec = max(dot(rayDir, normalize(_ConeTipData.xyz - _WorldSpaceCameraPos)), 0);
        #else
            half spec = max(dot(rayDir, _ToLightDir.xyz), 0);
        #endif
        half diffusion = 1.0 + spec * spec * _ToLightDir.w;
        half3 lightColor = _LightColor.rgb * diffusion;
    #else
        half3 lightColor = _LightColor.rgb;
    #endif

    float rs = MIN_STEPPING + max(0, log(t1-t0)) / FOG_STEPPING;
    half4 sum = half4(0,0,0,0);

    float3 wpos = rayStart + rayDir * t0;
    rayDir *= rs;

    half energyStep = min(1.0, _Density * half(rs));

	float t = 0;
    t1 -= t0;
    t1 = min(t1, MAX_ITERATIONS * rs);

    while (t < t1) {
        #if VL_SHADOWS || VL_SPOT_COOKIE
            half3 atten = GetShadowAtten(t / t1);
            AddFog(wpos, scrPos, energyStep, lightColor * atten, sum);
        #else 
            AddFog(wpos, scrPos, energyStep, lightColor, sum);
        #endif
        t += rs;
        wpos += rayDir;
        if (sum.a > 0.99) break;
    }

    // Apply dither
	sum = max(0, sum - (half)(jitter * DITHERING));

    // Final alpha
    sum *= _LightColor.a;

    return sum;
}