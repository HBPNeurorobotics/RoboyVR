using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmbodimentDiscrepancy
{

	public enum TrackedJoint
	{
		Head, HandLeft, HandRight, FootLeft, FootRight
	}

	public struct Discrepancy
	{
		public TrackedJoint joint;
		public Transform trackedPos;
		public Transform simulatedPos;
		public float distance;
		public float duration;
	}

	public class DiscrepancyHandler : MonoBehaviour
	{

		[SerializeField]
		[Tooltip("Set to instance in scene, not a prefab. If not set, will be searched in scene")]
		DiscrepancyLineHandler discrepancyLineHandler;

		[SerializeField]
		[Tooltip("If not set, will be searched in scene")]
		DiscrepancyHeadEffects discrepancyHeadEffects;

		[SerializeField]
		[Tooltip("If not set, will be searched in scene")]
		DiscrepancyHapticHandler discrepancyHapticHandler;

		[SerializeField]
		[Tooltip("If not set, will be searched in scene")]
		DiscrepancySoundHandler discrepancySoundHandler;
	
		[SerializeField]
		[Tooltip("If not set, will be searched in scene")]
		DiscrepancyIndicatorHandler discrepancyIndicatorHandler;

		[SerializeField]
		[Tooltip("If not set, will be searched in scene")]
		DiscrepancyPostProcessingEffects discrepancyPostProcessingEffects;

		public bool lineEffectHands = true;
		public bool lineEffectFeet = true;
		public bool hapticEffectHands = true;
		public bool hapticEffectFeet = true;
		public bool geigerSoundEffectHands = true;
		public bool geigerSoundEffectFeet = true;
		public bool noiseSoundEffectHands = true;
		public bool noiseSoundEffectFeet = true;
		public bool discrepancyIndicatorEffectHands = true;
		public bool discrepancyIndicatorEffectFeet = true;
		public bool colorShiftEffectHands = true;
		public bool colorShiftEffectFeet = true;
		public bool colorShiftEffectHead = true;
		public bool desaturationEffectHands = true;
		public bool desaturationEffectFeet = true;
		public bool desaturationEffectHead = true;
		public bool blurEffect = true;
		public bool fadeToBlackEffect = true;

		[SerializeField]
		[Tooltip("seconds before discrepancy is handled at Hands")]
		[Range(0, 3)]
		public float toleranceTimeHands = 1;

		[SerializeField]
		[Tooltip("seconds before discrepancy is handled at Feet")]
		[Range(0, 3)]
		public float toleranceTimeFeet = 1;

		[SerializeField]
		[Tooltip("seconds before discrepancy is handled at Head")]
		[Range(0, 1)]
		public float toleranceTimeHead = 0.5f;

		List<Discrepancy> discrepancyList = new List<Discrepancy>();


		public void HandleDiscrepancy(Discrepancy disc)
		{
			discrepancyList.Add(disc);
		}

		void Update()
		{
			foreach (Discrepancy disc in discrepancyList)
			{
				if (disc.joint == TrackedJoint.HandLeft || disc.joint == TrackedJoint.HandRight)
				{
					if (lineEffectHands && (disc.duration > toleranceTimeHands)){
						discrepancyLineHandler.DrawLine(disc);
					}
					if (hapticEffectHands && (disc.duration > toleranceTimeHands)){
						discrepancyHapticHandler.HandleRumble(disc);
					}
					if (geigerSoundEffectHands && (disc.duration > toleranceTimeHands)){
						discrepancySoundHandler.HandleGeigerSounds(disc);
					}
					if (noiseSoundEffectHands){
						discrepancySoundHandler.HandleNoise(disc);
					}
					if (discrepancyIndicatorEffectHands && (disc.duration > toleranceTimeHands)){
						discrepancyIndicatorHandler.HandleIndicator(disc);
					}
					if (colorShiftEffectHands && (disc.duration > toleranceTimeHands)){
						discrepancyPostProcessingEffects.HandleColorShift(disc);
					}
					if (desaturationEffectHands && (disc.duration > toleranceTimeHands)){
						discrepancyPostProcessingEffects.HanldeDesaturation(disc);
					}
				}

				if (disc.joint == TrackedJoint.FootLeft || disc.joint == TrackedJoint.FootRight)
				{
					if (lineEffectFeet && (disc.duration > toleranceTimeFeet)){
						discrepancyLineHandler.DrawLine(disc);
					}
					if (hapticEffectFeet && (disc.duration > toleranceTimeFeet)){
						discrepancyHapticHandler.HandleRumble(disc);
					}
					if (geigerSoundEffectFeet && (disc.duration > toleranceTimeFeet)){
						discrepancySoundHandler.HandleGeigerSounds(disc);
					}
					if (noiseSoundEffectFeet){
						discrepancySoundHandler.HandleNoise(disc);
					}
					if (discrepancyIndicatorEffectFeet && (disc.duration > toleranceTimeFeet)){
						discrepancyIndicatorHandler.HandleIndicator(disc);
					}
					if (colorShiftEffectFeet && (disc.duration > toleranceTimeFeet)){
						discrepancyPostProcessingEffects.HandleColorShift(disc);
					}
					if (desaturationEffectFeet && (disc.duration > toleranceTimeFeet)){
						discrepancyPostProcessingEffects.HanldeDesaturation(disc);
					}
				}

				if (disc.joint == TrackedJoint.Head)
				{
					if (fadeToBlackEffect && (disc.duration > toleranceTimeHead)){
						discrepancyHeadEffects.HandleFade(disc);
					}
					if (blurEffect && (disc.duration > toleranceTimeHead)){
						discrepancyHeadEffects.HandleBlur(disc);
					}
					if (colorShiftEffectHead && (disc.duration > toleranceTimeHead)){
						discrepancyPostProcessingEffects.HandleColorShift(disc);
					}
					if (desaturationEffectHead && (disc.duration > toleranceTimeHead)){
						discrepancyPostProcessingEffects.HanldeDesaturation(disc);
					}
				}
			}
			discrepancyList.Clear(); //all discrepancies handled, clear for next frame
		}

		// Use this for initialization
		void Start()
		{
			if (discrepancyLineHandler == null)
			{
				discrepancyLineHandler = GameObject.FindObjectOfType<DiscrepancyLineHandler>();
				if (discrepancyLineHandler == null){
					Debug.LogError("no DiscrepancyLineHandler found");
				}
			}

			if (discrepancyHeadEffects == null)
			{
				discrepancyHeadEffects = GameObject.FindObjectOfType<DiscrepancyHeadEffects>();
				if (discrepancyHeadEffects == null){
					Debug.LogError("no DiscrepancyHeadEffects found");
				}
			}

			if (discrepancyHapticHandler== null)
			{
				discrepancyHapticHandler = GameObject.FindObjectOfType<DiscrepancyHapticHandler>();
				if (discrepancyHapticHandler == null){
					Debug.LogError("no DiscrepancyHapticHandler found");
				}
			}
			
			if (discrepancySoundHandler== null)
			{
				discrepancySoundHandler = GameObject.FindObjectOfType<DiscrepancySoundHandler>();
				if (discrepancySoundHandler == null){
					Debug.LogError("no DiscrepancySoundHandler found");
				}
			}

			if (discrepancyIndicatorHandler== null)
			{
				discrepancyIndicatorHandler = GameObject.FindObjectOfType<DiscrepancyIndicatorHandler>();
				if (discrepancyIndicatorHandler == null){
					Debug.LogError("no DiscrepancyIndicatorHandler found");
				}
			}

			if (discrepancyPostProcessingEffects== null)
			{
				discrepancyPostProcessingEffects = GameObject.FindObjectOfType<DiscrepancyPostProcessingEffects>();
				if (discrepancyPostProcessingEffects == null){
					Debug.LogError("no DiscrepancyPostProcessingEffects found");
				}
			}
		}
	}
}