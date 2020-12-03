using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class TrackingIKTargetManager : MonoBehaviour
{
    public enum BODY_PART
    {
        HEAD = 0,
        VIEWING_DIRECTION,
        HIP,
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

    [Tooltip("Use mocks instead of real IK targets where necessary.")]
    [SerializeField] private bool debugUseMockIKTargets = false;

    [SerializeField] private Pose ikTargetOffsetViveControllerLeft = new Pose(new Vector3(-0.05f, 0f, -0.15f), Quaternion.Euler(0f, 0f, 90f));
    [SerializeField] private Pose ikTargetOffsetViveControllerRight = new Pose(new Vector3(0.05f, 0f, -0.15f), Quaternion.Euler(0f, 0f, -90f));
    [SerializeField] public float feetTargetOffsetAboveGround = 0.1f;
    [SerializeField] public float hmdOffsetForward = 0.1f;

    private Dictionary<uint, TrackingReferenceObject> trackingReferences = new Dictionary<uint, TrackingReferenceObject>();
    private Dictionary<uint, SteamVR_Input_Sources> dictSteamVRInputSources = new Dictionary<uint, SteamVR_Input_Sources>();
    private Dictionary<BODY_PART, Transform> trackingTargets = new Dictionary<BODY_PART, Transform>();

    /*private GameObject ikTargetHead;
    private GameObject ikTargetLookAt;
    private GameObject ikTargetBody;
    private GameObject ikTargetLeftHand;
    private GameObject ikTargetRightHand;
    private GameObject ikTargetLeftFoot;
    private GameObject ikTargetRightFoot;*/

    private Dictionary<BODY_PART, GameObject> ikTargets = new Dictionary<BODY_PART, GameObject>();

    // Bachelors Thesis VRHand
    [SerializeField] public GameObject vrGlovesWristLeft;
    [SerializeField] public GameObject vrGlovesWristRight;

    private bool initialized = false;

    // controller input
    private bool leftGripRelease = false;
    private bool rightGripRelease = false;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
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

    public void Initialize()
    {
        Debug.Log("OnControllerGripPress() - initializing ...");

        if (!UserAvatarService.Instance.use_gazebo)
        {
            UserAvatarService.Instance._avatarManager.InitializeBodyStructures();
        }

        IdentifyTrackingTargets();
        SetupIKTargets();

        if (this.debugUseMockIKTargets)
        {
            this.SetupMockIKTargets();
        }

        initialized = true;

        Debug.Log("OnControllerGripPress() - ... done");
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
            this.Initialize();
        }
    }

    private void IdentifyTrackingTargets()
    {
        Debug.Log("IdentifyTrackingTargets() - start ...");
        List<TrackingReferenceObject> genericTrackersFeet = new List<TrackingReferenceObject>();

        foreach (KeyValuePair<uint, TrackingReferenceObject> entry in trackingReferences)
        {
            uint deviceIndex = entry.Key;
            TrackingReferenceObject trackingReference = entry.Value;

            if (trackingReference.trackedDeviceClass == ETrackedDeviceClass.HMD)
            {
                //trackingTargetHead = trackingReference.gameObject.transform;
                trackingTargets.Add(BODY_PART.HEAD, trackingReference.gameObject.transform);
                dictSteamVRInputSources.Add(deviceIndex, SteamVR_Input_Sources.Head);
            }

            if (trackingReference.trackedDeviceClass == ETrackedDeviceClass.Controller)
            {
                if (OpenVR.System.GetControllerRoleForTrackedDeviceIndex(deviceIndex) == ETrackedControllerRole.LeftHand)
                {
                    //trackingTargetHandLeft = trackingReference.gameObject.transform;
                    trackingTargets.Add(BODY_PART.HAND_LEFT, trackingReference.gameObject.transform);
                    dictSteamVRInputSources.Add(deviceIndex, SteamVR_Input_Sources.LeftHand);
                }
                else if (OpenVR.System.GetControllerRoleForTrackedDeviceIndex(deviceIndex) == ETrackedControllerRole.RightHand)
                {
                    //trackingTargetHandRight = trackingReference.gameObject.transform;
                    trackingTargets.Add(BODY_PART.HAND_RIGHT, trackingReference.gameObject.transform);
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
                    //trackingTargetBody = trackingReference.gameObject.transform;
                    trackingTargets.Add(BODY_PART.HIP, trackingReference.gameObject.transform);
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
            Debug.LogWarning("Could not find proper amount of trackers for feet!");
        }
        else
        {
            Transform trackingTargetHead = trackingTargets[BODY_PART.HEAD];
            Vector3 positionToRight = trackingTargetHead.position + trackingTargetHead.right * 10;
            TrackingReferenceObject trackerA = genericTrackersFeet[0];
            TrackingReferenceObject trackerB = genericTrackersFeet[1];
            float distanceA = Vector3.Distance(trackerA.gameObject.transform.position, positionToRight);
            float distanceB = Vector3.Distance(trackerB.gameObject.transform.position, positionToRight);

            if (distanceA < distanceB)
            {
                //trackingTargetFootRight = trackerA.gameObject.transform;
                //trackingTargetFootLeft = trackerB.gameObject.transform;
                trackingTargets.Add(BODY_PART.FOOT_RIGHT, trackerA.gameObject.transform);
                trackingTargets.Add(BODY_PART.FOOT_LEFT, trackerB.gameObject.transform);

                dictSteamVRInputSources.Add((uint)trackerA.trackedObject.index, SteamVR_Input_Sources.RightFoot);
                dictSteamVRInputSources.Add((uint)trackerB.trackedObject.index, SteamVR_Input_Sources.LeftFoot);
            }
            else
            {
                //trackingTargetFootRight = trackerB.gameObject.transform;
                //trackingTargetFootLeft = trackerA.gameObject.transform;
                trackingTargets.Add(BODY_PART.FOOT_RIGHT, trackerB.gameObject.transform);
                trackingTargets.Add(BODY_PART.FOOT_LEFT, trackerA.gameObject.transform);

                dictSteamVRInputSources.Add((uint)trackerA.trackedObject.index, SteamVR_Input_Sources.LeftFoot);
                dictSteamVRInputSources.Add((uint)trackerB.trackedObject.index, SteamVR_Input_Sources.RightFoot);
            }
        }

        Debug.Log("IdentifyTrackingTargets() - ... done");
    }

    private void SetupIKTargets()
    {
        Debug.Log("SetupIKTargets() - start ...");

        if (trackingTargets.ContainsKey(BODY_PART.HEAD))
        {
            SetupIKTargetHead(trackingTargets[BODY_PART.HEAD]);
            SetupIKTargetLookAt(trackingTargets[BODY_PART.HEAD]);
        }
        if (trackingTargets.ContainsKey(BODY_PART.HIP)) SetupIKTargetHip(trackingTargets[BODY_PART.HIP]);
        if (trackingTargets.ContainsKey(BODY_PART.HAND_LEFT)) SetupIKTargetHandLeft(trackingTargets[BODY_PART.HAND_LEFT]);
        if (trackingTargets.ContainsKey(BODY_PART.HAND_RIGHT)) SetupIKTargetHandRight(trackingTargets[BODY_PART.HAND_RIGHT]);
        if (trackingTargets.ContainsKey(BODY_PART.FOOT_LEFT)) SetupIKTargetFootLeft(trackingTargets[BODY_PART.FOOT_LEFT]);
        if (trackingTargets.ContainsKey(BODY_PART.FOOT_RIGHT)) SetupIKTargetFootRight(trackingTargets[BODY_PART.FOOT_RIGHT]);

        Debug.Log("SetupIKTargets() - ... done");
    }

    private void SetupIKTargetHead(Transform trackingTarget)
    {
        GameObject ikTargetHead = new GameObject("IK Target Head");
        ikTargetHead.transform.parent = trackingTarget;
        ikTargetHead.transform.localRotation = new Quaternion();
        ikTargetHead.transform.localPosition = UserAvatarService.Instance.use_gazebo ? new Vector3() : new Vector3(0, 0, hmdOffsetForward);
        ikTargets.Add(BODY_PART.HEAD, ikTargetHead);
    }

    private void SetupIKTargetLookAt(Transform trackingTarget)
    {
        GameObject ikTargetLookAt = new GameObject("IK Target Look At");
        ikTargetLookAt.transform.parent = trackingTarget;
        ikTargetLookAt.transform.localRotation = new Quaternion();
        ikTargetLookAt.transform.localPosition = Vector3.forward;
        ikTargets.Add(BODY_PART.VIEWING_DIRECTION, ikTargetLookAt);
    }

    private void SetupIKTargetHip(Transform trackingTarget)
    {
        GameObject ikTargetHip = new GameObject("IK Target Hip");
        ikTargetHip.transform.parent = trackingTarget;
        //TODO: adjustments for body target?
        ikTargetHip.transform.rotation = Quaternion.FromToRotation(trackingTarget.up, Vector3.up) * trackingTarget.rotation; //TODO: needs to be checked again
        ikTargets.Add(BODY_PART.HIP, ikTargetHip);
    }

    // Bachelor Thesis VRHand

    private void SetupIKTargetHandLeft(Transform trackingTarget)
    {
        GameObject ikTargetLeftHand = new GameObject("IK Target Left Hand");
        ikTargetLeftHand.transform.parent = trackingTarget;

        if (DetermineController.Instance.UseKnucklesControllers())
        {
            Vector3 rot = vrGlovesWristLeft.transform.rotation.eulerAngles;
            rot = new Vector3(rot.x, rot.y, rot.z + 90);
            Vector3 pos = vrGlovesWristLeft.transform.position;
            pos = new Vector3(pos.x - 0.01f, pos.y - 0.015f, pos.z - 0.035f);
            ikTargetLeftHand.transform.SetPositionAndRotation(pos, Quaternion.Euler(rot));
        }
        else
        {
            ikTargetLeftHand.transform.localPosition = ikTargetOffsetViveControllerLeft.position;
            ikTargetLeftHand.transform.localRotation = ikTargetOffsetViveControllerLeft.rotation;
        }
        ikTargets.Add(BODY_PART.HAND_LEFT, ikTargetLeftHand);
    }

    private void SetupIKTargetHandRight(Transform trackingTarget)
    {
        GameObject ikTargetRightHand = new GameObject("IK Target Right Hand");
        ikTargetRightHand.transform.parent = trackingTarget;

        if (DetermineController.Instance.UseKnucklesControllers())
        {
            Vector3 rot = vrGlovesWristRight.transform.rotation.eulerAngles;
            rot = new Vector3(rot.x, rot.y, rot.z - 90);
            Vector3 pos = vrGlovesWristRight.transform.position;
            pos = new Vector3(pos.x, pos.y - 0.02f, pos.z - 0.035f);
            ikTargetRightHand.transform.SetPositionAndRotation(pos, Quaternion.Euler(rot));
        }
        else
        {
            ikTargetRightHand.transform.localPosition = ikTargetOffsetViveControllerRight.position;
            ikTargetRightHand.transform.localRotation = ikTargetOffsetViveControllerRight.rotation;
        }
        ikTargets.Add(BODY_PART.HAND_RIGHT, ikTargetRightHand);
    }

    private void SetupIKTargetFootLeft(Transform trackingTarget)
    {
        GameObject ikTargetLeftFoot = new GameObject("IK Target Left Foot");
        ikTargetLeftFoot.transform.parent = trackingTarget;

        // rotate upright
        ikTargetLeftFoot.transform.rotation = Quaternion.FromToRotation(trackingTarget.up, Vector3.up) * trackingTarget.rotation;
        // assume standing on ground when setting up IK targets, then translate IK target down towards the ground
        ikTargetLeftFoot.transform.position = new Vector3(trackingTarget.position.x, feetTargetOffsetAboveGround, trackingTarget.position.z);
        //ikTargetLeftFoot.transform.localPosition = new Vector3(0f, -trackingTarget.position.y, 0f);
        ikTargets.Add(BODY_PART.FOOT_LEFT, ikTargetLeftFoot);
    }

    private void SetupIKTargetFootRight(Transform trackingTarget)
    {
        GameObject ikTargetRightFoot = new GameObject("IK Target Right Foot");
        ikTargetRightFoot.transform.parent = trackingTarget;

        // rotate upright
        ikTargetRightFoot.transform.rotation = Quaternion.FromToRotation(trackingTarget.up, Vector3.up) * trackingTarget.rotation;
        // assume standing on ground when setting up IK targets, then translate IK target down towards the ground
        ikTargetRightFoot.transform.position = new Vector3(trackingTarget.position.x, feetTargetOffsetAboveGround, trackingTarget.position.z);
        //ikTargetRightFoot.transform.localPosition = new Vector3(0f, -trackingTarget.position.y, 0f);
        ikTargets.Add(BODY_PART.FOOT_RIGHT, ikTargetRightFoot);
    }

    // DEBUG 
    private void AddIndicator(Transform parent)
    {
        GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        indicator.GetComponent<Renderer>().material.color = Color.red;
        indicator.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        indicator.transform.parent = parent;
    }

    private void SetupMockIKTargets()
    {
        Debug.LogWarning("IK Target Manager - using MOCK TARGETS");

        if (!ikTargets.ContainsKey(BODY_PART.HEAD))
        {
            GameObject ikTargetHead = new GameObject("MOCK IK Target Head");
            ikTargetHead.transform.parent = this.transform;
            ikTargetHead.transform.position = new Vector3(0f, 2f, 0f);
            ikTargets.Add(BODY_PART.HEAD, ikTargetHead);
        }

        if (!ikTargets.ContainsKey(BODY_PART.VIEWING_DIRECTION))
        {
            GameObject ikTargetLookAt = new GameObject("MOCK IK Target Look At");
            ikTargetLookAt.transform.parent = this.transform;
            ikTargetLookAt.transform.position = new Vector3(0f, 2f, 0.1f);
            ikTargets.Add(BODY_PART.VIEWING_DIRECTION, ikTargetLookAt);
        }

        if (!ikTargets.ContainsKey(BODY_PART.HIP))
        {
            GameObject ikTargetHip = new GameObject("MOCK IK Target Hip");
            ikTargetHip.transform.parent = this.transform;
            ikTargets.Add(BODY_PART.HIP, ikTargetHip);
        }

        if (!ikTargets.ContainsKey(BODY_PART.HAND_LEFT))
        {
            GameObject ikTargetLeftHand = new GameObject("MOCK IK Target Left Hand");
            ikTargetLeftHand.transform.parent = this.transform;
            ikTargets.Add(BODY_PART.HAND_LEFT, ikTargetLeftHand);
        }

        if (!ikTargets.ContainsKey(BODY_PART.HAND_RIGHT))
        {
            GameObject ikTargetRightHand = new GameObject("MOCK IK Target Right Hand");
            ikTargetRightHand.transform.parent = this.transform;
            ikTargets.Add(BODY_PART.HAND_RIGHT, ikTargetRightHand);
        }


        if (!ikTargets.ContainsKey(BODY_PART.FOOT_LEFT))
        {
            GameObject ikTargetLeftFoot = new GameObject("MOCK IK Target Left Foot");
            ikTargetLeftFoot.transform.parent = this.transform;
            ikTargets.Add(BODY_PART.FOOT_LEFT, ikTargetLeftFoot);
        }

        if (!ikTargets.ContainsKey(BODY_PART.FOOT_RIGHT))
        {
            GameObject ikTargetRightFoot = new GameObject("MOCK IK Target Right Foot");
            ikTargetRightFoot.transform.parent = this.transform;
            ikTargets.Add(BODY_PART.FOOT_RIGHT, ikTargetRightFoot);
        }

        this.initialized = true;
    }

    #endregion IK_TARGET_SETUP

    #region TARGET_GETTERS

    public Transform GetTrackingTargetTransform(BODY_PART target)
    {
        if (trackingTargets.ContainsKey(target))
        {
            return trackingTargets[target];
        }
        else
        {
            return null;
        }
    }

    public SteamVR_TrackedObject GetTrackedObject(BODY_PART target)
    {
        if (this.debugUseMockIKTargets)
        {
            return null;
        }

        if (trackingTargets.ContainsKey(target))
        {
            return trackingTargets[target].GetComponent<SteamVR_TrackedObject>();
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

    public Transform GetIKTargetTransform(BODY_PART target)
    {
        if (ikTargets.ContainsKey(target))
        {
            return ikTargets[target].transform;
        }
        else
        {
            return null;
        }
    }

    #endregion TARGET_GETTERS
}