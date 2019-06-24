Shader "Unlit/Cubre Pantalla Simple"
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
	}
		SubShader
	{
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
		LOD 100
		ZWrite Off
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
			
			#include "UnityCG.cginc"

			struct appdata
			{
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				fixed4 screenPos:TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				
				o.screenPos = ComputeScreenPos(o.vertex);
				o.screenPos.x *= _ScreenParams.x/_ScreenParams.y;
				o.screenPos = float4( TRANSFORM_TEX(o.screenPos, _MainTex).xy,o.screenPos.zw);
				//UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.screenPos.xy).a;
				
				return col;
			}
			ENDCG
		}
	}
}
