using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class IKTargetManager : MonoBehaviour
{
    private class TrackingReferenceObject
    {
        public ETrackedDeviceClass trackedDeviceClass;
        public GameObject gameObject;
        public SteamVR_RenderModel renderModel;
        public SteamVR_TrackedObject trackedObject;
    }

    private Dictionary<uint, TrackingReferenceObject> trackingReferences = new Dictionary<uint, TrackingReferenceObject>();
    private Transform trackingHead;
    private Transform trackingBody;
    private Transform trackingHandLeft;
    private Transform trackingHandRight;
    private Transform trackingFootLeft;
    private Transform trackingFootRight;

    private GameObject ikTargetHead;
    private GameObject ikTargetBody;
    private GameObject ikTargetLeftHand;
    private GameObject ikTargetRightHand;
    private GameObject ikTargetLeftFoot;
    private GameObject ikTargetRightFoot;

    [SerializeField]
    private Pose targetOffsetLeftHand = new Pose(new Vector3(-0.05f, 0f, -0.15f), Quaternion.Euler(0f, 0f, 90f) );
    [SerializeField]
    private Pose targetOffsetRightHand = new Pose(new Vector3(0.05f, 0f, -0.15f), Quaternion.Euler(0f, 0f, -90f));

    // Use this for initialization
    void Start ()
    {
    }
	
	// Update is called once per frame
	void Update () {
	}


    private void OnEnable()
    {
        SteamVR_Events.NewPoses.AddListener(OnNewPoses);
    }

    private void OnDisable()
    {
        SteamVR_Events.NewPoses.RemoveListener(OnNewPoses);
    }

    private void OnNewPoses(TrackedDevicePose_t[] poses)
    {
        if (poses == null)
            return;

        for (uint deviceIndex = 0; deviceIndex < poses.Length; deviceIndex++)
        {
            if (trackingReferences.ContainsKey(deviceIndex) == false)
            {
                ETrackedDeviceClass deviceClass = OpenVR.System.GetTrackedDeviceClass(deviceIndex);

                if (deviceClass == ETrackedDeviceClass.HMD || deviceClass == ETrackedDeviceClass.Controller || deviceClass == ETrackedDeviceClass.GenericTracker)
                {
                    TrackingReferenceObject trackingReference = new TrackingReferenceObject();
                    trackingReference.trackedDeviceClass = deviceClass;
                    trackingReference.gameObject = new GameObject("Tracking Reference " + deviceIndex.ToString());
                    trackingReference.gameObject.transform.parent = this.transform;
                    trackingReference.trackedObject = trackingReference.gameObject.AddComponent<SteamVR_TrackedObject>();
                    trackingReference.trackedObject.index = (SteamVR_TrackedObject.EIndex) deviceIndex;
                    /*trackingReference.renderModel = trackingReference.gameObject.AddComponent<SteamVR_RenderModel>();
                    trackingReference.renderModel.createComponents = false;
                    trackingReference.renderModel.updateDynamically = false;*/

                    trackingReferences.Add(deviceIndex, trackingReference);

                    trackingReference.gameObject.SendMessage("SetDeviceIndex", (int)deviceIndex, SendMessageOptions.DontRequireReceiver);
                    

                }
            }
        }
    }

    public void OnControllerGripPress()
    {
        Debug.Log("OnControllerGripPress");
        IdentifyTrackingTargets();
        SetupIKTargets();
    }

    private void IdentifyTrackingTargets()
    {
        List<TrackingReferenceObject> genericTrackersFeet = new List<TrackingReferenceObject>();

        foreach (KeyValuePair<uint, TrackingReferenceObject> entry in trackingReferences)
        {
            uint deviceIndex = entry.Key;
            TrackingReferenceObject trackingReference = entry.Value;

            if (trackingReference.trackedDeviceClass == ETrackedDeviceClass.HMD)
            {
                trackingHead = trackingReference.gameObject.transform;
            }

            if (trackingReference.trackedDeviceClass == ETrackedDeviceClass.Controller)
            {
                if (OpenVR.System.GetControllerRoleForTrackedDeviceIndex(deviceIndex) == ETrackedControllerRole.LeftHand)
                {
                    trackingHandLeft = trackingReference.gameObject.transform;
                }
                else if (OpenVR.System.GetControllerRoleForTrackedDeviceIndex(deviceIndex) == ETrackedControllerRole.RightHand)
                {
                    trackingHandRight = trackingReference.gameObject.transform;
                }
            }

            if (trackingReference.trackedDeviceClass == ETrackedDeviceClass.GenericTracker)
            {
                // figure out which generic tracker belongs to body, left and right foot
                //TODO: future API might provide better access and configurability

                // body tracker if is it at least 50cm above ground
                if (trackingReference.gameObject.transform.position.y >= 0.5f)
                {
                    trackingBody = trackingReference.gameObject.transform;
                }
                // else feet tracker
                else
                {
                    genericTrackersFeet.Add(trackingReference);

                }
            }
        }

        // identify left and right foot
        if (genericTrackersFeet.Count != 2)
        {
            Debug.LogError("Could not find proper amount of trackers for feet!");
        }
        else
        {
            Vector3 positionToRight = trackingHead.position + trackingHead.right * 10;
            TrackingReferenceObject trackerA = genericTrackersFeet[0];
            TrackingReferenceObject trackerB = genericTrackersFeet[1];
            float distanceA = Vector3.Distance(trackerA.gameObject.transform.position, positionToRight);
            float distanceB = Vector3.Distance(trackerB.gameObject.transform.position, positionToRight);

            if (distanceA < distanceB)
            {
                trackingFootRight = trackerA.gameObject.transform;
                trackingFootLeft = trackerB.gameObject.transform;
            }
            else
            {
                trackingFootRight = trackerB.gameObject.transform;
                trackingFootLeft = trackerA.gameObject.transform;
            }
        }
    }

    private void SetupIKTargets()
    {
        if (trackingHead) SetupIKTargetHead(trackingHead);
        if (trackingBody) SetupIKTargetBody(trackingBody);
        if (trackingHandLeft) SetupIKTargetHandLeft(trackingHandLeft);
        if (trackingHandRight) SetupIKTargetHandRight(trackingHandRight);
        if (trackingFootLeft) SetupIKTargetFootLeft(trackingFootLeft);
        if (trackingFootRight) SetupIKTargetFootRight(trackingFootRight);
    }

    #region IK_TARGET_SETUP
    private void SetupIKTargetHead(Transform trackingTarget)
    {
        ikTargetHead = new GameObject("IK Target Head");
        ikTargetHead.transform.parent = trackingTarget;
        ikTargetHead.transform.localRotation = new Quaternion();
        ikTargetHead.transform.localPosition = Vector3.forward;
    }

    private void SetupIKTargetBody(Transform trackingTarget)
    {
        ikTargetBody = new GameObject("IK Target Body");
        ikTargetBody.transform.parent = trackingTarget;
        //TODO: adjustments for body target?
    }

    private void SetupIKTargetHandLeft(Transform trackingTarget)
    {
        ikTargetLeftHand = new GameObject("IK Target Left Hand");
        ikTargetLeftHand.transform.parent = trackingTarget;
        ikTargetLeftHand.transform.localPosition = targetOffsetLeftHand.position;
        ikTargetLeftHand.transform.localRotation = targetOffsetLeftHand.rotation;
    }

    private void SetupIKTargetHandRight(Transform trackingTarget)
    {
        ikTargetRightHand = new GameObject("IK Target Right Hand");
        ikTargetRightHand.transform.parent = trackingTarget;
        ikTargetRightHand.transform.localPosition = targetOffsetRightHand.position;
        ikTargetRightHand.transform.localRotation = targetOffsetRightHand.rotation;
    }

    private void SetupIKTargetFootLeft(Transform trackingTarget)
    {
        ikTargetLeftFoot = new GameObject("IK Target Left Foot");
        ikTargetLeftFoot.transform.parent = trackingTarget;

        // rotate upright
        ikTargetLeftFoot.transform.localRotation = Quaternion.FromToRotation(trackingTarget.up, Vector3.up);
        // assume standing on ground when setting up IK targets, then translate IK target down towards the ground
        ikTargetLeftFoot.transform.localPosition = -trackingTarget.position.y * trackingTarget.InverseTransformVector(Vector3.up);
    }

    private void SetupIKTargetFootRight(Transform trackingTarget)
    {
        ikTargetRightFoot = new GameObject("IK Target Right Foot");
        ikTargetRightFoot.transform.parent = trackingTarget;

        // rotate upright
        ikTargetRightFoot.transform.localRotation = Quaternion.FromToRotation(trackingTarget.up, Vector3.up);
        // assume standing on ground when setting up IK targets, then translate IK target down towards the ground
        ikTargetRightFoot.transform.localPosition = -trackingTarget.position.y * trackingTarget.InverseTransformVector(Vector3.up);
    }
    #endregion IK_TARGET_SETUP

    #region IK_TARGET_GETTERS
    public Transform GetIKTargetHead()
    {
        return ikTargetHead.transform;
    }

    public Transform GetIKTargetBody()
    {
        return ikTargetBody.transform;
    }

    public Transform GetIKTargetLeftHand()
    {
        return ikTargetLeftHand.transform;
    }

    public Transform GetIKTargetRightHand()
    {
        return ikTargetRightHand.transform;
    }

    public Transform GetIKTargetLeftFoot()
    {
        return ikTargetLeftFoot.transform;
    }

    public Transform GetIKTargetRightFoot()
    {
        return ikTargetRightFoot.transform;
    }
    #endregion IK_TARGET_GETTERS
}
