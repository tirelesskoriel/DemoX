Shader "Unlit/Raw3D" {
	Properties {
		_MainTex ("Background", Rect) = "black" {}

		_FgTex ("Camera Movie(Rect)", Rect) = "black" {}
		
		[Toggle(HAS_ALPHA)] _HasAlpha("有透明度(HAS_ALPHA)", Float) = 0
		
		_Edge ("边缘羽化宽度", Range(0, 0.2)) = 0
		
		[Enum(None, 0, Side by Side, 1, Over Under, 2)]  _Layout ("3D Layout", Float) = 0.000000

		[Enum(None, 0, Has 3D, 1)] _Has3D("有3D效果", Float) = 0
		 _ForceZ ("ForceZ(0 BACK, 1 FRONT)", Range(0, 1)) = 1

		 [Toggle(TO_GAMMA)] _ToGamma("LinearToGamma", Float) = 0
		 _ExtraAlpha("ExtraAlpha", Range(0.5, 1.5)) = 1
	}
	SubShader 
	{
		LOD 200
		Tags { 
			"RenderType" = "Opaque"
			"IgnoreProjector"="True" 
			"ForceNoShadowCasting"="True" 
			"Queue" = "Transparent"
			"DisableBatching" = "True"
		}

		Cull Back
		Lighting Off
		Blend SrcAlpha OneMinusSrcAlpha  
		ZWrite Off
		//ZTest Less
		//Offset -1, -1
		Fog { Mode Off }
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#define pi 3.14159265358979
			
			#pragma multi_compile HAS_ALPHA_OFF HAS_ALPHA
			#pragma multi_compile TO_GAMMA_OFF TO_GAMMA

			#include "UnityCG.cginc"
			uniform sampler2D _MainTex;
			float4 _MainTex_ST;

			uniform sampler2D _FgTex;
			
			uniform float _Layout;

			float4 _FgTex_ST;
			uniform float _ForceZ;
			float _Edge;

			uniform float _Has3D;
			uniform float _ExtraAlpha;

			int GetStereoEyeIndex()
			{
				if (_Has3D)
					return unity_StereoEyeIndex;
				else
					return 0;
			}

			struct VertexOutput
			{
				float4 pos:SV_POSITION;
				float2 uv:TEXCOORD0;
			};

			VertexOutput vert(appdata_base v)
			{
				VertexOutput o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.pos.z = _ForceZ;
				//o.uv = TRANSFORM_TEX(v.texcoord, _FgTex);
				o.uv = v.texcoord.xy;

				return o;
			}
			
			float4 GetTex(sampler2D sample, float2 xy) {
			    return tex2D(sample, TRANSFORM_TEX(xy, _FgTex));
			}

			float4 frag(VertexOutput i):COLOR {
				float2 uv = i.uv;

#if defined(HAS_ALPHA)
                uv.y = uv.y / 2;                
                float4 alpha = GetTex(_FgTex, uv);

				#if defined(TO_GAMMA)
				alpha = pow(alpha, 2.2);
				#endif
        
                float gray = alpha.r * 0.299 + alpha.g * 0.587 + alpha.b * 0.114;
				
				uv.y += 0.5;
								
				float4 result = GetTex(_FgTex, uv);
				#if defined(TO_GAMMA)
				result = pow(result, 2.2);
				#endif

				result.a = gray * _ExtraAlpha;
				return result;
#else				


				if (_Layout == 1.0) {
					uv.x = uv.x / 2.0;
					if (GetStereoEyeIndex() == 1.0)
					{	
						uv.x += 0.5;
					}
				} else if (_Layout == 2.0) {
					uv.y = uv.y / 2;
					if (GetStereoEyeIndex() == 0.0)
					{	
						uv.y += 0.5;
					}
				}


				float w = saturate((0.5 - abs(uv.x - 0.5)) / _Edge) 
					* saturate((0.5 - abs(uv.y - 0.5)) / _Edge);

				float4 result = float4(GetTex(_FgTex, uv).xyz, w);
				#if defined(TO_GAMMA)
				result = pow(result, 2.2);
				#endif

				return result;
#endif				
			}


			ENDCG
		}

	} 
}

 