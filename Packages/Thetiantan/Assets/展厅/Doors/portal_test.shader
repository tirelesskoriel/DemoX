Shader "Unlit/portal_test"
{
    Properties
    {
        [Enum(CompareFunction)] _StencilComp("Stencil Comp", Int) = 3
        _Ref("World", Int) = 1
        _MainColor("Main Color", Color) = (1,1,1,1) // 颜色属性
        _MainTex("Albedo", 2D) = "white" {}
        _TintColor("Tint Color", Color) = (1,1,1,1) // 颜色属性
        _Emission("Emission Intensity", Float) = 1.0 // 自发光强度属性
    }

        SubShader
        {
            Tags { "RenderType" = "Opaque" "PerformanceChecks" = "False" }
            LOD 300

            Stencil
            {
                Ref[_Ref]
                Comp[_StencilComp]
            }

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fog

                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    UNITY_FOG_COORDS(1)
                    float4 vertex : SV_POSITION;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                fixed4 _MainColor; // 颜色属性的uniform变量
                fixed4 _TintColor; // 颜色属性的uniform变量
                float _Emission; // 自发光强度的uniform变量

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    UNITY_TRANSFER_FOG(o,o.vertex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    // sample the texture
                    fixed4 texColor = tex2D(_MainTex, i.uv);
                // multiply texture color with the tint color
                fixed4 col = _MainColor * _TintColor;
;
                float rr = (i.uv.x - 0.5) * (i.uv.x - 0.5) + (i.uv.y - 0.5) * (i.uv.y - 0.5);

                col.rgb = lerp(_MainColor, _TintColor,  sqrt(rr) * _Emission); // 
                // apply emission intensity
                //col *= _Emission;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
        }
            FallBack "Diffuse"

}