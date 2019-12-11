//adapted from: https://github.com/PushyPixels/BreakfastWithUnity/blob/master/Assets/54StencilBufferShader/Shaders/MaskOneZLess.shader

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Embodiment/Stencil/SetStencilZLess"
{
	Properties
	{
        _Stencil ("Stencil ID", Float) = 1
	}

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry-1" }
        ColorMask 0 //don't color
        ZWrite off
        
        Stencil //set stencil to 1	
        {
            Ref 1
            Comp always
            Pass replace
        }
        
        Pass
        {
            Cull Back
            ZTest Less
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            struct appdata
            {
                float4 vertex : POSITION;
            };
            struct v2f
            {
                float4 pos : SV_POSITION;
            };
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            half4 frag(v2f i) : COLOR
            {
                return half4(1,1,0,1);
            }
            
            ENDCG
        }
    } 
}