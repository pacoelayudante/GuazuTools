Shader "Unlit/Fill De Pantalla"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendSrc("Blend Source", Int)=1
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendDst("Blend Dest",Int)=10

        [Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp ("Stencil Comparison", Int) = 8
        [MaskFlagsDrawer]_Stencil ("Stencil ID", Int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilOp ("Stencil Operation", Int) = 0
        [MaskFlagsDrawer]_StencilWriteMask ("Stencil Write Mask", Int) = 255
        [MaskFlagsDrawer]_StencilReadMask ("Stencil Read Mask", Int) = 255
		[MaterialToggle] ExpandirQuad("(Solo Quad) Expandir a pantalla", Int) = 0
	}
		SubShader
	{
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent+500" }
		LOD 100
		ZWrite Off
		ZTest Off
        Lighting Off
		Cull Off
        Blend [_BlendSrc] [_BlendDst]

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ EXPANDIRQUAD_ON
			
			#include "UnityCG.cginc"

			struct appdata
			{
#if EXPANDIRQUAD_ON
                float4 uv : TEXCOORD0;
#else
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
#endif
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
#if EXPANDIRQUAD_ON
				o.vertex = (v.uv - float4(.5, .5, -.5, .5));
#else
				o.vertex = UnityObjectToClipPos(v.vertex);
#endif

#if EXPANDIRQUAD_ON
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv.x *= _ScreenParams.x/_ScreenParams.y;
#else
				float4 screenPos = ComputeScreenPos(o.vertex);
				screenPos.x *= _ScreenParams.x/_ScreenParams.y;
				o.uv = TRANSFORM_TEX(screenPos, _MainTex);
#endif
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				
				return col;
			}
			ENDCG
		}
	}
}
