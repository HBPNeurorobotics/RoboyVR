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

    [SerializeField] private Pose targetOffsetLeftHand = new Pose(new Vector3(-0.05f, 0f, -0.15f), Quaternion.Euler(0f, 0f, 90f));
    [SerializeField] private Pose targetOffsetRightHand = new Pose(new Vector3(0.05f, 0f, -0.15f), Quaternion.Euler(0f, 0f, -90f));

    private Dictionary<uint, TrackingReferenceObject> trackingReferences = new Dictionary<uint, TrackingReferenceObject>();
    private Transform trackingTargetHead;
    private Transform trackingTargetBody;
    private Transform trackingTargetHandLeft;
    private Transform trackingTargetHandRight;
    private Transform trackingTargetFootLeft;
    private Transform trackingTargetFootRight;

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
    private Transform tThumb1;
    private Transform tThumb2;
    private Transform tThumb3;
    private Transform tThumb4;

    private Transform tIndex1;
    private Transform tIndex2;
    private Transform tIndex3;
    private Transform tIndex4;

    private Transform tMiddle1;
    private Transform tMiddle2;
    private Transform tMiddle3;
    private Transform tMiddle4;

    private Transform tRing1;
    private Transform tRing2;
    private Transform tRing3;
    private Transform tRing4;

    private Transform tPinky1;
    private Transform tPinky2;
    private Transform tPinky3;
    private Transform tPinky4;

    private Transform tThumb1R;
    private Transform tThumb2R;
    private Transform tThumb3R;
    private Transform tThumb4R;

    private Transform tIndex1R;
    private Transform tIndex2R;
    private Transform tIndex3R;
    private Transform tIndex4R;

    private Transform tMiddle1R;
    private Transform tMiddle2R;
    private Transform tMiddle3R;
    private Transform tMiddle4R;

    private Transform tRing1R;
    private Transform tRing2R;
    private Transform tRing3R;
    private Transform tRing4R;

    private Transform tPinky1R;
    private Transform tPinky2R;
    private Transform tPinky3R;
    private Transform tPinky4R;

    private Transform trackingTargetThumb1;
    private Transform trackingTargetThumb2;
    private Transform trackingTargetThumb3;
    private Transform trackingTargetThumb4;
    private Transform trackingTargetThumb1R;
    private Transform trackingTargetThumb2R;
    private Transform trackingTargetThumb3R;
    private Transform trackingTargetThumb4R;

    private Transform trackingTargetIndex1;
    private Transform trackingTargetIndex2;
    private Transform trackingTargetIndex3;
    private Transform trackingTargetIndex4;
    private Transform trackingTargetIndex1R;
    private Transform trackingTargetIndex2R;
    private Transform trackingTargetIndex3R;
    private Transform trackingTargetIndex4R;

    private Transform trackingTargetMiddle1;
    private Transform trackingTargetMiddle2;
    private Transform trackingTargetMiddle3;
    private Transform trackingTargetMiddle4;
    private Transform trackingTargetMiddle1R;
    private Transform trackingTargetMiddle2R;
    private Transform trackingTargetMiddle3R;
    private Transform trackingTargetMiddle4R;

    private Transform trackingTargetRing1;
    private Transform trackingTargetRing2;
    private Transform trackingTargetRing3;
    private Transform trackingTargetRing4;
    private Transform trackingTargetRing1R;
    private Transform trackingTargetRing2R;
    private Transform trackingTargetRing3R;
    private Transform trackingTargetRing4R;

    private Transform trackingTargetPinky1;
    private Transform trackingTargetPinky2;
    private Transform trackingTargetPinky3;
    private Transform trackingTargetPinky4;
    private Transform trackingTargetPinky1R;
    private Transform trackingTargetPinky2R;
    private Transform trackingTargetPinky3R;
    private Transform trackingTargetPinky4R;

    private GameObject TargetThumb1;
    private GameObject TargetThumb2;
    private GameObject TargetThumb3;
    private GameObject TargetThumb4;
    private GameObject TargetThumb1R;
    private GameObject TargetThumb2R;
    private GameObject TargetThumb3R;
    private GameObject TargetThumb4R;

    private GameObject TargetIndex1;
    private GameObject TargetIndex2;
    private GameObject TargetIndex3;
    private GameObject TargetIndex4;
    private GameObject TargetIndex1R;
    private GameObject TargetIndex2R;
    private GameObject TargetIndex3R;
    private GameObject TargetIndex4R;

    private GameObject TargetMiddle1;
    private GameObject TargetMiddle2;
    private GameObject TargetMiddle3;
    private GameObject TargetMiddle4;
    private GameObject TargetMiddle1R;
    private GameObject TargetMiddle2R;
    private GameObject TargetMiddle3R;
    private GameObject TargetMiddle4R;

    private GameObject TargetRing1;
    private GameObject TargetRing2;
    private GameObject TargetRing3;
    private GameObject TargetRing4;
    private GameObject TargetRing1R;
    private GameObject TargetRing2R;
    private GameObject TargetRing3R;
    private GameObject TargetRing4R;

    private GameObject TargetPinky1;
    private GameObject TargetPinky2;
    private GameObject TargetPinky3;
    private GameObject TargetPinky4;
    private GameObject TargetPinky1R;
    private GameObject TargetPinky2R;
    private GameObject TargetPinky3R;
    private GameObject TargetPinky4R;

    private bool initialized = false;

    // Use this for initialization
    void Start ()
    {
        tThumb1R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_thumb_0_r").transform;
        tThumb2R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_thumb_0_r/finger_thumb_1_r").transform;
        tThumb3R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_thumb_0_r/finger_thumb_1_r/finger_thumb_2_r").transform;
        tThumb4R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_thumb_0_r/finger_thumb_1_r/finger_thumb_2_r/finger_thumb_r_end").transform;

        tThumb1 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_thumb_0_r").transform;
        tThumb2 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_thumb_0_r/finger_thumb_1_r").transform;
        tThumb3 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_thumb_0_r/finger_thumb_1_r/finger_thumb_2_r").transform;
        tThumb4 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_thumb_0_r/finger_thumb_1_r/finger_thumb_2_r/finger_thumb_r_end").transform;

        tIndex1R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_index_meta_r/finger_index_0_r").transform;
        tIndex2R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_index_meta_r/finger_index_0_r/finger_index_1_r").transform;
        tIndex3R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_index_meta_r/finger_index_0_r/finger_index_1_r/finger_index_2_r").transform;
        tIndex4R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_index_meta_r/finger_index_0_r/finger_index_1_r/finger_index_2_r/finger_index_r_end").transform;

        tIndex1 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_index_meta_r/finger_index_0_r").transform;
        tIndex2 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_index_meta_r/finger_index_0_r/finger_index_1_r").transform;
        tIndex3 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_index_meta_r/finger_index_0_r/finger_index_1_r/finger_index_2_r").transform;
        tIndex4 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_index_meta_r/finger_index_0_r/finger_index_1_r/finger_index_2_r/finger_index_r_end").transform;

        tMiddle1R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_middle_meta_r/finger_middle_0_r").transform;
        tMiddle2R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_middle_meta_r/finger_middle_0_r/finger_middle_1_r").transform;
        tMiddle3R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_middle_meta_r/finger_middle_0_r/finger_middle_1_r/finger_middle_2_r").transform;
        tMiddle4R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_middle_meta_r/finger_middle_0_r/finger_middle_1_r/finger_middle_2_r/finger_middle_r_end").transform;

        tMiddle1 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_middle_meta_r/finger_middle_0_r").transform;
        tMiddle2 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_middle_meta_r/finger_middle_0_r/finger_middle_1_r").transform;
        tMiddle3 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_middle_meta_r/finger_middle_0_r/finger_middle_1_r/finger_middle_2_r").transform;
        tMiddle4 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_middle_meta_r/finger_middle_0_r/finger_middle_1_r/finger_middle_2_r/finger_middle_r_end").transform;

        tRing1R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_ring_meta_r/finger_ring_0_r").transform;
        tRing2R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_ring_meta_r/finger_ring_0_r/finger_ring_1_r").transform;
        tRing3R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_ring_meta_r/finger_ring_0_r/finger_ring_1_r/finger_ring_2_r").transform;
        tRing4R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_ring_meta_r/finger_ring_0_r/finger_ring_1_r/finger_ring_2_r/finger_ring_r_end").transform;

        tRing1 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_ring_meta_r/finger_ring_0_r").transform;
        tRing2 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_ring_meta_r/finger_ring_0_r/finger_ring_1_r").transform;
        tRing3 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_ring_meta_r/finger_ring_0_r/finger_ring_1_r/finger_ring_2_r").transform;
        tRing4 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_ring_meta_r/finger_ring_0_r/finger_ring_1_r/finger_ring_2_r/finger_ring_r_end").transform;

        tPinky1R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_pinky_meta_r/finger_pinky_0_r").transform;
        tPinky2R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_pinky_meta_r/finger_pinky_0_r/finger_pinky_1_r").transform;
        tPinky3R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_pinky_meta_r/finger_pinky_0_r/finger_pinky_1_r/finger_pinky_2_r").transform;
        tPinky4R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_pinky_meta_r/finger_pinky_0_r/finger_pinky_1_r/finger_pinky_2_r/finger_pinky_r_end").transform;

        tPinky1 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_pinky_meta_r/finger_pinky_0_r").transform;
        tPinky2 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_pinky_meta_r/finger_pinky_0_r/finger_pinky_1_r").transform;
        tPinky3 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_pinky_meta_r/finger_pinky_0_r/finger_pinky_1_r/finger_pinky_2_r").transform;
        tPinky4 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_pinky_meta_r/finger_pinky_0_r/finger_pinky_1_r/finger_pinky_2_r/finger_pinky_r_end").transform;
    }
	
	// Update is called once per frame
	void Update () {
        if (initialized)
        {
            TargetThumb1.transform.SetPositionAndRotation(tThumb1.transform.position, tThumb1.transform.rotation);
            TargetThumb2.transform.SetPositionAndRotation(tThumb2.transform.position, tThumb2.transform.rotation);
            TargetThumb3.transform.SetPositionAndRotation(tThumb3.transform.position, tThumb3.transform.rotation);
            TargetThumb4.transform.SetPositionAndRotation(tThumb4.transform.position, tThumb4.transform.rotation);

            TargetIndex1.transform.SetPositionAndRotation(tIndex1.transform.position, tIndex1.transform.rotation);
            TargetIndex2.transform.SetPositionAndRotation(tIndex2.transform.position, tIndex2.transform.rotation);
            TargetIndex3.transform.SetPositionAndRotation(tIndex3.transform.position, tIndex3.transform.rotation);
            TargetIndex4.transform.SetPositionAndRotation(tIndex4.transform.position, tIndex4.transform.rotation);

            TargetMiddle1.transform.SetPositionAndRotation(tMiddle1.transform.position, tMiddle1.transform.rotation);
            TargetMiddle2.transform.SetPositionAndRotation(tMiddle2.transform.position, tMiddle2.transform.rotation);
            TargetMiddle3.transform.SetPositionAndRotation(tMiddle3.transform.position, tMiddle3.transform.rotation);
            TargetMiddle4.transform.SetPositionAndRotation(tMiddle4.transform.position, tMiddle4.transform.rotation);

            TargetRing1.transform.SetPositionAndRotation(tRing1.transform.position, tRing1.transform.rotation);
            TargetRing2.transform.SetPositionAndRotation(tRing2.transform.position, tRing2.transform.rotation);
            TargetRing3.transform.SetPositionAndRotation(tRing3.transform.position, tRing3.transform.rotation);
            TargetRing4.transform.SetPositionAndRotation(tRing4.transform.position, tRing4.transform.rotation);

            TargetPinky1.transform.SetPositionAndRotation(tPinky1.transform.position, tPinky1.transform.rotation);
            TargetPinky2.transform.SetPositionAndRotation(tPinky2.transform.position, tPinky2.transform.rotation);
            TargetPinky3.transform.SetPositionAndRotation(tPinky3.transform.position, tPinky3.transform.rotation);
            TargetPinky4.transform.SetPositionAndRotation(tPinky4.transform.position, tPinky4.transform.rotation);

            TargetThumb1R.transform.SetPositionAndRotation(tThumb1R.transform.position, tThumb1R.transform.rotation);
            TargetThumb2R.transform.SetPositionAndRotation(tThumb2R.transform.position, tThumb2R.transform.rotation);
            TargetThumb3R.transform.SetPositionAndRotation(tThumb3R.transform.position, tThumb3R.transform.rotation);
            TargetThumb4R.transform.SetPositionAndRotation(tThumb4R.transform.position, tThumb4R.transform.rotation);

            TargetIndex1R.transform.SetPositionAndRotation(tIndex1R.transform.position, tIndex1R.transform.rotation);
            TargetIndex2R.transform.SetPositionAndRotation(tIndex2R.transform.position, tIndex2R.transform.rotation);
            TargetIndex3R.transform.SetPositionAndRotation(tIndex3R.transform.position, tIndex3R.transform.rotation);
            TargetIndex4R.transform.SetPositionAndRotation(tIndex4R.transform.position, tIndex4R.transform.rotation);

            TargetMiddle1R.transform.SetPositionAndRotation(tMiddle1R.transform.position, tMiddle1R.transform.rotation);
            TargetMiddle2R.transform.SetPositionAndRotation(tMiddle2R.transform.position, tMiddle2R.transform.rotation);
            TargetMiddle3R.transform.SetPositionAndRotation(tMiddle3R.transform.position, tMiddle3R.transform.rotation);
            TargetMiddle4R.transform.SetPositionAndRotation(tMiddle4R.transform.position, tMiddle4R.transform.rotation);

            TargetRing1R.transform.SetPositionAndRotation(tRing1R.transform.position, tRing1R.transform.rotation);
            TargetRing2R.transform.SetPositionAndRotation(tRing2R.transform.position, tRing2R.transform.rotation);
            TargetRing3R.transform.SetPositionAndRotation(tRing3R.transform.position, tRing3R.transform.rotation);
            TargetRing4R.transform.SetPositionAndRotation(tRing4R.transform.position, tRing4R.transform.rotation);

            TargetPinky1R.transform.SetPositionAndRotation(tPinky1R.transform.position, tPinky1R.transform.rotation);
            TargetPinky2R.transform.SetPositionAndRotation(tPinky2R.transform.position, tPinky2R.transform.rotation);
            TargetPinky3R.transform.SetPositionAndRotation(tPinky3R.transform.position, tPinky3R.transform.rotation);
            TargetPinky4R.transform.SetPositionAndRotation(tPinky4R.transform.position, tPinky4R.transform.rotation);
        }
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

    public void OnControllerGripPress()
    {
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
            }

            if (trackingReference.trackedDeviceClass == ETrackedDeviceClass.Controller)
            {
                if (OpenVR.System.GetControllerRoleForTrackedDeviceIndex(deviceIndex) == ETrackedControllerRole.LeftHand)
                {
                    trackingTargetHandLeft = trackingReference.gameObject.transform;
                    // Bachelor Thesis VRHand
                    
                    trackingTargetThumb1 = trackingReference.gameObject.transform;
                    trackingTargetThumb2 = trackingReference.gameObject.transform;
                    trackingTargetThumb3 = trackingReference.gameObject.transform;
                    trackingTargetThumb4 = trackingReference.gameObject.transform;
                    trackingTargetIndex1 = trackingReference.gameObject.transform;
                    trackingTargetIndex2 = trackingReference.gameObject.transform;
                    trackingTargetIndex3 = trackingReference.gameObject.transform;
                    trackingTargetIndex4 = trackingReference.gameObject.transform;
                    trackingTargetMiddle1 = trackingReference.gameObject.transform;
                    trackingTargetMiddle2 = trackingReference.gameObject.transform;
                    trackingTargetMiddle3 = trackingReference.gameObject.transform;
                    trackingTargetMiddle4 = trackingReference.gameObject.transform;
                    trackingTargetRing1 = trackingReference.gameObject.transform;
                    trackingTargetRing2 = trackingReference.gameObject.transform;
                    trackingTargetRing3 = trackingReference.gameObject.transform;
                    trackingTargetRing4 = trackingReference.gameObject.transform;
                    trackingTargetPinky1 = trackingReference.gameObject.transform;
                    trackingTargetPinky2 = trackingReference.gameObject.transform;
                    trackingTargetPinky3 = trackingReference.gameObject.transform;
                    trackingTargetPinky4 = trackingReference.gameObject.transform;
                }
                else if (OpenVR.System.GetControllerRoleForTrackedDeviceIndex(deviceIndex) == ETrackedControllerRole.RightHand)
                {
                    trackingTargetHandRight = trackingReference.gameObject.transform;

                    trackingTargetThumb1R = trackingReference.gameObject.transform;
                    trackingTargetThumb2R = trackingReference.gameObject.transform;
                    trackingTargetThumb3R = trackingReference.gameObject.transform;
                    trackingTargetThumb4R = trackingReference.gameObject.transform;
                    trackingTargetIndex1R = trackingReference.gameObject.transform;
                    trackingTargetIndex2R = trackingReference.gameObject.transform;
                    trackingTargetIndex3R = trackingReference.gameObject.transform;
                    trackingTargetIndex4R = trackingReference.gameObject.transform;
                    trackingTargetMiddle1R = trackingReference.gameObject.transform;
                    trackingTargetMiddle2R = trackingReference.gameObject.transform;
                    trackingTargetMiddle3R = trackingReference.gameObject.transform;
                    trackingTargetMiddle4R = trackingReference.gameObject.transform;
                    trackingTargetRing1R = trackingReference.gameObject.transform;
                    trackingTargetRing2R = trackingReference.gameObject.transform;
                    trackingTargetRing3R = trackingReference.gameObject.transform;
                    trackingTargetRing4R = trackingReference.gameObject.transform;
                    trackingTargetPinky1R = trackingReference.gameObject.transform;
                    trackingTargetPinky2R = trackingReference.gameObject.transform;
                    trackingTargetPinky3R = trackingReference.gameObject.transform;
                    trackingTargetPinky4R = trackingReference.gameObject.transform;
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
            }
            else
            {
                trackingTargetFootRight = trackerB.gameObject.transform;
                trackingTargetFootLeft = trackerA.gameObject.transform;
            }
        }
    }

    private void SetupIKTargets()
    {
        if (trackingTargetHead) SetupIKTargetHead(trackingTargetHead);
        if (trackingTargetHead) SetupIKTargetLookAt(trackingTargetHead);
        if (trackingTargetBody) SetupIKTargetBody(trackingTargetBody);
        if (trackingTargetHandLeft) SetupIKTargetHandLeft(trackingTargetHandLeft);

        if (trackingTargetThumb1) SetupTargetThumb1(trackingTargetThumb1);
        if (trackingTargetThumb2) SetupTargetThumb2(trackingTargetThumb2);
        if (trackingTargetThumb3) SetupTargetThumb3(trackingTargetThumb3);
        if (trackingTargetThumb4) SetupTargetThumb4(trackingTargetThumb4);
        if (trackingTargetIndex1) SetupTargetIndex1(trackingTargetIndex1);
        if (trackingTargetIndex2) SetupTargetIndex2(trackingTargetIndex2);
        if (trackingTargetIndex3) SetupTargetIndex3(trackingTargetIndex3);
        if (trackingTargetIndex4) SetupTargetIndex4(trackingTargetIndex4);
        if (trackingTargetMiddle1) SetupTargetMiddle1(trackingTargetMiddle1);
        if (trackingTargetMiddle2) SetupTargetMiddle2(trackingTargetMiddle2);
        if (trackingTargetMiddle3) SetupTargetMiddle3(trackingTargetMiddle3);
        if (trackingTargetMiddle4) SetupTargetMiddle4(trackingTargetMiddle4);
        if (trackingTargetRing1) SetupTargetRing1(trackingTargetRing1);
        if (trackingTargetRing2) SetupTargetRing2(trackingTargetRing2);
        if (trackingTargetRing3) SetupTargetRing3(trackingTargetRing3);
        if (trackingTargetRing4) SetupTargetRing4(trackingTargetRing4);
        if (trackingTargetPinky1) SetupTargetPinky1(trackingTargetPinky1);
        if (trackingTargetPinky2) SetupTargetPinky2(trackingTargetPinky2);
        if (trackingTargetPinky3) SetupTargetPinky3(trackingTargetPinky3);
        if (trackingTargetPinky4) SetupTargetPinky4(trackingTargetPinky4);

        if (trackingTargetThumb1R) SetupTargetThumb1R(trackingTargetThumb1R);
        if (trackingTargetThumb2R) SetupTargetThumb2R(trackingTargetThumb2R);
        if (trackingTargetThumb3R) SetupTargetThumb3R(trackingTargetThumb3R);
        if (trackingTargetThumb4R) SetupTargetThumb4R(trackingTargetThumb4R);
        if (trackingTargetIndex1R) SetupTargetIndex1R(trackingTargetIndex1R);
        if (trackingTargetIndex2R) SetupTargetIndex2R(trackingTargetIndex2R);
        if (trackingTargetIndex3R) SetupTargetIndex3R(trackingTargetIndex3R);
        if (trackingTargetIndex4R) SetupTargetIndex4R(trackingTargetIndex4R);
        if (trackingTargetMiddle1R) SetupTargetMiddle1R(trackingTargetMiddle1R);
        if (trackingTargetMiddle2R) SetupTargetMiddle2R(trackingTargetMiddle2R);
        if (trackingTargetMiddle3R) SetupTargetMiddle3R(trackingTargetMiddle3R);
        if (trackingTargetMiddle4R) SetupTargetMiddle4R(trackingTargetMiddle4R);
        if (trackingTargetRing1R) SetupTargetRing1R(trackingTargetRing1R);
        if (trackingTargetRing2R) SetupTargetRing2R(trackingTargetRing2R);
        if (trackingTargetRing3R) SetupTargetRing3R(trackingTargetRing3R);
        if (trackingTargetRing4R) SetupTargetRing4R(trackingTargetRing4R);
        if (trackingTargetPinky1R) SetupTargetPinky1R(trackingTargetPinky1R);
        if (trackingTargetPinky2R) SetupTargetPinky2R(trackingTargetPinky2R);
        if (trackingTargetPinky3R) SetupTargetPinky3R(trackingTargetPinky3R);
        if (trackingTargetPinky4R) SetupTargetPinky4R(trackingTargetPinky4R);

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

    private void SetupTargetThumb1 (Transform trackingTarget)
    {
        TargetThumb1 = new GameObject("Target Thumb1");
        TargetThumb1.transform.parent = trackingTarget;
        TargetThumb1.transform.localPosition = trackingTarget.position;
        TargetThumb1.transform.localRotation = trackingTarget.rotation;

        TargetThumb1.transform.SetPositionAndRotation(tThumb1.transform.position, tThumb1.transform.rotation);
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

    private void SetupTargetThumb1R(Transform trackingTarget)
    {
        TargetThumb1R = new GameObject("Target Thumb1");
        TargetThumb1R.transform.parent = trackingTarget;
        TargetThumb1R.transform.localPosition = trackingTarget.position;
        TargetThumb1R.transform.localRotation = trackingTarget.rotation;

        TargetThumb1R.transform.SetPositionAndRotation(tThumb1R.transform.position, tThumb1R.transform.rotation);
    }

    private void SetupTargetThumb2R(Transform trackingTarget)
    {
        TargetThumb2R = new GameObject("Target Thumb2");
        TargetThumb2R.transform.parent = trackingTarget;
        TargetThumb2R.transform.localPosition = trackingTarget.position;
        TargetThumb2R.transform.localRotation = trackingTarget.rotation;

        TargetThumb2R.transform.SetPositionAndRotation(tThumb2R.transform.position, tThumb2R.transform.rotation);
    }

    private void SetupTargetThumb3R(Transform trackingTarget)
    {
        TargetThumb3R = new GameObject("Target Thumb3");
        TargetThumb3R.transform.parent = trackingTarget;
        TargetThumb3R.transform.localPosition = trackingTarget.position;
        TargetThumb3R.transform.localRotation = trackingTarget.rotation;

        TargetThumb3R.transform.SetPositionAndRotation(tThumb3R.transform.position, tThumb3R.transform.rotation);
    }

    private void SetupTargetThumb4R(Transform trackingTarget)
    {
        TargetThumb4R = new GameObject("Target Thumb4");
        TargetThumb4R.transform.parent = trackingTarget;
        TargetThumb4R.transform.localPosition = trackingTarget.position;
        TargetThumb4R.transform.localRotation = trackingTarget.rotation;

        TargetThumb4R.transform.SetPositionAndRotation(tThumb4R.transform.position, tThumb4R.transform.rotation);
    }

    private void SetupTargetIndex1R(Transform tt1)
    {
        TargetIndex1R = new GameObject("Target Index1");
        TargetIndex1R.transform.parent = tt1;
        TargetIndex1R.transform.localPosition = tt1.position;
        TargetIndex1R.transform.localRotation = tt1.rotation;

        TargetIndex1R.transform.SetPositionAndRotation(tIndex1R.transform.position, tIndex1R.transform.rotation);
    }

    private void SetupTargetIndex2R(Transform tt2)
    {
        TargetIndex2R = new GameObject("Target Index2");
        TargetIndex2R.transform.parent = tt2;
        TargetIndex2R.transform.localPosition = tt2.position;
        TargetIndex2R.transform.localRotation = tt2.rotation;

        TargetIndex2R.transform.SetPositionAndRotation(tIndex2R.transform.position, tIndex2R.transform.rotation);
    }

    private void SetupTargetIndex3R(Transform tt3)
    {
        TargetIndex3R = new GameObject("Target Index3");
        TargetIndex3R.transform.parent = tt3;
        TargetIndex3R.transform.localPosition = tt3.position;
        TargetIndex3R.transform.localRotation = tt3.rotation;

        TargetIndex3R.transform.SetPositionAndRotation(tIndex3R.transform.position, tIndex3R.transform.rotation);
    }

    private void SetupTargetIndex4R(Transform tt4)
    {
        TargetIndex4R = new GameObject("Target Index4");
        TargetIndex4R.transform.parent = tt4;
        TargetIndex4R.transform.localPosition = tt4.position;
        TargetIndex4R.transform.localRotation = tt4.rotation;

        TargetIndex4R.transform.SetPositionAndRotation(tIndex4R.transform.position, tIndex4R.transform.rotation);
    }

    private void SetupTargetMiddle1R(Transform trackingTarget)
    {
        TargetMiddle1R = new GameObject("Target Middle1");
        TargetMiddle1R.transform.parent = trackingTarget;
        TargetMiddle1R.transform.localPosition = trackingTarget.position;
        TargetMiddle1R.transform.localRotation = trackingTarget.rotation;

        TargetMiddle1R.transform.SetPositionAndRotation(tMiddle1R.transform.position, tMiddle1R.transform.rotation);
    }

    private void SetupTargetMiddle2R(Transform trackingTarget)
    {
        TargetMiddle2R = new GameObject("Target Middle2");
        TargetMiddle2R.transform.parent = trackingTarget;
        TargetMiddle2R.transform.localPosition = trackingTarget.position;
        TargetMiddle2R.transform.localRotation = trackingTarget.rotation;

        TargetMiddle2R.transform.SetPositionAndRotation(tMiddle2R.transform.position, tMiddle2R.transform.rotation);
    }

    private void SetupTargetMiddle3R(Transform trackingTarget)
    {
        TargetMiddle3R = new GameObject("Target Middle3");
        TargetMiddle3R.transform.parent = trackingTarget;
        TargetMiddle3R.transform.localPosition = trackingTarget.position;
        TargetMiddle3R.transform.localRotation = trackingTarget.rotation;

        TargetMiddle3R.transform.SetPositionAndRotation(tMiddle3R.transform.position, tMiddle3R.transform.rotation);
    }

    private void SetupTargetMiddle4R(Transform trackingTarget)
    {
        TargetMiddle4R = new GameObject("Target Middle4");
        TargetMiddle4R.transform.parent = trackingTarget;
        TargetMiddle4R.transform.localPosition = trackingTarget.position;
        TargetMiddle4R.transform.localRotation = trackingTarget.rotation;

        TargetMiddle4R.transform.SetPositionAndRotation(tMiddle4R.transform.position, tMiddle4R.transform.rotation);
    }

    private void SetupTargetRing1R(Transform trackingTarget)
    {
        TargetRing1R = new GameObject("Target Ring1");
        TargetRing1R.transform.parent = trackingTarget;
        TargetRing1R.transform.localPosition = trackingTarget.position;
        TargetRing1R.transform.localRotation = trackingTarget.rotation;

        TargetRing1R.transform.SetPositionAndRotation(tRing1R.transform.position, tRing1R.transform.rotation);
    }

    private void SetupTargetRing2R(Transform trackingTarget)
    {
        TargetRing2R = new GameObject("Target Ring2");
        TargetRing2R.transform.parent = trackingTarget;
        TargetRing2R.transform.localPosition = trackingTarget.position;
        TargetRing2R.transform.localRotation = trackingTarget.rotation;

        TargetRing2R.transform.SetPositionAndRotation(tRing2R.transform.position, tRing2R.transform.rotation);
    }

    private void SetupTargetRing3R(Transform trackingTarget)
    {
        TargetRing3R = new GameObject("Target Ring3");
        TargetRing3R.transform.parent = trackingTarget;
        TargetRing3R.transform.localPosition = trackingTarget.position;
        TargetRing3R.transform.localRotation = trackingTarget.rotation;

        TargetRing3R.transform.SetPositionAndRotation(tRing3R.transform.position, tRing3R.transform.rotation);
    }

    private void SetupTargetRing4R(Transform trackingTarget)
    {
        TargetRing4R = new GameObject("Target Ring4");
        TargetRing4R.transform.parent = trackingTarget;
        TargetRing4R.transform.localPosition = trackingTarget.position;
        TargetRing4R.transform.localRotation = trackingTarget.rotation;

        TargetRing4R.transform.SetPositionAndRotation(tRing4R.transform.position, tRing4R.transform.rotation);
    }

    private void SetupTargetPinky1R(Transform trackingTarget)
    {
        TargetPinky1R = new GameObject("Target Pinky1");
        TargetPinky1R.transform.parent = trackingTarget;
        TargetPinky1R.transform.localPosition = trackingTarget.position;
        TargetPinky1R.transform.localRotation = trackingTarget.rotation;

        TargetPinky1R.transform.SetPositionAndRotation(tPinky1R.transform.position, tPinky1R.transform.rotation);
    }

    private void SetupTargetPinky2R(Transform trackingTarget)
    {
        TargetPinky2R = new GameObject("Target Pinky2");
        TargetPinky2R.transform.parent = trackingTarget;
        TargetPinky2R.transform.localPosition = trackingTarget.position;
        TargetPinky2R.transform.localRotation = trackingTarget.rotation;

        TargetPinky2R.transform.SetPositionAndRotation(tPinky2R.transform.position, tPinky2R.transform.rotation);
    }

    private void SetupTargetPinky3R(Transform trackingTarget)
    {
        TargetPinky3R = new GameObject("Target Pinky3");
        TargetPinky3R.transform.parent = trackingTarget;
        TargetPinky3R.transform.localPosition = trackingTarget.position;
        TargetPinky3R.transform.localRotation = trackingTarget.rotation;

        TargetPinky3R.transform.SetPositionAndRotation(tPinky3R.transform.position, tPinky3R.transform.rotation);
    }

    private void SetupTargetPinky4R(Transform trackingTarget)
    {
        TargetPinky4R = new GameObject("Target Pinky4");
        TargetPinky4R.transform.parent = trackingTarget;
        TargetPinky4R.transform.localPosition = trackingTarget.position;
        TargetPinky4R.transform.localRotation = trackingTarget.rotation;

        TargetPinky4R.transform.SetPositionAndRotation(tPinky4R.transform.position, tPinky4R.transform.rotation);
    }

    private void SetupIKTargetFootLeft(Transform trackingTarget)
    {
        ikTargetLeftFoot = new GameObject("IK Target Left Foot");
        ikTargetLeftFoot.transform.parent = trackingTarget;

        // rotate upright
        ikTargetLeftFoot.transform.rotation = Quaternion.FromToRotation(trackingTarget.up, Vector3.up) * trackingTarget.rotation;
        // assume standing on ground when setting up IK targets, then translate IK target down towards the ground
        ikTargetLeftFoot.transform.position = new Vector3(trackingTarget.position.x, 0.05f, trackingTarget.position.z);
        //ikTargetLeftFoot.transform.localPosition = new Vector3(0f, -trackingTarget.position.y, 0f);
    }

    private void SetupIKTargetFootRight(Transform trackingTarget)
    {
        ikTargetRightFoot = new GameObject("IK Target Right Foot");
        ikTargetRightFoot.transform.parent = trackingTarget;

        // rotate upright
        ikTargetRightFoot.transform.rotation = Quaternion.FromToRotation(trackingTarget.up, Vector3.up) * trackingTarget.rotation;
        // assume standing on ground when setting up IK targets, then translate IK target down towards the ground
        ikTargetRightFoot.transform.position = new Vector3(trackingTarget.position.x, 0.05f, trackingTarget.position.z);
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

    #region IK_TARGET_GETTERS

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


    // Bachelor Thesis VRHand
    public Transform GetTargetThumb1()
    {
        return TargetThumb1.transform;
    }

    public Transform GetTargetThumb2()
    {
        return TargetThumb2.transform;
    }

    public Transform GetTargetThumb3()
    {
        return TargetThumb3.transform;
    }

    public Transform GetTargetThumb4()
    {
        return TargetThumb4.transform;
    }

    public Transform GetTargetIndex1()
    {
        return TargetIndex1.transform;
    }

    public Transform GetTargetIndex2()
    {
        return TargetIndex2.transform;
    }

    public Transform GetTargetIndex3()
    {
        return TargetIndex3.transform;
    }

    public Transform GetTargetIndex4()
    {
        return TargetIndex4.transform;
    }

    public Transform GetTargetMiddle1()
    {
        return TargetMiddle1.transform;
    }

    public Transform GetTargetMiddle2()
    {
        return TargetMiddle2.transform;
    }

    public Transform GetTargetMiddle3()
    {
        return TargetMiddle3.transform;
    }

    public Transform GetTargetMiddle4()
    {
        return TargetMiddle4.transform;
    }

    public Transform GetTargetRing1()
    {
        return TargetRing1.transform;
    }

    public Transform GetTargetRing2()
    {
        return TargetRing2.transform;
    }

    public Transform GetTargetRing3()
    {
        return TargetRing3.transform;
    }

    public Transform GetTargetRing4()
    {
        return TargetRing4.transform;
    }

    public Transform GetTargetPinky1()
    {
        return TargetPinky1.transform;
    }

    public Transform GetTargetPinky2()
    {
        return TargetPinky2.transform;
    }

    public Transform GetTargetPinky3()
    {
        return TargetPinky3.transform;
    }

    public Transform GetTargetPinky4()
    {
        return TargetPinky4.transform;
    }

    public Transform GetIKTargetRightHand()
    {
        return ikTargetRightHand.transform;
    }

    public Transform GetTargetThumb1R()
    {
        return TargetThumb1R.transform;
    }

    public Transform GetTargetThumb2R()
    {
        return TargetThumb2R.transform;
    }

    public Transform GetTargetThumb3R()
    {
        return TargetThumb3R.transform;
    }

    public Transform GetTargetThumb4R()
    {
        return TargetThumb4R.transform;
    }

    public Transform GetTargetIndex1R()
    {
        return TargetIndex1R.transform;
    }

    public Transform GetTargetIndex2R()
    {
        return TargetIndex2R.transform;
    }

    public Transform GetTargetIndex3R()
    {
        return TargetIndex3R.transform;
    }

    public Transform GetTargetIndex4R()
    {
        return TargetIndex4R.transform;
    }

    public Transform GetTargetMiddle1R()
    {
        return TargetMiddle1R.transform;
    }

    public Transform GetTargetMiddle2R()
    {
        return TargetMiddle2R.transform;
    }

    public Transform GetTargetMiddle3R()
    {
        return TargetMiddle3R.transform;
    }

    public Transform GetTargetMiddle4R()
    {
        return TargetMiddle4R.transform;
    }

    public Transform GetTargetRing1R()
    {
        return TargetRing1R.transform;
    }

    public Transform GetTargetRing2R()
    {
        return TargetRing2R.transform;
    }

    public Transform GetTargetRing3R()
    {
        return TargetRing3R.transform;
    }

    public Transform GetTargetRing4R()
    {
        return TargetRing4R.transform;
    }

    public Transform GetTargetPinky1R()
    {
        return TargetPinky1R.transform;
    }

    public Transform GetTargetPinky2R()
    {
        return TargetPinky2R.transform;
    }

    public Transform GetTargetPinky3R()
    {
        return TargetPinky3R.transform;
    }

    public Transform GetTargetPinky4R()
    {
        return TargetPinky4R.transform;
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
