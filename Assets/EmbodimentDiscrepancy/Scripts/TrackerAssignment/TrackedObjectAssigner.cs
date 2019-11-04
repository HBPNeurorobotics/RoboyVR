using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class TrackedObjectAssigner : MonoBehaviour {

	public KeyCode keyToAssignTrackers = KeyCode.A;

	public UserAvatarVisualsIKControl userAvatarVisualsIKControl;
	public EmbodimentDiscrepancy.DiscrepancyHapticHandler discrepancyHapticHandler;

	public GameObject mainCam;

	public Transform avatarLeftHandTransform;
	public Transform avatarRightHandTransform;
	public Transform avatarLeftFootTransform;
	public Transform avatarRightFootTransform;
	public Transform avatarBodyTransform;

	public List<SteamVR_TrackedObject> trackedObjects = new List<SteamVR_TrackedObject>();
	public Transform leftHandIKTargetPrefab;
	public Transform rightHandIKTargetPrefab;
	public Transform leftFootIKTargetPrefab;
	public Transform rightFootIKTargetPrefab;
	public Transform bodyIKTargetPrefab;
	public Transform lookAtIKTargetPrefab;

	public GameObject[] objectsToDisableAfterAssignment;


	SteamVR_TrackedObject GetClosestTrackedObject(Transform origin)
	{
		float minDistance = -1;
		SteamVR_TrackedObject closest = null;
		foreach (SteamVR_TrackedObject trackedObject in trackedObjects)
		{
			float distance = Vector3.Distance(origin.position, trackedObject.transform.position);
			if (distance < minDistance || minDistance == -1){
				closest = trackedObject;
				minDistance = distance;
			}
		}
		return closest;
	}

	void AssignTrackersToRoles()
	{
		SteamVR_TrackedObject closest;
		Transform IKTarget;

		closest = GetClosestTrackedObject(avatarLeftHandTransform);
		IKTarget = Instantiate(leftHandIKTargetPrefab, closest.transform);
		userAvatarVisualsIKControl.leftHandTarget = IKTarget;
		discrepancyHapticHandler.handLeftTrackedObject = closest;

		closest = GetClosestTrackedObject(avatarRightHandTransform);
		IKTarget = Instantiate(rightHandIKTargetPrefab, closest.transform);
		userAvatarVisualsIKControl.rightHandTarget = IKTarget;
		discrepancyHapticHandler.handRightTrackedObject = closest;

		closest = GetClosestTrackedObject(avatarLeftFootTransform);
		IKTarget = Instantiate(leftFootIKTargetPrefab, closest.transform);
		userAvatarVisualsIKControl.leftFootTarget = IKTarget;
		discrepancyHapticHandler.footLeftTrackedObject = closest;

		closest = GetClosestTrackedObject(avatarRightFootTransform);
		IKTarget = Instantiate(rightFootIKTargetPrefab, closest.transform);
		userAvatarVisualsIKControl.rightFootTarget = IKTarget;
		discrepancyHapticHandler.footRightTrackedObject = closest;

		closest = GetClosestTrackedObject(avatarBodyTransform);
		IKTarget = Instantiate(bodyIKTargetPrefab, closest.transform);
		userAvatarVisualsIKControl.bodyTarget = IKTarget;
		
		IKTarget = Instantiate(lookAtIKTargetPrefab, mainCam.transform);
		userAvatarVisualsIKControl.lookAtObj = IKTarget;
	}


	// Use this for initialization
	void Start () {


	}

	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyUp(keyToAssignTrackers))
		{
			AssignTrackersToRoles();
			userAvatarVisualsIKControl.GetComponent<Animator>().enabled = true;

			foreach (GameObject obj in objectsToDisableAfterAssignment){
				obj.SetActive(false);
			}
		}
	}
}
