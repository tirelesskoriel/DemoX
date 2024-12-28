Shader "Unlit/mix" {
	Properties {
		_MainTex ("Background", Rect) = "white" {}
		_MainTex2 ("Background", Rect) = "white" {}
		
		_Multi1 ("Main*Multi+Main2*(1-Multi)", Range(0, 1)) = 0

		_Alpha ("Alpha", Range(0, 1)) = 1
        _ForceZ ("ForceZ(0 BACK, 1 FRONT)", Range(0, 1)) = 0

		[Toggle(UPDOWN1)]  _Layout ("UpDown1", Float) = 0
		[Toggle(UPDOWN2)]  _Layout2 ("UpDown2", Float) = 0

		[Toggle(HAS_3D)] _Has3D("OPEN 3D", Float) = 1
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
		//Offset -1, -1
		Fog { Mode Off }
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#define pi 3.14159265358979
			
			#pragma multi_compile HAS_ALPHA_OFF HAS_ALPHA
			#pragma multi_compile FULL_SPHERE_OFF FULL_SPHERE
			#pragma multi_compile HAS_3D_OFF HAS_3D
			#pragma multi_compile UPDOWN1_OFF UPDOWN1
			#pragma multi_compile UPDOWN2_OFF UPDOWN2

			#include "UnityCG.cginc"
			uniform sampler2D _MainTex;
			float4 _MainTex_ST;

			uniform sampler2D _MainTex2;
			float4 _MainTex2_ST;
			
			uniform float _Alpha, _ForceZ;
			uniform float _Multi1;

			int GetStereoEyeIndex()
			{
				#if HAS_3D
					return unity_StereoEyeIndex;
				#else
					return 0;
				#endif
			}

			struct VertexOutput
			{
				float2 uv:TEXCOORD0;
				float4 pos:SV_POSITION;
			};

			VertexOutput vert(appdata_base v)
			{
				VertexOutput o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.pos.z = _ForceZ;
				o.uv = v.texcoord.xy;

				return o;
			}
			
			float4 frag(VertexOutput i):COLOR {
				// === MainTex ===
				float2 uv = i.uv;
				#if UPDOWN1
					uv.y = uv.y / 2.0 + (1.0 - GetStereoEyeIndex()) * 0.5;
				#endif
				float4 t1 = tex2D(_MainTex, TRANSFORM_TEX(uv, _MainTex));

				// === MainTex2 ===
				float2 uv2 = i.uv;
				#if UPDOWN2
					uv2.y = uv2.y / 2.0 + (1.0 - GetStereoEyeIndex()) * 0.5;
				#endif

				float4 t2 = tex2D(_MainTex2, TRANSFORM_TEX(uv2, _MainTex2));
				
				float4 result = t1 * _Multi1 + t2 * (1-_Multi1);
				result.a = _Alpha;
				return result;
			}


			ENDCG
		}

	} 
}

 