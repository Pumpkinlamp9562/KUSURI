Shader "Hidden/VolumetricLights/Blur"
{
Properties
{
        [HideInInspector] _BlendSrc("Blend Src", Int) = 1
        [HideInInspector] _BlendDest("Blend Dest", Int) = 1
}
SubShader
{
    ZWrite Off ZTest Always Blend Off Cull Off

    HLSLINCLUDE
    #pragma target 3.0
    #pragma prefer_hlslcc gles
    #pragma exclude_renderers d3d11_9x
    
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
    ENDHLSL

  Pass { // 0 Blur horizontally
      HLSLPROGRAM
      #pragma vertex VertBlur
      #pragma fragment FragBlur
      #define BLUR_HORIZ
      #include "Blur.hlsl"
      ENDHLSL
  }    
      
  Pass { // 1 Blur vertically
	  HLSLPROGRAM
      #pragma vertex VertBlur
      #pragma fragment FragBlur
      #define BLUR_VERT
      #include "Blur.hlsl"
      ENDHLSL
  }
      
  Pass { // 2 Blend
      Blend [_BlendSrc] [_BlendDest]
	  HLSLPROGRAM
      #pragma vertex VertBlur
      #pragma fragment FragBlur
      #define BLUR_VERT
      //#define DITHER // commented for future uses
      #define FINAL_BLEND
      #include "Blur.hlsl"
      ENDHLSL
  }    

}
}
