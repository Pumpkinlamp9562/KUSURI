﻿using UnityEngine;

namespace VolumetricLights {

    public partial class VolumetricLight : MonoBehaviour {

        static class ShaderParams {
            public static int RayMarchSettings = Shader.PropertyToID("_RayMarchSettings");
            public static int Density = Shader.PropertyToID("_Density");
            public static int FallOff = Shader.PropertyToID("_FallOff");
            public static int RangeFallOff = Shader.PropertyToID("_DistanceFallOff");
            public static int Penumbra = Shader.PropertyToID("_Border");
            public static int NoiseFinalMultiplier = Shader.PropertyToID("_NoiseFinalMultiplier");
            public static int NoiseScale = Shader.PropertyToID("_NoiseScale");
            public static int NoiseStrength = Shader.PropertyToID("_NoiseStrength");
            public static int NoiseTex = Shader.PropertyToID("_NoiseTex");
            public static int BlendDest = Shader.PropertyToID("_BlendDest");
            public static int BlendSrc = Shader.PropertyToID("_BlendSrc");
            public static int FlipDepthTexture = Shader.PropertyToID("_FlipDepthTexture");
            public static int AreaExtents = Shader.PropertyToID("_AreaExtents");
            public static int BoundsExtents = Shader.PropertyToID("_BoundsExtents");
            public static int BoundsCenter = Shader.PropertyToID("_BoundsCenter");
            public static int ExtraGeoData = Shader.PropertyToID("_ExtraGeoData");
            public static int ConeAxis = Shader.PropertyToID("_ConeAxis");
            public static int ConeTipData = Shader.PropertyToID("_ConeTipData");
            public static int WorldToLocalMatrix = Shader.PropertyToID("_WorldToLocal");
            public static int ToLightDir = Shader.PropertyToID("_ToLightDir");
            public static int WindDirection = Shader.PropertyToID("_WindDirection");
            public static int LightColor = Shader.PropertyToID("_LightColor");
            public static int ParticleLightColor = Shader.PropertyToID("_ParticleLightColor");
            public static int ParticleDistanceAtten = Shader.PropertyToID("_ParticleDistanceAtten");
            public static int CookieTexture = Shader.PropertyToID("_Cookie2D");
            public static int BlueNoiseTexture = Shader.PropertyToID("_BlueNoise");
            public static int ShadowTexture = Shader.PropertyToID("_ShadowTexture");
            public static int ShadowIntensity = Shader.PropertyToID("_ShadowIntensity");
            public static int ShadowMatrix = Shader.PropertyToID("_ShadowMatrix");

            // shader keywords
            public const string SKW_NOISE = "VL_NOISE";
            public const string SKW_BLUENOISE = "VL_BLUENOISE";
            public const string SKW_SPOT = "VL_SPOT";
            public const string SKW_SPOT_COOKIE = "VL_SPOT_COOKIE";
            public const string SKW_POINT = "VL_POINT";
            public const string SKW_AREA_RECT = "VL_AREA_RECT";
            public const string SKW_AREA_DISC = "VL_AREA_DISC";
            public const string SKW_SHADOWS = "VL_SHADOWS";
            public const string SKW_DIFFUSION = "VL_DIFFUSION";
            public const string SKW_PHYSICAL_ATTEN = "VL_PHYSICAL_ATTEN";
            public const string SKW_CUSTOM_BOUNDS = "VL_CUSTOM_BOUNDS";
        }
    }
}
