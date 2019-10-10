using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace EmbodimentDiscrepancy
{
	public class DiscrepancySoundHandler : MonoBehaviour
	{
		enum SoundOrigin {
			trackedPos, simulatedPos
		}

		[SerializeField]
		SoundOrigin SoundComesFrom = SoundOrigin.simulatedPos;


		//GeigerClicks

		class GeigerClicker 
		{
			public GeigerClicker(TrackedJoint joint){
				this.joint = joint;
			}

			public TrackedJoint joint;
			public float intensity = 0;
			float timer = 0;
			int prevAudioClipIndex = 0;

			public void DoClick(AudioSource audioSource, List<AudioClip> geigerClickClips, float deltaTime, int maxClicksPerMinute, float variance, float intensity)
			{
				timer += Time.deltaTime;
				if (intensity > 0)
				{
					float clicksPerMinute = Mathf.Lerp(0, maxClicksPerMinute, intensity);
					if (variance > 0){
						clicksPerMinute = Random.Range(clicksPerMinute - clicksPerMinute * variance, clicksPerMinute + (clicksPerMinute * variance));
					}
					clicksPerMinute = (clicksPerMinute < 1) ? 1 : clicksPerMinute; //should always be > 1
					if (timer > (60f / clicksPerMinute))
					{
						//choose random audioclip, without playing same one back to back
						int audioClipIndex = Random.Range(0, geigerClickClips.Count);
						audioClipIndex = (audioClipIndex == prevAudioClipIndex) ? audioClipIndex + 1 : audioClipIndex;
						audioClipIndex = (audioClipIndex >= geigerClickClips.Count) ? (geigerClickClips.Count - 1) : audioClipIndex;
						audioSource.PlayOneShot(geigerClickClips[audioClipIndex]);
						timer = 0;
						prevAudioClipIndex = audioClipIndex;
					}
				}
			}
		}

		[SerializeField]
		List<AudioClip> geigerClickClipsSingle = new List<AudioClip>();

		[SerializeField]
		List<AudioClip> geigerClickClipsMulti = new List<AudioClip>();

		List<AudioClip> geigerClickClips = new List<AudioClip>();

		[SerializeField]
		[Tooltip("set to Prefabs/GeigerAudioSourcePrefab")]
		AudioSource geigerAudioSourcePrefab;

		[SerializeField]
		[Tooltip("only applies on Start(), set via property UseMultiClicks at runtime")]
		bool geigerUseMultiClicks = true;
		public bool GeigerUseMultiClicks
		{
			get{
				return geigerUseMultiClicks;
			}
			set
			{
				geigerUseMultiClicks = value;
				geigerClickClips.Clear();
				if (geigerUseMultiClicks){
					geigerClickClips.AddRange(geigerClickClipsSingle);
					geigerClickClips.AddRange(geigerClickClipsMulti);
				}
				else{
					geigerClickClips.AddRange(geigerClickClipsSingle);
				}
			}
		}

		[SerializeField]
		float geigerDistanceToMaxIntensity = 1;

		[SerializeField]
		int geigerMaxClicksPerMinute = 600;

		[SerializeField]
		[Range(0, 1)]
		float geigerVariance = 0.5f;

		Dictionary<TrackedJoint, AudioSource> geigerAudioSourceDict = new Dictionary<TrackedJoint, AudioSource>();
		List<GeigerClicker> geigerClickerList = new List<GeigerClicker>();

		//!GeigerClicks

		//Noise

		[SerializeField]
		[Tooltip("will be applied to all NoiseAudioSources")]
		AudioClip noiseClipInspector;

		AudioClip noiseClip;
		public AudioClip NoiseClip 
		{
			get {
				return noiseClip;
			}
			set {
				foreach (AudioSource audioSource in noiseAudioSourceDict.Values){
					audioSource.clip = value;
				}
				noiseClip = value;
			}

		}

		[SerializeField]
		[Tooltip("set to Prefabs/NoiseAudioSourcePrefab")]
		AudioSource noiseAudioSourcePrefab;

		Dictionary<TrackedJoint, AudioSource> noiseAudioSourceDict = new Dictionary<TrackedJoint, AudioSource>();

		[SerializeField]
		float noiseDistanceToMaxVolume = 1;

		//!Noise


		//needs to be called every frame it should apply
		public void HandleNoise(Discrepancy disc)
		{
			//update AudioSources
			if (!noiseAudioSourceDict.ContainsKey(disc.joint)){
				AudioSource newAudioSource = Instantiate(original: noiseAudioSourcePrefab, parent: this.transform);
				newAudioSource.clip = NoiseClip;
				newAudioSource.loop = true;
				newAudioSource.Play();
				noiseAudioSourceDict[disc.joint] = newAudioSource;
			}
			else {
				if (SoundComesFrom == SoundOrigin.simulatedPos){
					noiseAudioSourceDict[disc.joint].transform.position = disc.simulatedPos.transform.position;
				}
				else if (SoundComesFrom == SoundOrigin.trackedPos){
					noiseAudioSourceDict[disc.joint].transform.position = disc.trackedPos.transform.position;
				}
			}
			noiseAudioSourceDict[disc.joint].volume = Mathf.Lerp(0, 1, disc.distance / noiseDistanceToMaxVolume);
		}

		//needs to be called every frame it should apply
		public void HandleGeigerSounds(Discrepancy disc)
		{
			//update AudioSources
			if (!geigerAudioSourceDict.ContainsKey(disc.joint)){
				geigerAudioSourceDict[disc.joint] = Instantiate(original: geigerAudioSourcePrefab, parent: this.transform);
			}
			else {
				if (SoundComesFrom == SoundOrigin.simulatedPos){
					geigerAudioSourceDict[disc.joint].transform.position = disc.simulatedPos.transform.position;
				}
				else if (SoundComesFrom == SoundOrigin.trackedPos){
					geigerAudioSourceDict[disc.joint].transform.position = disc.trackedPos.transform.position;
				}
			}

			//update GeigerClickers
			GeigerClicker geigerClicker = null;
			foreach (GeigerClicker entry in geigerClickerList)
			{
				if (entry.joint == disc.joint){
					geigerClicker = entry;
				}
			}
			if (geigerClicker == null)
			{
				geigerClicker = new GeigerClicker(disc.joint);
				geigerClickerList.Add(geigerClicker);
			}
			geigerClicker.intensity = Mathf.Lerp(0, 1, disc.distance / geigerDistanceToMaxIntensity);
		}

		void Start()
		{
			GeigerUseMultiClicks = geigerUseMultiClicks;
			if (geigerClickClips.Count < 1){
				Debug.LogError("no Geiger Click sounds set");
			}

			NoiseClip = noiseClipInspector;
		}

		void Update()
		{
			foreach (GeigerClicker geigerClicker in geigerClickerList)
			{
				AudioSource audioSource = geigerAudioSourceDict[geigerClicker.joint];
				geigerClicker.DoClick(audioSource, geigerClickClips, Time.deltaTime, geigerMaxClicksPerMinute, geigerVariance, geigerClicker.intensity);
				geigerClicker.intensity = 0; //reset intensity until next HandleGeigerSounds()
			}

			if (NoiseClip != noiseClipInspector){
				NoiseClip = noiseClipInspector;
			}
			StartCoroutine(noiseResetVolume()); //reset noise volume to 0 after each frame
		}

		IEnumerator noiseResetVolume(){
			yield return new WaitForEndOfFrame();
			foreach (AudioSource audioSource in noiseAudioSourceDict.Values){
				audioSource.volume = 0;	
			}
		}
	}
}