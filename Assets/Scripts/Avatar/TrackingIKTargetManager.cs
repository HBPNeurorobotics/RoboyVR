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

    // Bachelors Thesis VRHand
    [SerializeField] public GameObject vrHand;
    [SerializeField] public GameObject vrHandRight;

    private bool initialized = false;

    // controller input
    private bool leftGripRelease = false;
    private bool rightGripRelease = false;

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

    public bool IsReady()
    {
        return initialized;
    }

    #region IK_TARGET_SETUP

    public void OnControllerGripPress(SteamVR_Behaviour_Boolean fromBehaviour, SteamVR_Input_Sources fromSource, System.Boolean state)
    {
        //Debug.Log("OnControllerGripPress");
        if (fromSource == SteamVR_Input_Sources.LeftHand)
        {
            this.leftGripRelease = state;
        }
        if (fromSource == SteamVR_Input_Sources.RightHand)
        {
            this.rightGripRelease = state;
        }
        //Debug.Log(fromSource);
        //Debug.Log(state);

        if (this.leftGripRelease && this.rightGripRelease && !initialized)
        {
            Debug.Log("OnControllerGripPress() - initializing");

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
    /*
    private void SetupIKTargetHandLeft(Transform trackingTarget)
    {
        ikTargetLeftHand = new GameObject("IK Target Left Hand");
        ikTargetLeftHand.transform.parent = trackingTarget;
        ikTargetLeftHand.transform.localPosition = targetOffsetLeftHand.position;
        ikTargetLeftHand.transform.localRotation = targetOffsetLeftHand.rotation;
    }
    */

    // Bachelor Thesis VRHand
    
    private void SetupIKTargetHandLeft(Transform trackingTarget)
    {
        ikTargetLeftHand = new GameObject("IK Target Left Hand");
        ikTargetLeftHand.transform.parent = trackingTarget;
        ikTargetLeftHand.transform.localPosition = targetOffsetLeftHand.position;
        ikTargetLeftHand.transform.localRotation = targetOffsetLeftHand.rotation;
        Vector3 rot = vrHand.transform.rotation.eulerAngles;
        rot = new Vector3(rot.x, rot.y, rot.z + 90);
        Vector3 pos = vrHand.transform.position;
        pos = new Vector3(pos.x - 0.01f, pos.y - 0.015f, pos.z - 0.035f);
        ikTargetLeftHand.transform.SetPositionAndRotation(pos, Quaternion.Euler(rot));
    }
    
    /*
    // TODO: in den legacy code und in den getter das trackingTarget ausgeben
    private void SetupTargetThumb1 (Transform trackingTarget)
    {
        TargetThumb1 = new GameObject("Target Thumb1");
        TargetThumb1.transform.parent = trackingTarget;
        Debug.Log(string.Format("Target Thumb1: {0} {1} {2}", tThumb1.localPosition.x, tThumb1.localPosition.y, tThumb1.localPosition.z));
        TargetThumb1.transform.localPosition = tThumb1.localPosition;
        //TargetThumb1.transform.localRotation = new Quaternion();

        //TargetThumb1.transform.SetPositionAndRotation(tThumb1.transform.position, tThumb1.transform.rotation);
    }

    private void SetupTargetThumb2(Transform trackingTarget)
    {
        TargetThumb2 = new GameObject("Target Thumb2");
        TargetThumb2.transform.parent = trackingTarget;
        TargetThumb2.transform.localPosition = trackingTarget.position;
        TargetThumb2.transform.localRotation = trackingTarget.rotation;
        
        TargetThumb2.transform.SetPositionAndRotation(tThumb2.transform.position, tThumb2.transform.rotation);
    }

    private void SetupTargetThumb3(Transform trackingTarget)
    {
        TargetThumb3 = new GameObject("Target Thumb3");
        TargetThumb3.transform.parent = trackingTarget;
        TargetThumb3.transform.localPosition = trackingTarget.position;
        TargetThumb3.transform.localRotation = trackingTarget.rotation;

        TargetThumb3.transform.SetPositionAndRotation(tThumb3.transform.position, tThumb3.transform.rotation);
    }

    private void SetupTargetThumb4(Transform trackingTarget)
    {
        TargetThumb4 = new GameObject("Target Thumb4");
        TargetThumb4.transform.parent = trackingTarget;
        TargetThumb4.transform.localPosition = trackingTarget.position;
        TargetThumb4.transform.localRotation = trackingTarget.rotation;

        TargetThumb4.transform.SetPositionAndRotation(tThumb4.transform.position, tThumb4.transform.rotation);
    }

    private void SetupTargetIndex1(Transform tt1)
    {
        TargetIndex1 = new GameObject("Target Index1");
        TargetIndex1.transform.parent = tt1;
        TargetIndex1.transform.localPosition = tt1.position;
        TargetIndex1.transform.localRotation = tt1.rotation;

        TargetIndex1.transform.SetPositionAndRotation(tIndex1.transform.position, tIndex1.transform.rotation);
    }

    private void SetupTargetIndex2(Transform tt2)
    {
        TargetIndex2 = new GameObject("Target Index2");
        TargetIndex2.transform.parent = tt2;
        TargetIndex2.transform.localPosition = tt2.position;
        TargetIndex2.transform.localRotation = tt2.rotation;

        TargetIndex2.transform.SetPositionAndRotation(tIndex2.transform.position, tIndex2.transform.rotation);
    }

    private void SetupTargetIndex3(Transform tt3)
    {
        TargetIndex3 = new GameObject("Target Index3");
        TargetIndex3.transform.parent = tt3;
        TargetIndex3.transform.localPosition = tt3.position;
        TargetIndex3.transform.localRotation = tt3.rotation;

        TargetIndex3.transform.SetPositionAndRotation(tIndex3.transform.position, tIndex3.transform.rotation);
    }

    private void SetupTargetIndex4(Transform tt4)
    {
        TargetIndex4 = new GameObject("Target Index4");
        TargetIndex4.transform.parent = tt4;
        TargetIndex4.transform.localPosition = tt4.position;
        TargetIndex4.transform.localRotation = tt4.rotation;

        TargetIndex4.transform.SetPositionAndRotation(tIndex4.transform.position, tIndex4.transform.rotation);
    }

    private void SetupTargetMiddle1(Transform trackingTarget)
    {
        TargetMiddle1 = new GameObject("Target Middle1");
        TargetMiddle1.transform.parent = trackingTarget;
        TargetMiddle1.transform.localPosition = trackingTarget.position;
        TargetMiddle1.transform.localRotation = trackingTarget.rotation;

        TargetMiddle1.transform.SetPositionAndRotation(tMiddle1.transform.position, tMiddle1.transform.rotation);
    }

    private void SetupTargetMiddle2(Transform trackingTarget)
    {
        TargetMiddle2 = new GameObject("Target Middle2");
        TargetMiddle2.transform.parent = trackingTarget;
        TargetMiddle2.transform.localPosition = trackingTarget.position;
        TargetMiddle2.transform.localRotation = trackingTarget.rotation;

        TargetMiddle2.transform.SetPositionAndRotation(tMiddle2.transform.position, tMiddle2.transform.rotation);
    }

    private void SetupTargetMiddle3(Transform trackingTarget)
    {
        TargetMiddle3 = new GameObject("Target Middle3");
        TargetMiddle3.transform.parent = trackingTarget;
        TargetMiddle3.transform.localPosition = trackingTarget.position;
        TargetMiddle3.transform.localRotation = trackingTarget.rotation;

        TargetMiddle3.transform.SetPositionAndRotation(tMiddle3.transform.position, tMiddle3.transform.rotation);
    }

    private void SetupTargetMiddle4(Transform trackingTarget)
    {
        TargetMiddle4 = new GameObject("Target Middle4");
        TargetMiddle4.transform.parent = trackingTarget;
        TargetMiddle4.transform.localPosition = trackingTarget.position;
        TargetMiddle4.transform.localRotation = trackingTarget.rotation;

        TargetMiddle4.transform.SetPositionAndRotation(tMiddle4.transform.position, tMiddle4.transform.rotation);
    }

    private void SetupTargetRing1(Transform trackingTarget)
    {
        TargetRing1 = new GameObject("Target Ring1");
        TargetRing1.transform.parent = trackingTarget;
        TargetRing1.transform.localPosition = trackingTarget.position;
        TargetRing1.transform.localRotation = trackingTarget.rotation;

        TargetRing1.transform.SetPositionAndRotation(tRing1.transform.position, tRing1.transform.rotation);
    }

    private void SetupTargetRing2(Transform trackingTarget)
    {
        TargetRing2 = new GameObject("Target Ring2");
        TargetRing2.transform.parent = trackingTarget;
        TargetRing2.transform.localPosition = trackingTarget.position;
        TargetRing2.transform.localRotation = trackingTarget.rotation;

        TargetRing2.transform.SetPositionAndRotation(tRing2.transform.position, tRing2.transform.rotation);
    }

    private void SetupTargetRing3(Transform trackingTarget)
    {
        TargetRing3 = new GameObject("Target Ring3");
        TargetRing3.transform.parent = trackingTarget;
        TargetRing3.transform.localPosition = trackingTarget.position;
        TargetRing3.transform.localRotation = trackingTarget.rotation;

        TargetRing3.transform.SetPositionAndRotation(tRing3.transform.position, tRing3.transform.rotation);
    }

    private void SetupTargetRing4(Transform trackingTarget)
    {
        TargetRing4 = new GameObject("Target Ring4");
        TargetRing4.transform.parent = trackingTarget;
        TargetRing4.transform.localPosition = trackingTarget.position;
        TargetRing4.transform.localRotation = trackingTarget.rotation;

        TargetRing4.transform.SetPositionAndRotation(tRing4.transform.position, tRing4.transform.rotation);
    }

    private void SetupTargetPinky1(Transform trackingTarget)
    {
        TargetPinky1 = new GameObject("Target Pinky1");
        TargetPinky1.transform.parent = trackingTarget;
        TargetPinky1.transform.localPosition = trackingTarget.position;
        TargetPinky1.transform.localRotation = trackingTarget.rotation;

        TargetPinky1.transform.SetPositionAndRotation(tPinky1.transform.position, tPinky1.transform.rotation);
    }

    private void SetupTargetPinky2(Transform trackingTarget)
    {
        TargetPinky2 = new GameObject("Target Pinky2");
        TargetPinky2.transform.parent = trackingTarget;
        TargetPinky2.transform.localPosition = trackingTarget.position;
        TargetPinky2.transform.localRotation = trackingTarget.rotation;

        TargetPinky2.transform.SetPositionAndRotation(tPinky2.transform.position, tPinky2.transform.rotation);
    }

    private void SetupTargetPinky3(Transform trackingTarget)
    {
        TargetPinky3 = new GameObject("Target Pinky3");
        TargetPinky3.transform.parent = trackingTarget;
        TargetPinky3.transform.localPosition = trackingTarget.position;
        TargetPinky3.transform.localRotation = trackingTarget.rotation;

        TargetPinky3.transform.SetPositionAndRotation(tPinky3.transform.position, tPinky3.transform.rotation);
    }

    private void SetupTargetPinky4(Transform trackingTarget)
    {
        TargetPinky4 = new GameObject("Target Pinky4");
        TargetPinky4.transform.parent = trackingTarget;
        TargetPinky4.transform.localPosition = trackingTarget.position;
        TargetPinky4.transform.localRotation = trackingTarget.rotation;

        TargetPinky4.transform.SetPositionAndRotation(tPinky4.transform.position, tPinky4.transform.rotation);
    }
    */

    private void SetupIKTargetHandRight(Transform trackingTarget)
    {
        ikTargetRightHand = new GameObject("IK Target Right Hand");
        ikTargetRightHand.transform.parent = trackingTarget;
        ikTargetRightHand.transform.localPosition = targetOffsetRightHand.position;
        ikTargetRightHand.transform.localRotation = targetOffsetRightHand.rotation;
        Vector3 rot = vrHandRight.transform.rotation.eulerAngles;
        rot = new Vector3(rot.x, rot.y, rot.z - 90);
        Vector3 pos = vrHandRight.transform.position;
        pos = new Vector3(pos.x, pos.y - 0.02f, pos.z - 0.035f);
        ikTargetRightHand.transform.SetPositionAndRotation(pos, Quaternion.Euler(rot));
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
