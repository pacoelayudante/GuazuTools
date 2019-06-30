Shader "Unlit/Solo Estencil"
{
	Properties
	{
        [Enum(UnityEngine.Rendering.CullMode)]_Cull ("Cull Mode", Int) = 2
        [MaterialToggle]_ZWrite ("Z Write", Int) = 1
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest ("Z Test", Int) = 4
        [Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp ("Stencil Comparison", Int) = 8
        [MaskFlagsDrawer]_Stencil ("Stencil Ref Value", Int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilOp ("Stencil Pass Op", Int) = 0
        [MaskFlagsDrawer]_StencilWriteMask ("Stencil Write Mask", Int) = 255
        [MaskFlagsDrawer]_StencilReadMask ("Stencil Read Mask", Int) = 255
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilFail ("Stencil Fail Op", Int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilZFail ("Stencil ZFail Op", Int) = 0
	}
	SubShader
	{
		Stencil{
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
            Fail [_StencilFail]
            ZFail [_StencilZFail]
		}

		ColorMask 0
        Lighting Off
		Tags { "RenderType"="Opaque" "Queue"="Background"}
		ZWrite [_ZWrite]
		ZTest [_ZTest]
		Blend Off
		Cull [_Cull]

		Pass
		{
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
						
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return fixed4(1.0,1.0,1.0,1.0);
			}
			ENDCG
		}
	}
}
