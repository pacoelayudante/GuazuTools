// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
// modificado para que sea mas generico el blending
Shader "Sprites/Blend Generico"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		[MaterialToggle] SoloAlfa("Ignora RGB de Sprite", Int) = 0
		[MaterialToggle] IgnoraRendAlfa("Ignora Alfa del Renderer", Int) = 0
		[MaterialToggle] Premultiply("Premultiply Alfa y Color", Int) = 1
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendSrc("Blend Source", Int)=1
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendDst("Blend Dest",Int)=10
		[Enum(UnityEngine.Rendering.BlendOp)] _BlendOp("Blend Operation",Int)=0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend [_BlendSrc] [_BlendDst]
        BlendOp [_BlendOp]

        Pass
        {
        CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment SpriteFragBlendGenerico
            #pragma target 2.0
            #pragma multi_compile_instancing
			#pragma multi_compile _ PIXELSNAP_ON
			#pragma multi_compile _ SOLOALFA_ON
			#pragma multi_compile _ PREMULTIPLY_ON
			#pragma multi_compile _ IGNORARENDALFA_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

		fixed4 SpriteFragBlendGenerico(v2f IN) : SV_Target
		{
#ifdef SOLOALFA_ON
			fixed4 c = IN.color;
	#if IGNORARENDALFA_ON
			c.a = SampleSpriteTexture(IN.texcoord).a;
	#else
			c.a *= SampleSpriteTexture(IN.texcoord).a;
	#endif
#else
			fixed4 c = SampleSpriteTexture(IN.texcoord);
	#if IGNORARENDALFA_ON
			c.rgb *= IN.color.rgb;
	#else
			c *= IN.color;
	#endif
#endif
#if PREMULTIPLY_ON
			c.rgb *= c.a;
#endif
			return c;
		}

        ENDCG
        }
    }
}
