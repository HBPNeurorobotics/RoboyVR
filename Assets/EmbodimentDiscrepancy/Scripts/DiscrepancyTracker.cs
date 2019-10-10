using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmbodimentDiscrepancy
{
	[RequireComponent(typeof(DiscrepancyHandler))]
	public class DiscrepancyTracker : MonoBehaviour
	{

		[SerializeField]
		[Tooltip("Set to tracked object of avatar")]
		Transform localHeadPos, localHandPosLeft, localHandPosRight, localFootPosLeft, localFootPosRight;

		Dictionary<TrackedJoint, Transform> localPosDict = new Dictionary<TrackedJoint, Transform>();

		Dictionary<TrackedJoint, Transform> remotePosDict = new Dictionary<TrackedJoint, Transform>();

		[SerializeField]
		[Tooltip("If not set, will be searched in scene")]
		UserAvatarService userAvatarService;

		[SerializeField]
		[Tooltip("If not set, will be searched in scene")]
		DiscrepancyHandler discrepancyHandler;

		[SerializeField]
		[Range(0, 1)]
		float toleranceDistanceGlobal = 0.1f;

		Dictionary<TrackedJoint, float> timerDict = new Dictionary<TrackedJoint, float>();


		void Start()
		{
			if (userAvatarService == null)
			{
				userAvatarService = GameObject.FindObjectOfType<UserAvatarService>();
				if (userAvatarService == null){
					Debug.LogError("no UserAvatarService found");
				}
			}

			if (discrepancyHandler == null)
			{
				discrepancyHandler = GameObject.FindObjectOfType<DiscrepancyHandler>();
				if (discrepancyHandler == null){
					Debug.LogError("no DiscrepancyHandler found");
				}
			}

			if (localHeadPos == null || localHandPosLeft == null || localHandPosRight == null || localFootPosLeft == null || localFootPosRight == null){
				Debug.LogError("local avatar joint(s) not set");
			}

			localPosDict[TrackedJoint.Head] = localHeadPos;
			localPosDict[TrackedJoint.HandLeft] = localHandPosLeft;
			localPosDict[TrackedJoint.HandRight] = localHandPosRight;
			localPosDict[TrackedJoint.FootLeft] = localFootPosLeft;
			localPosDict[TrackedJoint.FootRight] = localFootPosRight;
		}

		void Update()
		{
			if (userAvatarService.avatar != null)
			{
				//find remote joint objects
				foreach (KeyValuePair<TrackedJoint, Transform> entry in localPosDict)
				{
					string name = userAvatarService.avatar.name + "::avatar_ybot::" + localPosDict[entry.Key].name;
					TrackedJoint joint = entry.Key;
					if (!remotePosDict.ContainsKey(joint)){
						remotePosDict[joint] = GameObject.Find(name).transform;
					}
					if (remotePosDict[joint] == null){
						Debug.LogError("could not find " + name);
					}
				}

				//check & handle discrepancies
				foreach (KeyValuePair<TrackedJoint, Transform> entry in localPosDict)
				{
					TrackedJoint joint = entry.Key;
					Transform localPos = entry.Value;
					Transform remotePos = remotePosDict[joint];
					float distance = (localPos.position - remotePos.position).magnitude;
					if (distance > toleranceDistanceGlobal)
					{
						if (!timerDict.ContainsKey(joint)){
							timerDict[joint] = 0;
						}
						timerDict[joint] += Time.deltaTime;
						HandleDiscrepancy(joint, localPos, remotePos, distance - toleranceDistanceGlobal, timerDict[joint]);
					}
					else{
						timerDict[joint] = 0;
					}
				}
			}
		}

		void HandleDiscrepancy(TrackedJoint joint, Transform localPos, Transform remotePos, float distance, float time)
		{
			Discrepancy disc = new Discrepancy();
			disc.joint = joint;
			disc.trackedPos = localPos;
			disc.simulatedPos = remotePos;
			disc.distance = distance;
			disc.duration = time;
			discrepancyHandler.HandleDiscrepancy(disc);
		}
	}
}