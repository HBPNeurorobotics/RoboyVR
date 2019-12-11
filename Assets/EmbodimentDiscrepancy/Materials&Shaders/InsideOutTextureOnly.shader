
//https://forum.unity.com/threads/what-simple-shader-only-shows-diffuse-without-lighting.41054/#post-262044

Shader "Embodiment/Inside Out Texture Only"
{
	Properties {
		_MainTex ("Texture", 2D) = ""
	}

	SubShader 
	{
		Pass
		{
			Cull Front
			SetTexture [_MainTex]
		}
	}
}