#ifndef VOLUMETRIC_LIGHTS_COMMONS_URP
#define VOLUMETRIC_LIGHTS_COMMONS_URP

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#include "Options.hlsl"

#ifndef SHADER_API_PS4
CBUFFER_START(UnityPerMaterial)
#endif

float4 _ConeTipData, _ConeAxis;
half4 _ExtraGeoData;
float3 _BoundsCenter, _BoundsExtents;
half4 _ToLightDir;

float jitter;
float _NoiseScale;
half _NoiseStrength, _NoiseFinalMultiplier, _Border, _DistanceFallOff;
half3 _FallOff;
half4 _Color;
float4 _AreaExtents;

float4 _RayMarchSettings;
float4 _WindDirection;
half4 _LightColor;
half  _Density;
int _FlipDepthTexture;

#ifndef SHADER_API_PS4
CBUFFER_END
#endif

sampler3D _NoiseTex;

#define FOG_STEPPING _RayMarchSettings.x
#define DITHERING _RayMarchSettings.y
#define JITTERING _RayMarchSettings.z
#define MIN_STEPPING _RayMarchSettings.w

// Common URP code
#define VR_ENABLED defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED) || defined(SINGLE_PASS_STEREO)

#if defined(USE_ALTERNATE_RECONSTRUCT_API) || VR_ENABLED 
   #define CLAMP_RAY_DEPTH(rayStart, scrPos, t1) ClampRayDepthAlt(rayStart, scrPos, t1)
#else
   #define CLAMP_RAY_DEPTH(rayStart, scrPos, t1) ClampRayDepth(rayStart, scrPos, t1)
#endif



inline void ClampRayDepth(float3 rayStart, float4 scrPos, inout float t1) {
    float2 uv =  scrPos.xy / scrPos.w;

    // World position reconstruction
    float depth  = SampleSceneDepth(_FlipDepthTexture ? float2(uv.x, 1.0 - uv.y) : uv);
    float4 raw   = mul(UNITY_MATRIX_I_VP, float4(uv * 2 - 1, depth, 1));
    float3 wpos  = raw.xyz / raw.w;

    float z = distance(rayStart, wpos);
    t1 = min(t1, z);
} 


// Alternate reconstruct API; URP 7.4 doesn't set UNITY_MATRIX_I_VP correctly in VR, so we need to use this alternate method

inline float GetLinearEyeDepth(float2 uv) {
    float rawDepth = SampleSceneDepth(_FlipDepthTexture ? float2(uv.x, 1.0 - uv.y) : uv);
	float sceneLinearDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
    #if defined(ORTHO_SUPPORT)
        #if UNITY_REVERSED_Z
              rawDepth = 1.0 - rawDepth;
        #endif
        float orthoDepth = lerp(_ProjectionParams.y, _ProjectionParams.z, rawDepth);
        sceneLinearDepth = lerp(sceneLinearDepth, orthoDepth, unity_OrthoParams.w);
    #endif
    return sceneLinearDepth;
}


void ClampRayDepthAlt(float3 rayStart, float4 scrPos, inout float t1) {
    float2 uv =  scrPos.xy / scrPos.w;
    float vz = GetLinearEyeDepth(uv);
    #if defined(ORTHO_SUPPORT)
        if (unity_OrthoParams.w) {
            t1 = min(t1, vz);
            return;
        }
    #endif
    float2 p11_22 = float2(unity_CameraProjection._11, unity_CameraProjection._22);
    float2 suv = uv;
    #if UNITY_SINGLE_PASS_STEREO
        // If Single-Pass Stereo mode is active, transform the
        // coordinates to get the correct output UV for the current eye.
        float4 scaleOffset = unity_StereoScaleOffset[unity_StereoEyeIndex];
        suv = (suv - scaleOffset.zw) / scaleOffset.xy;
    #endif
    float3 vpos = float3((suv * 2 - 1) / p11_22, -1) * vz;
    float4 wpos = mul(unity_CameraToWorld, float4(vpos, 1));
    float z = distance(rayStart, wpos.xyz);
    t1 = min(t1, z);
}



#endif // VOLUMETRIC_LIGHTS_COMMONS_URP

