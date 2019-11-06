using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace EmbodimentDiscrepancy
{
	public class DiscrepancyHapticHandler : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("how many rumble pulses can be sent out per second")]
        [Range(1,20)]
		int maxRumblesPerSecond = 10;
		
		[SerializeField]
		[Tooltip("seconds after which maxRumblesPerSecond is reached")]
        [Range(0.1f, 10)]
		float maxRumblesTime = 5;

		[SerializeField]
		[Tooltip("maximum pulse length in seconds")]
        [Range(0, 1)]
		float maxRumblePulseDuration = 0.1f;

		[SerializeField]
		[Tooltip("distance at which maxRumblePulseDuration is reached")]
        [Range(0.1f, 2)]
		float maxRumblePulseDistance = 1;

		[SerializeField]
		[Tooltip("strength of each pulse")]
		[Range(0, 1)]
		float pulseStrength = 1;

        [SerializeField] private TrackingIKTargetManager trackingIKTargetManager;
        [SerializeField] private SteamVR_Action_Vibration vibrateAction;
        
		private SteamVR_TrackedObject handLeftTrackedObject, handRightTrackedObject, footLeftTrackedObject, footRightTrackedObject;

		Dictionary<TrackedJoint, SteamVR_TrackedObject> trackedObjectDict = new Dictionary<TrackedJoint, SteamVR_TrackedObject>();
		List<Rumble> rumbleList = new List<Rumble>();


		class Rumble
		{
			public int trackedObjectIndex;
			public float timeTillNextPulse;
			public float pulseLength;
			public bool sentOut = true;
		}

		//needs to be called every frame it should apply
		public void HandleRumble(Discrepancy disc)
		{
			SteamVR_TrackedObject trackedObject;
			if (trackedObjectDict.ContainsKey(disc.joint)){
				trackedObject = trackedObjectDict[disc.joint];
			}
			else
			{
				Debug.Log("haptic not set up for " + disc.joint.ToString() + " (no SteamVR_TrackedObject set)");
				return;
			}

			//create rumble for joint if it doesn't exist
			Rumble rumble = null;
			for (int i = 0; i < rumbleList.Count; i++)
			{
				if (rumbleList[i].trackedObjectIndex == (int)trackedObject.index){
					rumble = rumbleList[i];
				}
			}
			if(rumble == null){
				rumble = new Rumble { trackedObjectIndex = (int)trackedObject.index };
				rumbleList.Add(rumble);
			}

			//only apply new rumble when next pulse allowed
			if (rumble.sentOut && rumble.timeTillNextPulse <= 0)
			{
				//length of pulses dependant on distance of discrepancy
				//larger distance -> longer pulse
				rumble.pulseLength = (Mathf.Lerp(0, 1, disc.distance / maxRumblePulseDistance) * (maxRumblePulseDuration));
				//frequency of pulses dependant on duration of discrepancy
				//longer discrepancy -> more frequent pulses
				rumble.timeTillNextPulse = (1.0f / Mathf.Lerp(0, 1, disc.duration / maxRumblesTime)) * (1.0f / maxRumblesPerSecond);
				rumble.sentOut = false;
			}
		}

		//adapted from https://steamcommunity.com/app/358720/discussions/0/405693392914144440/#c357284767229628161
		//length is how long the vibration should go for
		//strength is vibration strength from 0-1
        IEnumerator LongVibration(Rumble rumble)
        {
			for(float i = 0; i < rumble.pulseLength; i += Time.deltaTime)
            {
                //SteamVR_Controller.Input((int)rumble.trackedObjectIndex).TriggerHapticPulse((ushort)Mathf.Lerp(0, 3999, pulseStrength));
                Debug.Log("VIBRATION");
				yield return null;
			}
            rumble.sentOut = true;
		}

		void SetTrackedObjectForJoint(TrackedJoint joint, SteamVR_TrackedObject trackedObject)
		{
			if (trackedObject == null){
				return;
			}
			trackedObjectDict[joint] = trackedObject;	
		}

        void Update()
		{
            // initialize tracked objects from tracking manager when ready
            if (trackingIKTargetManager.IsReady() && handLeftTrackedObject == null)
            {
                handLeftTrackedObject = trackingIKTargetManager.GetTrackedObject(TrackingIKTargetManager.TRACKING_TARGET.HAND_LEFT);
                handRightTrackedObject = trackingIKTargetManager.GetTrackedObject(TrackingIKTargetManager.TRACKING_TARGET.HAND_RIGHT);
                footLeftTrackedObject = trackingIKTargetManager.GetTrackedObject(TrackingIKTargetManager.TRACKING_TARGET.FOOT_LEFT);
                footRightTrackedObject = trackingIKTargetManager.GetTrackedObject(TrackingIKTargetManager.TRACKING_TARGET.FOOT_RIGHT);
            }

			//update tracked objects in library in case they have changed
			SetTrackedObjectForJoint(TrackedJoint.HandLeft, handLeftTrackedObject);
			SetTrackedObjectForJoint(TrackedJoint.HandRight, handRightTrackedObject);
			SetTrackedObjectForJoint(TrackedJoint.FootLeft, footLeftTrackedObject);
			SetTrackedObjectForJoint(TrackedJoint.FootRight, footRightTrackedObject);


			foreach (Rumble rumble in rumbleList)
			{
				//send out pulse
				if (!rumble.sentOut){
					//SteamVR_Controller.Input(rumble.trackedObjectIndex).TriggerHapticPulse((ushort)rumble.pulseLength);
					StartCoroutine(LongVibration(rumble));
				}
				//update timer
				rumble.timeTillNextPulse -= Time.deltaTime;
			}

		}
	}
}
