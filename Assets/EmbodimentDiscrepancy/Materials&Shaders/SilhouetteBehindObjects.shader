// adapted from https://www.youtube.com/watch?v=EthjeNeNTsM

Shader "Embodiment/SilhouetteBehindObjects"
{
	Properties
	{
		_Color ("Silhouette color", Color) = (0,0,0,0)
         _Stencil ("if this stencil set, don't draw", Float) = 0	
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		LOD 100

		Stencil //if stencil is _Stencil, don't do anything
		{
			Ref [_Stencil]
			Comp notequal
			Pass keep
		}

		Pass
		{
			Cull Off
			ZWrite Off
			ZTest Greater //Only show silhuette if other objects in front

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

			float4 _Color;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return _Color;
			}
			ENDCG
		}
	}
}
