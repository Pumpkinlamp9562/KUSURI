
TEXTURE2D_X(_MainTex);
float4    _MainTex_TexelSize;
float4    _MainTex_ST;
float     _BlurScale;
float     _Brightness;

// Optimization for SSPR
#define uvN uv1
#define uvE uv2
#define uvW uv3
#define uvS uv4


#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED) || defined(SINGLE_PASS_STEREO)
    #define VERTEX_CROSS_UV_DATA
    #define VERTEX_OUTPUT_GAUSSIAN_UV(o)

    #if defined(BLUR_HORIZ)
        #define FRAG_SETUP_GAUSSIAN_UV(i) float2 inc = float2(_MainTex_TexelSize.x * 1.3846153846 * _BlurScale, 0); float2 uv1 = i.uv - inc; float2 uv2 = i.uv + inc; float2 inc2 = float2(_MainTex_TexelSize.x * 3.2307692308 * _BlurScale, 0); float2 uv3 = i.uv - inc2; float2 uv4 = i.uv + inc2;
    #else
        #define FRAG_SETUP_GAUSSIAN_UV(i) float2 inc = float2(0, _MainTex_TexelSize.y * 1.3846153846 * _BlurScale); float2 uv1 = i.uv - inc; float2 uv2 = i.uv + inc; float2 inc2 = float2(0, _MainTex_TexelSize.y * 3.2307692308 * _BlurScale); float2 uv3 = i.uv - inc2; float2 uv4 = i.uv + inc2;
    #endif

#else
    #define VERTEX_CROSS_UV_DATA float2 uvN : TEXCOORD1; float2 uvW: TEXCOORD2; float2 uvE: TEXCOORD3; float2 uvS: TEXCOORD4;

    #if defined(BLUR_HORIZ)
        #define VERTEX_OUTPUT_GAUSSIAN_UV(o) float2 inc = float2(_MainTex_TexelSize.x * 1.3846153846 * _BlurScale, 0); o.uv1 = o.uv - inc; o.uv2 = o.uv + inc; float2 inc2 = float2(_MainTex_TexelSize.x * 3.2307692308 * _BlurScale, 0); o.uv3 = o.uv - inc2; o.uv4 = o.uv + inc2;
    #else
        #define VERTEX_OUTPUT_GAUSSIAN_UV(o) float2 inc = float2(0, _MainTex_TexelSize.y * 1.3846153846 * _BlurScale); o.uv1 = o.uv - inc; o.uv2 = o.uv + inc; float2 inc2 = float2(0, _MainTex_TexelSize.y * 3.2307692308 * _BlurScale); o.uv3 = o.uv - inc2; o.uv4 = o.uv + inc2;
    #endif
    #define FRAG_SETUP_GAUSSIAN_UV(i) float2 uv1 = i.uv1; float2 uv2 = i.uv2; float2 uv3 = i.uv3; float2 uv4 = i.uv4;

#endif

	struct VaryingsCross {
	    float4 positionCS : SV_POSITION;
	    float2 uv: TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
        VERTEX_CROSS_UV_DATA
        UNITY_VERTEX_OUTPUT_STEREO
	};


	VaryingsCross VertBlur(Attributes v) {
    	VaryingsCross o;
        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_TRANSFER_INSTANCE_ID(v, o);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	    o.positionCS = v.positionOS;
		o.positionCS.y *= _ProjectionParams.x;
    	o.uv = v.uv;
        VERTEX_OUTPUT_GAUSSIAN_UV(o)

    	return o;
	}
	
	float4 FragBlur (VaryingsCross i): SV_Target {
    	UNITY_SETUP_INSTANCE_ID(i);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
        i.uv = UnityStereoTransformScreenSpaceTex(i.uv);
        FRAG_SETUP_GAUSSIAN_UV(i)

        float4 pixel0 = SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, i.uv) * 0.2270270270;
        float4 pixel1 = SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv1) * 0.3162162162;
        float4 pixel2 = SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv2) * 0.3162162162;
		float4 pixel3 = SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv3) * 0.0702702703;
        float4 pixel4 = SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv4) * 0.0702702703;

        float4 pixel = pixel0 + pixel1 + pixel2 + pixel3 + pixel4;

        #if defined(DITHER)
            const float3 magic = float3( 0.06711056, 0.00583715, 52.9829189 );
            float jitter = frac( magic.z * frac( dot( i.uv.xy * _MainTex_TexelSize.zw, magic.xy ) ) );
	        pixel = max(0, pixel - jitter * 0.01);
        #endif

        #if defined(FINAL_BLEND)
            pixel.rgb *= _Brightness;
        #endif
   		return pixel;
	}	

	
	