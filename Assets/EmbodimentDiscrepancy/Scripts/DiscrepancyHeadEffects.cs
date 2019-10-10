using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmbodimentDiscrepancy
{
	public class DiscrepancyHeadEffects : MonoBehaviour
	{

		[SerializeField]
		[Tooltip("If not set, will be searched in scene")]
		SteamVR_Camera steamvrCam;

		[SerializeField]
		[Range(0, 1)]
		float maxDistToBlack = 0.3f;


		[SerializeField]
		[Range(0, 1)]
		float maxDistToFullBlur = 0.3f;

		//	----------------------------------------------------------------------------------
		//	SuperBlur taken from: https://github.com/PavelDoGreat/Super-Blur
		//	----------------------------------------------------------------------------------
		SuperBlur.SuperBlur superBlur;

		[SerializeField]
		[Range(1, 8)]
		int superBlurIterations = 5;

		[SerializeField]
		SuperBlur.BlurKernelSize superBlurKernelsize = SuperBlur.BlurKernelSize.Big;

		[SerializeField]
		[Tooltip("set to SuperBlur/Material/SuperBlurPostEffect material")]
		Material superBlurPostEffectMaterial;

		[SerializeField]
		[Tooltip("set to SuperBlur/Material/SuperBlurUI material")]
		Material superBlurUIMaterial;


		float curFadeToBlackDist = 0;
		float curBlurDist = 0;
		float prevBlurDist = 0;
		float prevFadeToBlackDist = 0;
		bool isSmoothingFade = false;
		bool isSmoothingBlur = false;


		void Start()
		{
			if (steamvrCam == null)
			{
				steamvrCam = GameObject.FindObjectOfType<SteamVR_Camera>();
				if (steamvrCam == null){
					Debug.LogError("no SteamVR_Camera found");
				}
				if (steamvrCam.gameObject.GetComponent<SteamVR_Fade>() == null){
					steamvrCam.gameObject.AddComponent<SteamVR_Fade>();
				}
			}

			if (steamvrCam.gameObject.GetComponent<SuperBlur.SuperBlur>() == null){
				superBlur = steamvrCam.gameObject.AddComponent<SuperBlur.SuperBlur>();
			}
			if (superBlurPostEffectMaterial == null || superBlurUIMaterial == null){
				Debug.LogError("superBlur Material(s) not set");
			}
			superBlur.gammaCorrection = false;
			superBlur.iterations = superBlurIterations;
			superBlur.kernelSize = superBlurKernelsize;
			superBlur.downsample = 0;
			superBlur.interpolation = 0;
			superBlur.blurMaterial = superBlurPostEffectMaterial;
			superBlur.UIMaterial = superBlurUIMaterial;
		}

		
		//handle everything after all Updates done, to make sure Handle*() functions called before
		void LateUpdate()
		{
			superBlur.iterations = superBlurIterations;
			superBlur.kernelSize = superBlurKernelsize;

			if (curFadeToBlackDist > 0)
			{
				//smooth out initial distance jump after tolerance timer reached
				if (prevFadeToBlackDist == 0 && !isSmoothingFade){
					StartCoroutine(FadeSmoothing(1f));
				}
				if (!isSmoothingFade){
					SetFade(curFadeToBlackDist); //after smoothing set directly
				}
			}	
			prevFadeToBlackDist = curFadeToBlackDist;

			if (curBlurDist > 0)
			{
				//smooth out initial distance jump after tolerance timer reached
				if (prevBlurDist == 0 && !isSmoothingBlur){
					StartCoroutine(BlurSmoothing(1f));
				}
				if (!isSmoothingBlur){
					SetBlur(curBlurDist); //after smoothing set directly
				}
			}
			prevBlurDist = curBlurDist;

			StartCoroutine(ResetAfterFrame()); //reset after each frame
		}

		IEnumerator ResetAfterFrame(){
			yield return new WaitForEndOfFrame();
			curFadeToBlackDist = 0;
			SetFade(0);
			curBlurDist = 0;
			SetBlur(0);
		}
		
		
		//needs to be called every frame it should apply
		public void HandleFade(Discrepancy disc)
		{
			curFadeToBlackDist = disc.distance;
		}

		void SetFade(float distance)
		{
			float alpha = distance / maxDistToBlack;
			SteamVR_Fade.Start(newColor: new Color(0, 0, 0, alpha), duration: 0);
		}

		IEnumerator FadeSmoothing(float time)
		{
			isSmoothingFade = true;
			while (time > 0)
			{
				float smoothedDistance = Mathf.Lerp(curFadeToBlackDist, 0, time -= Time.deltaTime);
				SetFade(smoothedDistance);
				yield return null;
			}
			isSmoothingFade = false;
		}


		//needs to be called every frame it should apply
		public void HandleBlur(Discrepancy disc)
		{
			curBlurDist = disc.distance;
		}
		void SetBlur(float distance)
		{
			float strength = distance / maxDistToFullBlur;
			superBlur.interpolation = strength;
		}

		IEnumerator BlurSmoothing(float time)
		{
			isSmoothingBlur = true;
			while (time > 0)
			{
				float smoothedDistance = Mathf.Lerp(curBlurDist, 0, time -= Time.deltaTime);
				SetBlur(smoothedDistance);
				yield return null;
			}
			isSmoothingBlur = false;
		}
	}
}