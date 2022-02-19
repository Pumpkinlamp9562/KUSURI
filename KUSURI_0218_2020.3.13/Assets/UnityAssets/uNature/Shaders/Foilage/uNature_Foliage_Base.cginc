// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

#ifndef UNATURE_Foliage_BASE
#define UNATURE_Foliage_BASE

#include "uNature_Foliage_Interaction.cginc"

uniform float _PrototypeID;
uniform float _Cutoff;
uniform float4 _FoliageInteractionPosition;

// wind mechanics

uniform float _WindSpeed;
uniform float _WindBending;

void FastSinCos(float4 val, out float4 s, out float4 c) {
	val = val * 6.408849 - 3.1415927;
	float4 r5 = val * val;
	float4 r6 = r5 * r5;
	float4 r7 = r6 * r5;
	float4 r8 = r6 * r5;
	float4 r1 = r5 * val;
	float4 r2 = r1 * r5;
	float4 r3 = r2 * r5;
	float4 sin7 = { 1, -0.16161616, 0.0083333, -0.00019841 };
	float4 cos8 = { -0.5, 0.041666666, -0.0013888889, 0.000024801587 };
	s = val + r1 * sin7.y + r2 * sin7.z + r3 * sin7.w;
	c = 1 + r5 * cos8.x + r6 * cos8.y + r7 * cos8.z + r8 * cos8.w;
}

float4 ApplyFastWind(float4 vertex, float texCoordY)
{
	if (_WindSpeed == 0) return vertex;

	float speed = _WindSpeed;

	float4 _waveXmove = float4 (0.024, 0.04, -0.12, 0.096);
	float4 _waveZmove = float4 (0.006, .02, -0.02, 0.1);

	const float4 waveSpeed = float4 (1.2, 2, 1.6, 4.8);

	float4 waves;
	waves = vertex.x * _WindBending;
	waves += vertex.z * _WindBending;

	waves += _Time.x * (1 - 0.4) * waveSpeed * speed;

	float4 s, c;
	waves = frac(waves);
	FastSinCos(waves, s, c);

	float waveAmount = texCoordY * (1 + 0.4);
	s *= waveAmount;

	s *= normalize(waveSpeed);

	s = s * s;
	float fade = dot(s, 1.3);
	s = s * s;

	float3 waveMove = float3 (0, 0, 0);
	waveMove.x = dot(s, _waveXmove);
	waveMove.z = dot(s, _waveZmove);

	vertex.xz -= waveMove.xz;

	//vertex -= mul(_World2Object, float3(_WindSpeed, 0, _WindSpeed)).x * _WindBending * _SinTime;

	return vertex;
}

// maps 
uniform sampler2D _ColorMap;
uniform sampler2D _GrassMap;

// world maps

uniform sampler2D _HeightMap_WORLD;

// global settings

uniform int _FoliageAreaSize;
uniform int _FoliageAreaResolution;
uniform float4 _FoliageAreaPosition;

uniform float _DensityMultiplier;
uniform float _NoiseMultiplier;
uniform half MaxDensity;

uniform float _MinimumWidth;
uniform float _MaximumWidth;

uniform float _MinimumHeight;
uniform float _MaximumHeight;

uniform float _UseColorMap;

uniform half4 _dryColor;
uniform half4 _healthyColor;

uniform float _RotateNormals;

uniform float fadeDistance = 100;

// lods

uniform float lods_Enabled;

uniform float lod0_Distance;
uniform float lod0_Value;

uniform float lod1_Distance;
uniform float lod1_Value;

uniform float lod2_Distance;
uniform float lod2_Value;

uniform float lod3_Distance;
uniform float lod3_Value;

// interactions
float4 _InteractionTouchBendedInstances[20];

uniform float touchBendingEnabled;
uniform float touchBendingStrength;

// Property Block
#if INSTANCING_ON
UNITY_INSTANCING_BUFFER_START(Props)
UNITY_DEFINE_INSTANCED_PROP(float4, _WorldPosition)	// Make _Color an instanced property (i.e. an array)
#define _WorldPosition_arr Props
UNITY_INSTANCING_BUFFER_END(Props)
#else
float4 _WorldPosition;
#endif

float4 _StreamingAdjuster;

struct uNature_Foliage_appdata
{
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;

	float4 texcoord : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
	float4 texcoord2 : TEXCOORD2;
	float4 texcoord3 : TEXCOORD3;
	float4 color : COLOR;

	#if INSTANCING_ON
	UNITY_VERTEX_INPUT_INSTANCE_ID
	#endif
};

float InterpolatedSingle(float cord, float difference, float currentHeight)
{
	float remainer = cord - floor(cord);

	return currentHeight + (difference * remainer);
}

float InterpolateValue(float xVertex, float zVertex, float currentHeight, float nextHeight)
{
	float difference = nextHeight - currentHeight;

	return InterpolatedSingle(zVertex, difference, currentHeight);
}

half3 GetDryHealthy(float spread)
{
	float3 differences = _dryColor.rgb - _healthyColor.rgb;

	return _healthyColor.xyz + differences.xyz * spread;
	//return float4(_dryColor.x + diferences.x * spread, _dryColor.y + diferences.y * spread, _dryColor.z + diferences.z * spread, 1);
}

float GetSize(float noise, float minValue, float maxValue)
{
	float difference = maxValue - minValue;
	difference *= noise;

	return minValue + difference;
}

float InverseNegativeConvertedToPositive(float value)
{
	value -= 0.5;
	value *= 2;

	return value;
}

float CalculateNormalBChannel(float x, float y)
{
	return sqrt(1 - (x*x + y*y));
}

float CalculateHeights(float3 vertexNoisedPosition)
{
	const float height_snapFixDistance = 2.5;

	float4 results;
	float _FoliageWorldDifferences = (float)_FoliageAreaSize / _FoliageAreaResolution;

	//calculate interpolated heights
	float3 flooredVertexWorldPosition = floor(vertexNoisedPosition) / _FoliageWorldDifferences;
	float3 ceiledVertexWorldPosition = ceil(vertexNoisedPosition) / _FoliageWorldDifferences;

	float2 flooredHeights = GetInformationFromMap(flooredVertexWorldPosition, _FoliageAreaResolution, _HeightMap_WORLD).xy;
	float2 ceiledHeights = GetInformationFromMap(ceiledVertexWorldPosition, _FoliageAreaResolution, _HeightMap_WORLD).xy;

	float2 flooredHeightNormalized = flooredHeights.xy * 255;
	float2 ceiledHeightNormalized = ceiledHeights.xy * 255;

	float flooredHeightTransformed = (((flooredHeightNormalized.x * 256.0) + flooredHeightNormalized.y) / 65535.0) * 2048;
	float ceiledHeightTransformed = (((ceiledHeightNormalized.x * 256.0) + ceiledHeightNormalized.y) / 65535.0) * 2048;

	float interpolatedValueHeights;

	if (abs(ceiledHeightTransformed - flooredHeightTransformed) > height_snapFixDistance)
	{
		interpolatedValueHeights = flooredHeightTransformed;
	}
	else
	{
		interpolatedValueHeights = InterpolateValue(vertexNoisedPosition.x, vertexNoisedPosition.z, flooredHeightTransformed, ceiledHeightTransformed);
	}

	//calculate results (heights)
	return interpolatedValueHeights;
}

float GetLOD(float distanceFromCamera)
{
	if (lods_Enabled == 0 || distanceFromCamera < lod0_Distance) return 1;

	if (distanceFromCamera <= lod0_Distance) return lod0_Value;
	else if (distanceFromCamera <= lod1_Distance) return lod1_Value;
	else if (distanceFromCamera <= lod2_Distance) return lod2_Value;
	else return lod3_Value;
}
float4 CalculateTouchBending(float4 vertex)
{
	if (touchBendingEnabled != 1) return vertex;

	float4 current;
	for (int i = 0; i < 20; i++)
	{
		current = _InteractionTouchBendedInstances[i];

		if (current.w > 0)
		{
			if (distance(vertex.xyz, current.xyz) < current.w)
			{
				float WMDistance = 1 - clamp(distance(vertex.xyz, current.xyz) / current.w, 0, 1);
				float3 posDifferences = normalize(vertex.xyz - current.xyz);

				float3 strengthedDifferences = posDifferences * (touchBendingStrength + touchBendingStrength);

				float3 resultXZ = WMDistance * strengthedDifferences;

				vertex.xz += resultXZ.xz;
				vertex.y -= WMDistance * touchBendingStrength;

				return vertex;
			}
		}
	}

	return vertex;
}
float GetDensity(float4 pixel)
{
	pixel *= 255;
	if (pixel.r == _PrototypeID) return pixel.g;
	if (pixel.b == _PrototypeID) return pixel.a;

	return 0;
}

void CalculateGPUVertex(inout uNature_Foliage_appdata v)
{
	float _FoliageAreaMultiplier = (float)_FoliageAreaResolution / _FoliageAreaSize;

	float4 WorldPosition;

	#if INSTANCING_ON
	WorldPosition = UNITY_ACCESS_INSTANCED_PROP(_WorldPosition_arr, _WorldPosition);
	#else
	WorldPosition = _WorldPosition;
	#endif

	float4 StreamedWorldPosition = WorldPosition;

	float2 vertexWorldPosition2D = v.vertex.xz + WorldPosition.xz;

	if (vertexWorldPosition2D.x < 0 || vertexWorldPosition2D.y < 0 || vertexWorldPosition2D.x > _FoliageAreaSize || vertexWorldPosition2D.y > _FoliageAreaSize) return;

	float distanceFromCamera = distance(vertexWorldPosition2D, _WorldSpaceCameraPos.xz - _FoliageAreaPosition.xz);

	if (distanceFromCamera > fadeDistance) return;

	float4 vertexInformation = GetInformationFromMap((v.vertex.xyz + StreamedWorldPosition.xyz) * _FoliageAreaMultiplier, _FoliageAreaResolution, _GrassMap);
	float density = GetDensity(vertexInformation) * _DensityMultiplier;
	density = ceil(density * GetLOD(distanceFromCamera));

	if (density > MaxDensity)
		density = MaxDensity;

	if (density >= v.texcoord3.y)
	{
		float4 hMap = GetInformationFromMap((v.vertex.xyz + StreamedWorldPosition.xyz) * _FoliageAreaMultiplier, _FoliageAreaResolution, _HeightMap_WORLD);

		float3 noise = float3(hMap.b * v.texcoord2.x, 0, hMap.b * v.texcoord2.y) * _NoiseMultiplier;

		float persistentNoise = hMap.b;

		float xNoisePosition = v.vertex.x + noise.x;
		float zNoisePosition = v.vertex.z + noise.z;

		float xNoiseWorldPosition = xNoisePosition + StreamedWorldPosition.x;
		float zNoiseWorldPosition = zNoisePosition + StreamedWorldPosition.z;

		float3 position = float3(xNoiseWorldPosition, 0, zNoiseWorldPosition);

		if (v.texcoord1.y > 0.15)
		{
			v.texcoord1.y *= GetSize(hMap.b * v.texcoord2.x, _MinimumHeight, _MaximumHeight);
		}

		float widthValue = GetSize(hMap.b * v.texcoord2.y, _MinimumWidth, _MaximumWidth);
		v.texcoord1.x *= widthValue;
		v.texcoord3.x *= widthValue;

		float3 pos = float3(v.texcoord1.x, v.texcoord1.y, v.texcoord3.x);

		v.vertex.y += CalculateHeights(position);
		v.normal = float3(0, 1, 0);

		float yEuler = hMap.b * v.texcoord2.x * 360;

		//Create quaternion from formula
		float4 q = float4(0, cos(0) * sin(yEuler) * cos(0) + sin(0) * cos(yEuler) * sin(0), 0, cos(0) * cos(yEuler) * cos(0) - sin(0) * sin(yEuler) * sin(0));

		float3 t = 2 * cross(q.xyz, pos);
		pos += q.w * t + cross(q.xyz, t);

		if (_RotateNormals == 1)
		{
			t = 2 * cross(q.xyz, v.normal);
			v.normal += q.w * t + cross(q.xyz, t);
		}

		t = 2 * cross(q.xyz, v.tangent.xyz);
		v.tangent.xyz += q.w * t + cross(q.xyz, t);
		v.tangent.xyz += pos + noise + WorldPosition.xyz + _FoliageAreaPosition.xyz - _StreamingAdjuster.xyz;

		v.vertex.xyz += pos + noise + WorldPosition.xyz + _FoliageAreaPosition.xyz - _StreamingAdjuster.xyz;

		v.vertex = CalculateTouchBending(v.vertex);

		//v.vertex = Read_Apply_Interaction(v.vertex);

		v.vertex = ApplyFastWind(v.vertex, v.texcoord.y);

		#if SHADER_API_MOBILE || SHADER_API_GLES || SHADER_TARGET < 25
		v.color.xyz = GetDryHealthy(persistentNoise);
		#else
		if (_UseColorMap) // if use color map == 1 (like a boolean == true)
		{
			v.color = GetInformationFromMap(position, _FoliageAreaSize, _ColorMap);
		}
		else
		{
			v.color.xyz = GetDryHealthy(persistentNoise);
		}
		#endif

		distanceFromCamera = distance(v.vertex.xz, _WorldSpaceCameraPos.xz); // update distance to camera.
		float fadeDistanceMin = fadeDistance * 0.8; // 20% of full amount.
		half alphaFalloff = saturate((distanceFromCamera - fadeDistanceMin) / (fadeDistance - fadeDistanceMin));
		v.color.a *= (1.0f - alphaFalloff);
	}
}
#endif