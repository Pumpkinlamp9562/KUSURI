// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'

Shader "uNature/FoliageShader_Advanced"
{
	Properties
	{
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" { }
		_BumpMap("Normal Map", 2D) = "bump" {}
		_Occlusion("Occlusion", 2D) = "white" {}

		_Color("Main Color", Color) = (1,1,1,1)

		///UNATURE PROPERTIES BEGIN
		_Cutoff("Alpha cutoff", Range(0,1)) = 0.3
		_WindSpeed("Wind Speed", Range(0, 5.0)) = 0.2
		_WindBending("Wind Bending", Range(0, 3.0)) = 0.1
		_DensityMultiplier("Density Multiplier", Range(0, 1)) = 1
		_NoiseMultiplier("Noise Multiplier", Range(0, 2)) = 1
		MaxDensity("Max Density", Float) = 10

		[HideInInspector]
		_UseColorMap("Use Color Map", Range(0, 1)) = 0
		[HideInInspector]
		_ColorMap("Color Map", 2D) = "white" {}

		[HideInInspector]
		_GrassMap("Grass Map", 2D) = "white" {}
		[HideInInspector]
		_WorldMap("Foliage World Map", 2D) = "white" {}
		[HideInInspector]
		_InteractionMap("Foliage Interaction Map", 2D) = "black" {}
		[HideInInspector]
		_InteractionMapRadius("Foliage Interaciton Radius", Float) = 102

		_MinimumWidth("Min Width", Float) = 1.5
		_MaximumWidth("Max Width", Float) = 2

		_MinimumHeight("Min Height", Float) = 1
		_MaximumHeight("Max Height", Float) = 1.2

		[HideInInspector]
		_RotateNormals("Rotate Normals", Float) = 0

		lods_Enabled("LOD Enabled", Range(0, 1)) = 1

		lod0_Distance("LOD 0 Distance", Float) = 50
		lod0_Value("LOD 0 Distance", Range(0,1)) = 0.8

		lod1_Distance("LOD 1 Distance", Float) = 100
		lod1_Value("LOD 1 Distance", Range(0,1)) = 0.6

		lod2_Distance("LOD 2 Distance", Float) = 120
		lod2_Value("LOD 2 Distance", Range(0,1)) = 0.4

		lod3_Distance("LOD 3 Distance", Float) = 200
		lod3_Value("LOD 3 Distance", Range(0,1)) = 0.2

		[HideInInspector]
	_FoliageAreaSize("Grass Area Size", Float) = 1024
		[HideInInspector]
	_FoliageAreaResolution("Grass Area Resolution", Float) = 2048
		[HideInInspector]
	_FoliageAreaPosition("Grass Area Position", Vector) = (1,1,1,1)
		[HideInInspector]
	_FoliageWorldMapResolution("WorldMap Resolution", Float) = 1024

		[HideInInspector]
	_FoliageInteractionPosition("Interaction Position", Vector) = (0,0,0,0)

		[HideInInspector]
	_InteractionMapRadius("Interaction Map Radius", Float) = 124

		_healthyColor("Healthy Color", Color) = (1,1,1,1)
		_dryColor("Dry Color", Color) = (1,1,1,1)

		fadeDistance("Fade Distance", Range(0, 10000)) = 100

		touchBendingEnabled("Touch Bending Enabled", Range(0, 1)) = 1
		touchBendingStrength("Touch Bending Strength", Range(0, 10)) = 0.97
		///UNATURE PROPERTIES END
	}

		SubShader{
		Tags{ "RenderType" = "Geometry" "IgnoreProjector" = "True" }

		Cull Off

		CGPROGRAM

		#include "uNature_Foliage_Base.cginc"
		
		#pragma surface surf Lambert vertex:vert addshadow
		#pragma multi_compile_INSTANCING_ON

		#if INSTANCING_ON
		#pragma multi_compile_instancing
		#endif
		
		#if SHADER_API_GLES || SHADER_API_D3D9
		#pragma target 2.0
		#elif instancing
		#pragma target 4.0
		#else
		#pragma target 3.0
		#endif
		
		#include "UnityCG.cginc"

		struct Input
		{
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float4 color : COLOR;

			#if INSTANCING_ON
			UNITY_VERTEX_INPUT_INSTANCE_ID
			#endif
		};

	uniform sampler2D _MainTex;
	uniform sampler2D _BumpMap;
	uniform sampler2D _Occlusion;
	uniform fixed4 _Color;

	void vert(inout uNature_Foliage_appdata v)
	{
		CalculateGPUVertex(v);
	}

	void surf(Input IN, inout SurfaceOutput o)
	{
		fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

		fixed4 occ = tex2D(_Occlusion, IN.uv_MainTex);

		if (_UseColorMap)
		{
			o.Albedo = (c.rgb / 2 + IN.color.rgb / 2) * occ.rgb;
		}
		else
		{
			o.Albedo = c.rgb * occ.rgb * IN.color.rgb;
		}

		o.Alpha = c.a * IN.color.a;

		o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));

		clip(o.Alpha - _Cutoff);
	}

	ENDCG
	}

	Fallback "uNature/FoliageShader_Mobile"
}
