using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class TrackingIKTargetManager : MonoBehaviour
{
    public enum TRACKING_TARGET
    {
        HEAD = 0,
        BODY,
        HAND_LEFT,
        HAND_RIGHT,
        FOOT_LEFT,
        FOOT_RIGHT
    }

    private class TrackingReferenceObject
    {
        public ETrackedDeviceClass trackedDeviceClass;
        public GameObject gameObject;
        public SteamVR_RenderModel renderModel;
        public SteamVR_TrackedObject trackedObject;
    }

    [SerializeField] private Pose targetOffsetLeftHand = new Pose(new Vector3(-0.05f, 0f, -0.15f), Quaternion.Euler(0f, 0f, 90f));
    [SerializeField] private Pose targetOffsetRightHand = new Pose(new Vector3(0.05f, 0f, -0.15f), Quaternion.Euler(0f, 0f, -90f));
    [SerializeField] public float feetTargetOffsetAboveGround = 0.1f;

    private Dictionary<uint, TrackingReferenceObject> trackingReferences = new Dictionary<uint, TrackingReferenceObject>();
    private Transform trackingTargetHead;
    private Transform trackingTargetBody;
    private Transform trackingTargetHandLeft;
    private Transform trackingTargetHandRight;
    private Transform trackingTargetFootLeft;
    private Transform trackingTargetFootRight;
    private Dictionary<uint, SteamVR_Input_Sources> dictSteamVRInputSources = new Dictionary<uint, SteamVR_Input_Sources>();

    private GameObject ikTargetHead;
    private GameObject ikTargetLookAt;
    private GameObject ikTargetBody;
    private GameObject ikTargetLeftHand;
    private GameObject ikTargetRightHand;
    private GameObject ikTargetLeftFoot;
    private GameObject ikTargetRightFoot;

    private bool initialized = false;

    // Use this for initialization
    void Start()
    {
    }
	
	// Update is called once per frame
	void Update () 
    {
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
                    trackingReference.trackedObject.index = (SteamVR_TrackedObject.EIndex)deviceIndex;
                    /*trackingReference.renderModel = trackingReference.gameObject.AddComponent<SteamVR_RenderModel>();
                    trackingReference.renderModel.createComponents = false;
                    trackingReference.renderModel.updateDynamically = false;*/

                    trackingReferences.Add(deviceIndex, trackingReference);

                    trackingReference.gameObject.SendMessage("SetDeviceIndex", (int)deviceIndex, SendMessageOptions.DontRequireReceiver);


                }
            }
        }
    }

    public bool IsReady()
    {
        return initialized;
    }

    #region IK_TARGET_SETUP

    public void OnControllerGripPress()
    {
        Debug.Log("OnControllerGripPress");
        if (!initialized)
        {
            IdentifyTrackingTargets();
            SetupIKTargets();

            initialized = true;
        }

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
                trackingTargetHead = trackingReference.gameObject.transform;
                dictSteamVRInputSources.Add(deviceIndex, SteamVR_Input_Sources.Head);
            }

            if (trackingReference.trackedDeviceClass == ETrackedDeviceClass.Controller)
            {
                if (OpenVR.System.GetControllerRoleForTrackedDeviceIndex(deviceIndex) == ETrackedControllerRole.LeftHand)
                {
                    trackingTargetHandLeft = trackingReference.gameObject.transform;
                    dictSteamVRInputSources.Add(deviceIndex, SteamVR_Input_Sources.LeftHand);
                }
                else if (OpenVR.System.GetControllerRoleForTrackedDeviceIndex(deviceIndex) == ETrackedControllerRole.RightHand)
                {
                    trackingTargetHandRight = trackingReference.gameObject.transform;
                    dictSteamVRInputSources.Add(deviceIndex, SteamVR_Input_Sources.RightHand);
                }
            }

            if (trackingReference.trackedDeviceClass == ETrackedDeviceClass.GenericTracker)
            {
                // figure out which generic tracker belongs to body, left and right foot
                //TODO: future API might provide better access and configurability

                // body tracker if is it at least 50cm above ground
                if (trackingReference.gameObject.transform.position.y >= 0.5f)
                {
                    trackingTargetBody = trackingReference.gameObject.transform;
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
            Vector3 positionToRight = trackingTargetHead.position + trackingTargetHead.right * 10;
            TrackingReferenceObject trackerA = genericTrackersFeet[0];
            TrackingReferenceObject trackerB = genericTrackersFeet[1];
            float distanceA = Vector3.Distance(trackerA.gameObject.transform.position, positionToRight);
            float distanceB = Vector3.Distance(trackerB.gameObject.transform.position, positionToRight);

            if (distanceA < distanceB)
            {
                trackingTargetFootRight = trackerA.gameObject.transform;
                trackingTargetFootLeft = trackerB.gameObject.transform;

                dictSteamVRInputSources.Add((uint)trackerA.trackedObject.index, SteamVR_Input_Sources.RightFoot);
                dictSteamVRInputSources.Add((uint)trackerB.trackedObject.index, SteamVR_Input_Sources.LeftFoot);
            }
            else
            {
                trackingTargetFootRight = trackerB.gameObject.transform;
                trackingTargetFootLeft = trackerA.gameObject.transform;

                dictSteamVRInputSources.Add((uint)trackerA.trackedObject.index, SteamVR_Input_Sources.LeftFoot);
                dictSteamVRInputSources.Add((uint)trackerB.trackedObject.index, SteamVR_Input_Sources.RightFoot);
            }
        }
    }

    private void SetupIKTargets()
    {
        if (trackingTargetHead) SetupIKTargetHead(trackingTargetHead);
        if (trackingTargetHead) SetupIKTargetLookAt(trackingTargetHead);
        if (trackingTargetBody) SetupIKTargetBody(trackingTargetBody);
        if (trackingTargetHandLeft) SetupIKTargetHandLeft(trackingTargetHandLeft);
        if (trackingTargetHandRight) SetupIKTargetHandRight(trackingTargetHandRight);
        if (trackingTargetFootLeft) SetupIKTargetFootLeft(trackingTargetFootLeft);
        if (trackingTargetFootRight) SetupIKTargetFootRight(trackingTargetFootRight);
    }

    private void SetupIKTargetHead(Transform trackingTarget)
    {
        ikTargetHead = new GameObject("IK Target Head");
        ikTargetHead.transform.parent = trackingTarget;
        ikTargetHead.transform.localRotation = new Quaternion();
        ikTargetHead.transform.localPosition = new Vector3();
    }

    private void SetupIKTargetLookAt(Transform trackingTarget)
    {
        ikTargetLookAt = new GameObject("IK Target Look At");
        ikTargetLookAt.transform.parent = trackingTarget;
        ikTargetLookAt.transform.localRotation = new Quaternion();
        ikTargetLookAt.transform.localPosition = Vector3.forward;
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
        ikTargetLeftFoot.transform.rotation = Quaternion.FromToRotation(trackingTarget.up, Vector3.up) * trackingTarget.rotation;
        // assume standing on ground when setting up IK targets, then translate IK target down towards the ground
        ikTargetLeftFoot.transform.position = new Vector3(trackingTarget.position.x, feetTargetOffsetAboveGround, trackingTarget.position.z);
        //ikTargetLeftFoot.transform.localPosition = new Vector3(0f, -trackingTarget.position.y, 0f);
    }

    private void SetupIKTargetFootRight(Transform trackingTarget)
    {
        ikTargetRightFoot = new GameObject("IK Target Right Foot");
        ikTargetRightFoot.transform.parent = trackingTarget;

        // rotate upright
        ikTargetRightFoot.transform.rotation = Quaternion.FromToRotation(trackingTarget.up, Vector3.up) * trackingTarget.rotation;
        // assume standing on ground when setting up IK targets, then translate IK target down towards the ground
        ikTargetRightFoot.transform.position = new Vector3(trackingTarget.position.x, feetTargetOffsetAboveGround, trackingTarget.position.z);
        //ikTargetRightFoot.transform.localPosition = new Vector3(0f, -trackingTarget.position.y, 0f);
    }

    // DEBUG 
    private void AddIndicator(Transform parent)
    {
        GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        indicator.GetComponent<Renderer>().material.color = Color.red;
        indicator.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        indicator.transform.parent = parent;
    }

    #endregion IK_TARGET_SETUP

    #region TARGET_GETTERS

    public Transform GetTrackingTargetTransform(TRACKING_TARGET target)
    {
        if (target == TRACKING_TARGET.HEAD)
        {
            return trackingTargetHead;
        }
        else if (target == TRACKING_TARGET.BODY)
        {
            return trackingTargetBody;
        }
        else if (target == TRACKING_TARGET.HAND_LEFT)
        {
            return trackingTargetHandLeft;
        }
        else if (target == TRACKING_TARGET.HAND_RIGHT)
        {
            return trackingTargetHandRight;
        }
        else if (target == TRACKING_TARGET.FOOT_LEFT)
        {
            return trackingTargetFootLeft;
        }
        else if (target == TRACKING_TARGET.FOOT_RIGHT)
        {
            return trackingTargetFootRight;
        }
        else
        {
            return null;
        }
    }

    public SteamVR_TrackedObject GetTrackedObject(TRACKING_TARGET target)
    {
        if (target == TRACKING_TARGET.HEAD)
        {
            return trackingTargetHead.GetComponent<SteamVR_TrackedObject>();
        }
        else if (target == TRACKING_TARGET.BODY)
        {
            return trackingTargetBody.GetComponent<SteamVR_TrackedObject>();
        }
        else if (target == TRACKING_TARGET.HAND_LEFT)
        {
            return trackingTargetHandLeft.GetComponent<SteamVR_TrackedObject>();
        }
        else if (target == TRACKING_TARGET.HAND_RIGHT)
        {
            return trackingTargetHandRight.GetComponent<SteamVR_TrackedObject>();
        }
        else if (target == TRACKING_TARGET.FOOT_LEFT)
        {
            return trackingTargetFootLeft.GetComponent<SteamVR_TrackedObject>();
        }
        else if (target == TRACKING_TARGET.FOOT_RIGHT)
        {
            return trackingTargetFootRight.GetComponent<SteamVR_TrackedObject>();
        }
        else
        {
            return null;
        }
    }

    public SteamVR_Input_Sources GetSteamVRInputSource(uint trackedObjectIndex)
    {
        return dictSteamVRInputSources[trackedObjectIndex];
    }

    public Transform GetIKTargetHead()
    {
        if (ikTargetHead != null)
        {
            return ikTargetHead.transform;
        }
        else
        {
            return null;
        }
    }

    public Transform GetIKTargetLookAt()
    {
        if (ikTargetLookAt != null)
        {
            return ikTargetLookAt.transform;
        }
        else
        {
            return null;
        }
    }

    public Transform GetIKTargetBody()
    {
        if (ikTargetBody != null)
        {
            return ikTargetBody.transform;
        }
        else
        {
            return null;
        }
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

    #endregion TARGET_GETTERS
}