// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

#ifndef UNATURE_Foliage_INTERACTION
#define UNATURE_Foliage_INTERACTION

// VARIABLES

//sampler2D interactionMap;
float3 interactionCenter;
float interactionMapSize;
float interactionMapResolution;

float4 GetInformationFromMap(float3 worldPos, float resolution, sampler2D map)
{
	worldPos.x = clamp(worldPos.x, 0, resolution - 1);
	worldPos.z = clamp(worldPos.z, 0, resolution - 1);

	#if SHADER_API_GLES || SHADER_TARGET < 30
	return tex2Dgrad(map, float2(worldPos.x / resolution, worldPos.z / resolution), 0, 0);
	#else
	return tex2Dlod(map, float4(worldPos.x / resolution, worldPos.z / resolution, 0, 0));
	#endif
}

float GetInteractionRatio()
{
	return interactionMapSize / interactionMapResolution;
}

float3 TransformInteractionCoord(float3 position)
{
	return (abs(position - interactionCenter)) / GetInteractionRatio();
}

float4 Read_Apply_Interaction(float4 vertexPosition)
{
	/*
	float3 transformedVertexPosition = TransformInteractionCoord(vertexPosition);

	if (transformedVertexPosition.x < 0 || transformedVertexPosition.z < 0 || transformedVertexPosition.x > interactionMapResolution || transformedVertexPosition.z > interactionMapResolution) return vertexPosition;

	float4 interactionMapInformation = GetInformationFromMap(transformedVertexPosition, interactionMapResolution, interactionMap);

	vertexPosition.xz -= interactionMapInformation.rg * 255;

	return vertexPosition;
	*/

	return vertexPosition;
}

#endif