using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace EmbodimentDiscrepancy
{
	public class DiscrepancyPostProcessingEffects : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("set to /Packages/Post Processing/PostProcessing/PostProcessResources")]
		PostProcessResources postProcessResources;

		[SerializeField]
		[Tooltip("set to DiscrepancyPostProcessProfile")]
		PostProcessProfile postProcessProfileAsset;

		[SerializeField]
		[Tooltip("set to Prefabs/PostProcessVolumePrefab")]
		PostProcessVolume postProcessVolumePrefab;

		[SerializeField]
		[Tooltip("set to Camera (eye) in scene")]
		GameObject mainCam;

		[SerializeField]
		[Tooltip("layer for postprocessing objects, must exist")]
		string postProcessLayerName = "PostProcessing";

		[SerializeField]
		[Tooltip("distance at which all colors will have been shifted through")]
		float distToFullColorShift = 2;
		
		[SerializeField]
		[Range(0, 350)]
		[Tooltip("lower this to shift through fewer colors")]
		int maxShiftDegrees = 300;

		[SerializeField]
		[Tooltip("distance at which full desaturation is reached")]
		float distToFullDesaturation = 2;

		[SerializeField]
		[Range(0, -100)]
		[Tooltip("maximum desaturation amount")]
		float maxDesaturation = -100;

		[SerializeField]
		[Tooltip("time over which to smooth sudden jump from 0 after tolerance time is reached")]
		float smoothingTime = 1;


		PostProcessVolume postProcessVolume;

		ColorGrading colorGradingSetting;
		
		float curMaxDiscrepancyDistanceColorShift = 0; //store the highest distance of all discrepancies, reset after frame
		float prevMaxDiscrepancyDistanceColorShift = 0;
		bool isSmoothingColorShift = false;
		float curMaxDiscrepancyDistanceDesaturation = 0;
		float prevMaxDiscrepancyDistanceDesaturation = 0;
		bool isSmoothingDesaturation = false;


		public void HandleColorShift(Discrepancy disc)
		{
			//effect will be based on largest discrepancy -> store the highest distance
			if (disc.distance > curMaxDiscrepancyDistanceColorShift){
				curMaxDiscrepancyDistanceColorShift = disc.distance;
			}
		}

		public void HanldeDesaturation(Discrepancy disc)
		{
			//effect will be based on largest discrepancy -> store the highest distance
			if (disc.distance > curMaxDiscrepancyDistanceDesaturation){
				curMaxDiscrepancyDistanceDesaturation = disc.distance;
			}
		}


		void Start()
		{
			mainCam.gameObject.layer = LayerMask.NameToLayer(postProcessLayerName);

			PostProcessLayer postProcessLayer = mainCam.gameObject.GetComponent<PostProcessLayer>();
			if (postProcessLayer == null){
				postProcessLayer = mainCam.gameObject.AddComponent<PostProcessLayer>();
				postProcessLayer.Init(postProcessResources);
			}
			postProcessLayer.volumeTrigger = mainCam.transform;
			postProcessLayer.volumeLayer |= 1 << LayerMask.NameToLayer(postProcessLayerName);

			postProcessVolume = Instantiate(postProcessVolumePrefab);
			postProcessVolume.transform.parent = this.transform;
			postProcessVolume.gameObject.layer = LayerMask.NameToLayer(postProcessLayerName);
			postProcessVolume.profile = postProcessProfileAsset;

			bool gotSetting = postProcessVolume.profile.TryGetSettings<ColorGrading>(out colorGradingSetting);
			if (!gotSetting){
				Debug.LogError("no colorgrading setting found");
			}
			SetColorShift(0);
		}

		IEnumerator ResetAfterFrame(){
			yield return new WaitForEndOfFrame();
			curMaxDiscrepancyDistanceColorShift = 0;
			curMaxDiscrepancyDistanceDesaturation = 0;
		}


		void SetDesaturation(float distance)
		{
			float newSat = Mathf.Lerp(0, -100, distance / distToFullDesaturation);
			colorGradingSetting.saturation.overrideState = true;
			colorGradingSetting.saturation.value = newSat;
		}

		IEnumerator SmoothAdjustDesaturation(float time)
		{
			isSmoothingDesaturation = true;
			while (time > 0)
			{
				float smoothedDistance = Mathf.Lerp(curMaxDiscrepancyDistanceDesaturation, 0, time -= Time.deltaTime);
				SetDesaturation(smoothedDistance);
				yield return null;
			}
			isSmoothingDesaturation = false;
		}


		void SetColorShift(float distance)
		{
			//hueshift goes from -180 to 180, 0 is neutral, 180 wraps to -180.
			float newShift = Mathf.Lerp(0, maxShiftDegrees, distance / distToFullColorShift);
			if (newShift > 180){
				newShift = -180 + (newShift - 180);
			}
			colorGradingSetting.hueShift.overrideState = true;
			colorGradingSetting.hueShift.value = newShift;
		}

		IEnumerator SmoothAdjustColorShift(float time)
		{
			isSmoothingColorShift = true;
			while (time > 0)
			{
				float smoothedDistance = Mathf.Lerp(curMaxDiscrepancyDistanceColorShift, 0, time -= Time.deltaTime);
				SetColorShift(smoothedDistance);
				yield return null;
			}
			isSmoothingColorShift = false;
		}

		//handle everything after all Updates done, to make sure Handle*() functions called before
		void LateUpdate()
		{
			if (curMaxDiscrepancyDistanceColorShift > 0)
			{	
				//smooth out initial distance jump after tolerance timer reached
				if (prevMaxDiscrepancyDistanceColorShift == 0	 && !isSmoothingColorShift){
					StartCoroutine(SmoothAdjustColorShift(smoothingTime));
				}
				if (!isSmoothingColorShift){ //after smoothing set directly
					SetColorShift(curMaxDiscrepancyDistanceColorShift);
				}
			}
			prevMaxDiscrepancyDistanceColorShift = curMaxDiscrepancyDistanceColorShift;
			
			if (curMaxDiscrepancyDistanceDesaturation > 0)
			{	
				//smooth out initial distance jump after tolerance timer reached
				if (prevMaxDiscrepancyDistanceDesaturation == 0	 && !isSmoothingDesaturation){
					StartCoroutine(SmoothAdjustDesaturation(smoothingTime));
				}
				if (!isSmoothingDesaturation){ //after smoothing set directly
					SetDesaturation(curMaxDiscrepancyDistanceDesaturation);
				}
			}
			prevMaxDiscrepancyDistanceDesaturation = curMaxDiscrepancyDistanceDesaturation;
			
			
			StartCoroutine(ResetAfterFrame());
		}
	}
}