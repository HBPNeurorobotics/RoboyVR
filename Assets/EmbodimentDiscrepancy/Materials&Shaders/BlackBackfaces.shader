
//https://forum.unity.com/threads/render-backface-pure-black.46630/#post-296208

Shader "Embodiment/BlackBackfaces" 
{
	SubShader 
	{
		Pass 
		{
			Cull front
			Color (0,0,0,1)
		}
	}
}
