// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Amplify Leaf Blotchy"
{
	Properties
	{
		_Tint("Tint", Color) = (0,0.9201922,1,0)
		_BlotchTint("Blotch Tint", Color) = (1,0,0,0)
		_BlotchSize("Blotch Size", Float) = 0.01
		_Contrast("Contrast", Float) = 1
		_Albedo("Albedo", 2D) = "white" {}
		_Roughness("Roughness", Range( 0 , 1)) = 0.23
		_Metallic("Metallic", Range( 0 , 1)) = 0
		_Cutoff( "Mask Clip Value", Float ) = 0.45
		_Normals("Normals", 2D) = "bump" {}
		_Occlusion("Occlusion", 2D) = "white" {}
		_WindFrequency("Wind Frequency", Float) = 1.34
		_NoiseScale("Noise Scale", Float) = 1.41
		_MovementMultiplier("Movement Multiplier", Float) = 0.07
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "AlphaTest+0" }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float _WindFrequency;
		uniform float _NoiseScale;
		uniform float _MovementMultiplier;
		uniform sampler2D _Normals;
		uniform float4 _Normals_ST;
		uniform float _Contrast;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform float _BlotchSize;
		uniform float4 _Tint;
		uniform float4 _BlotchTint;
		uniform float _Metallic;
		uniform float _Roughness;
		uniform sampler2D _Occlusion;
		uniform float4 _Occlusion_ST;
		uniform float _Cutoff = 0.45;


		float3 mod3D289( float3 x ) { return x - floor( x / 289.0 ) * 289.0; }

		float4 mod3D289( float4 x ) { return x - floor( x / 289.0 ) * 289.0; }

		float4 permute( float4 x ) { return mod3D289( ( x * 34.0 + 1.0 ) * x ); }

		float4 taylorInvSqrt( float4 r ) { return 1.79284291400159 - r * 0.85373472095314; }

		float snoise( float3 v )
		{
			const float2 C = float2( 1.0 / 6.0, 1.0 / 3.0 );
			float3 i = floor( v + dot( v, C.yyy ) );
			float3 x0 = v - i + dot( i, C.xxx );
			float3 g = step( x0.yzx, x0.xyz );
			float3 l = 1.0 - g;
			float3 i1 = min( g.xyz, l.zxy );
			float3 i2 = max( g.xyz, l.zxy );
			float3 x1 = x0 - i1 + C.xxx;
			float3 x2 = x0 - i2 + C.yyy;
			float3 x3 = x0 - 0.5;
			i = mod3D289( i);
			float4 p = permute( permute( permute( i.z + float4( 0.0, i1.z, i2.z, 1.0 ) ) + i.y + float4( 0.0, i1.y, i2.y, 1.0 ) ) + i.x + float4( 0.0, i1.x, i2.x, 1.0 ) );
			float4 j = p - 49.0 * floor( p / 49.0 );  // mod(p,7*7)
			float4 x_ = floor( j / 7.0 );
			float4 y_ = floor( j - 7.0 * x_ );  // mod(j,N)
			float4 x = ( x_ * 2.0 + 0.5 ) / 7.0 - 1.0;
			float4 y = ( y_ * 2.0 + 0.5 ) / 7.0 - 1.0;
			float4 h = 1.0 - abs( x ) - abs( y );
			float4 b0 = float4( x.xy, y.xy );
			float4 b1 = float4( x.zw, y.zw );
			float4 s0 = floor( b0 ) * 2.0 + 1.0;
			float4 s1 = floor( b1 ) * 2.0 + 1.0;
			float4 sh = -step( h, 0.0 );
			float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
			float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;
			float3 g0 = float3( a0.xy, h.x );
			float3 g1 = float3( a0.zw, h.y );
			float3 g2 = float3( a1.xy, h.z );
			float3 g3 = float3( a1.zw, h.w );
			float4 norm = taylorInvSqrt( float4( dot( g0, g0 ), dot( g1, g1 ), dot( g2, g2 ), dot( g3, g3 ) ) );
			g0 *= norm.x;
			g1 *= norm.y;
			g2 *= norm.z;
			g3 *= norm.w;
			float4 m = max( 0.6 - float4( dot( x0, x0 ), dot( x1, x1 ), dot( x2, x2 ), dot( x3, x3 ) ), 0.0 );
			m = m* m;
			m = m* m;
			float4 px = float4( dot( x0, g0 ), dot( x1, g1 ), dot( x2, g2 ), dot( x3, g3 ) );
			return 42.0 * dot( m, px);
		}


		float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }

		float snoise( float2 v )
		{
			const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
			float2 i = floor( v + dot( v, C.yy ) );
			float2 x0 = v - i + dot( i, C.xx );
			float2 i1;
			i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
			float4 x12 = x0.xyxy + C.xxzz;
			x12.xy -= i1;
			i = mod2D289( i );
			float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
			float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
			m = m * m;
			m = m * m;
			float3 x = 2.0 * frac( p * C.www ) - 1.0;
			float3 h = abs( x ) - 0.5;
			float3 ox = floor( x + 0.5 );
			float3 a0 = x - ox;
			m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.yz = a0.yz * x12.xz + h.yz * x12.yw;
			return 130.0 * dot( m, g );
		}


		float4 CalculateContrast( float contrastValue, float4 colorTarget )
		{
			float t = 0.5 * ( 1.0 - contrastValue );
			return mul( float4x4( contrastValue,0,0,t, 0,contrastValue,0,t, 0,0,contrastValue,t, 0,0,0,1 ), colorTarget );
		}

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float mulTime10 = _Time.y * _WindFrequency;
			float2 temp_cast_0 = (mulTime10).xx;
			float2 uv_TexCoord7 = v.texcoord.xy + temp_cast_0;
			float simplePerlin3D6 = snoise( float3( uv_TexCoord7 ,  0.0 )*_NoiseScale );
			simplePerlin3D6 = simplePerlin3D6*0.5 + 0.5;
			float temp_output_30_0 = ( ( simplePerlin3D6 + -0.5 ) * _MovementMultiplier );
			float simplePerlin2D33 = snoise( uv_TexCoord7*_NoiseScale );
			simplePerlin2D33 = simplePerlin2D33*0.5 + 0.5;
			float temp_output_36_0 = ( _MovementMultiplier * ( simplePerlin2D33 + -0.5 ) );
			float3 appendResult27 = (float3(temp_output_30_0 , temp_output_36_0 , ( ( temp_output_30_0 + temp_output_36_0 ) * 0.4 )));
			v.vertex.xyz += ( v.texcoord1.xy.y * appendResult27 );
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normals = i.uv_texcoord * _Normals_ST.xy + _Normals_ST.zw;
			o.Normal = UnpackNormal( tex2D( _Normals, uv_Normals ) );
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			float4 tex2DNode1 = tex2D( _Albedo, uv_Albedo );
			float2 appendResult68 = (float2(unity_ObjectToWorld[0].x , unity_ObjectToWorld[0].y));
			float simplePerlin2D65 = snoise( appendResult68*_BlotchSize );
			simplePerlin2D65 = simplePerlin2D65*0.5 + 0.5;
			float layeredBlendVar70 = simplePerlin2D65;
			float4 layeredBlend70 = ( lerp( _Tint,_BlotchTint , layeredBlendVar70 ) );
			o.Albedo = CalculateContrast(_Contrast,( tex2DNode1 * layeredBlend70 )).rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Roughness;
			float2 uv_Occlusion = i.uv_texcoord * _Occlusion_ST.xy + _Occlusion_ST.zw;
			o.Occlusion = tex2D( _Occlusion, uv_Occlusion ).r;
			o.Alpha = 1;
			clip( tex2DNode1.a - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17900
137;164;1687;714;870.4529;536.5354;1.527961;True;True
Node;AmplifyShaderEditor.RangedFloatNode;8;-1100.445,600.1421;Inherit;False;Property;_WindFrequency;Wind Frequency;10;0;Create;True;0;0;False;0;1.34;0.54;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;10;-896.8459,607.843;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;7;-701.1458,561.4418;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;28;-676.4127,692.6339;Inherit;False;Property;_NoiseScale;Noise Scale;11;0;Create;True;0;0;False;0;1.41;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;33;-415.2275,704.614;Inherit;False;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;6;-439.7458,481.5416;Inherit;False;Simplex3D;True;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0.01;False;1;FLOAT;0
Node;AmplifyShaderEditor.ObjectToWorldMatrixNode;64;-1079.407,-134.6676;Inherit;False;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.RangedFloatNode;29;-193.9351,692.2194;Inherit;False;Property;_MovementMultiplier;Movement Multiplier;12;0;Create;True;0;0;False;0;0.07;0.04;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;34;-218.4274,795.8138;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;22;-225.4586,567.2444;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.VectorFromMatrixNode;67;-853.5365,-127.18;Inherit;False;Row;0;1;0;FLOAT4x4;1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;68;-662.6083,-118.4448;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;71;-640.0386,5.59639;Inherit;False;Property;_BlotchSize;Blotch Size;2;0;Create;True;0;0;False;0;0.01;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;75.97253,699.814;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;26.46487,560.9194;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;38;267.1733,730.214;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;59;-445.701,145.2438;Inherit;False;Property;_Tint;Tint;0;0;Create;True;0;0;False;0;0,0.9201922,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NoiseGeneratorNode;65;-407.4501,-104.4785;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,1;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;61;-435.0613,313.6519;Inherit;False;Property;_BlotchTint;Blotch Tint;1;0;Create;True;0;0;False;0;1,0,0,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LayeredBlendNode;70;-77.08778,15.4831;Inherit;True;6;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;1;-167.0459,-204.4612;Inherit;True;Property;_Albedo;Albedo;4;0;Create;True;0;0;False;0;-1;5d93f9a7872434d419a703ec94e2b61b;47e31f197cf93ac468e725e114f473a2;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;39;411.1733,727.0139;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.4;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;27;516.8499,563.9042;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;58;244.8283,-147.2706;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;57;285.3972,457.5728;Inherit;False;1;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;73;445.1215,-4.804946;Inherit;False;Property;_Contrast;Contrast;3;0;Create;True;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;620.0734,400.9138;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;2;243.4763,68.99157;Inherit;True;Property;_Normals;Normals;8;0;Create;True;0;0;False;0;-1;164b81533617247499e86da8845bbcc4;50db1768118f76d489afcdee99ab8c04;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;60;251.108,273.1737;Inherit;False;Property;_Metallic;Metallic;6;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;25;130.892,-340.9033;Inherit;True;Property;_Occlusion;Occlusion;9;0;Create;True;0;0;False;0;-1;95ac4b066fb1566438ed2d7938ad0c37;234817250c675f94cb603eb067190ed1;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleContrastOpNode;72;498.6005,-120.93;Inherit;False;2;1;COLOR;0,0,0,0;False;0;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;4;462.5802,-379.874;Inherit;False;Property;_Roughness;Roughness;5;0;Create;True;0;0;False;0;0.23;0.15;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;769.1523,-31.29886;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Amplify Leaf Blotchy;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.45;True;True;0;True;Opaque;;AlphaTest;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;7;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;10;0;8;0
WireConnection;7;1;10;0
WireConnection;33;0;7;0
WireConnection;33;1;28;0
WireConnection;6;0;7;0
WireConnection;6;1;28;0
WireConnection;34;0;33;0
WireConnection;22;0;6;0
WireConnection;67;0;64;0
WireConnection;68;0;67;1
WireConnection;68;1;67;2
WireConnection;36;0;29;0
WireConnection;36;1;34;0
WireConnection;30;0;22;0
WireConnection;30;1;29;0
WireConnection;38;0;30;0
WireConnection;38;1;36;0
WireConnection;65;0;68;0
WireConnection;65;1;71;0
WireConnection;70;0;65;0
WireConnection;70;1;59;0
WireConnection;70;2;61;0
WireConnection;39;0;38;0
WireConnection;27;0;30;0
WireConnection;27;1;36;0
WireConnection;27;2;39;0
WireConnection;58;0;1;0
WireConnection;58;1;70;0
WireConnection;40;0;57;2
WireConnection;40;1;27;0
WireConnection;72;1;58;0
WireConnection;72;0;73;0
WireConnection;0;0;72;0
WireConnection;0;1;2;0
WireConnection;0;3;60;0
WireConnection;0;4;4;0
WireConnection;0;5;25;1
WireConnection;0;10;1;4
WireConnection;0;11;40;0
ASEEND*/
//CHKSM=66A91FDE6DDE58FCC45F5DE8FE44E1ECEA3090D2