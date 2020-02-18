using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LatencyHandler : MonoBehaviour {
	public float bufferTime = 2;
	public float latency_ms = 0;

	float bufferSize = 0;
	Queue<Dictionary<HumanBodyBones, Quaternion>> iKDataBuffer = new Queue<Dictionary<HumanBodyBones, Quaternion>>();
	ConfigJointManager jointManager;
	AvatarManager avatarManager;
	// Use this for initialization
	void Start () {
		bufferSize = Physics.defaultSolverIterations * bufferTime;

		jointManager = GetComponent<ConfigJointManager>();
		avatarManager = GetComponent<AvatarManager>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		Dictionary<HumanBodyBones, Quaternion> newIKData = GetIKBufferData();

		iKDataBuffer.Enqueue(newIKData);

		//Caps the amount of data that can be buffered
		if(iKDataBuffer.Count == bufferSize)
		{
			iKDataBuffer.Dequeue();
		}
		StartCoroutine(WaitUntilLatencyTimePassed());
	}

	Dictionary<HumanBodyBones, Quaternion> GetIKBufferData()
	{
		Dictionary<HumanBodyBones, Quaternion> bufferData = new Dictionary<HumanBodyBones, Quaternion>();
		Dictionary<HumanBodyBones, GameObject> userAvatarData = avatarManager.GetGameObjectPerBoneLocalAvatarDictionary();
		
		foreach(HumanBodyBones bone in userAvatarData.Keys)
		{
			Quaternion rotationAtBone = userAvatarData[bone].gameObject.transform.localRotation;
			bufferData.Add(bone, new Quaternion(rotationAtBone.x, rotationAtBone.y, rotationAtBone.z, rotationAtBone.w));
		}

		return bufferData;
	}

	IEnumerator WaitUntilLatencyTimePassed()
	{
		yield return new WaitForSeconds(latency_ms / 1000f);

		if(iKDataBuffer.Count != 0)
		{
			DelayedJointsUpdate(iKDataBuffer.Dequeue());
		}
	}

	void DelayedJointsUpdate(Dictionary<HumanBodyBones, Quaternion> delayedData)
	{
		foreach(HumanBodyBones bone in delayedData.Keys)
		{
			jointManager.SetTagetRotation(bone, delayedData[bone]);
		}
	}

}
