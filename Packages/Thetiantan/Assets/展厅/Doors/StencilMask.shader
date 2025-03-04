﻿Shader "Portal/StencilMask"
{
	Properties
	{
		_Ref("World", Int)=1
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100
		Zwrite Off
		ColorMask 0
		Cull off

		
		Pass
		{
			Stencil{
				Ref [_Ref]
				Comp always
				Pass replace
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return fixed4(0.0, 0.0, 0.0, 0.0);
			}
			ENDCG
		}
	}
}