using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class TrackingHandManager : MonoBehaviour {

    public enum HAND_SIDE
    {
        LEFT = 0,
        RIGHT
    }

    public enum FINGER_SEGMENT
    {
        THUMB_1 = 0,
        THUMB_2,
        THUMB_3,
        THUMB_4,

        INDEX_1,
        INDEX_2,
        INDEX_3,
        INDEX_4,

        MIDDLE_1,
        MIDDLE_2,
        MIDDLE_3,
        MIDDLE_4,

        RING_1,
        RING_2,
        RING_3,
        RING_4,

        PINKY_1,
        PINKY_2,
        PINKY_3,
        PINKY_4
    }


    [SerializeField] private TrackingIKTargetManager trackingIKTargetManager;

    private Dictionary<FINGER_SEGMENT, Transform> trackingTargetsHandLeft = new Dictionary<FINGER_SEGMENT, Transform>();
    private Dictionary<FINGER_SEGMENT, Transform> trackingTargetsHandRight = new Dictionary<FINGER_SEGMENT, Transform>();

    //private Dictionary<FINGER_SEGMENT, GameObject> remotePoseTargetsHandLeft = new Dictionary<FINGER_SEGMENT, GameObject>();
    //private Dictionary<FINGER_SEGMENT, GameObject> remotePoseTargetsHandRight = new Dictionary<FINGER_SEGMENT, GameObject>();

    private static string HAND_MODEL_HIERARCHY_WRIST = "vr_glove_model/Root/wrist_r";
    private static string[] THUMB_HIERARCHY = new string[] { "finger_thumb_0_r", "finger_thumb_1_r", "finger_thumb_2_r", "finger_thumb_r_end" };
    private static string[] INDEX_HIERARCHY = new string[] { "finger_index_meta_r/finger_index_0_r", "finger_index_1_r", "finger_index_2_r", "finger_index_r_end" };
    private static string[] MIDDLE_HIERARCHY = new string[] { "finger_middle_meta_r/finger_middle_0_r", "finger_middle_1_r", "finger_middle_2_r", "finger_middle_r_end" };
    private static string[] RING_HIERARCHY = new string[] { "finger_ring_meta_r/finger_ring_0_r", "finger_ring_1_r", "finger_ring_2_r", "finger_ring_r_end" };
    private static string[] PINKY_HIERARCHY = new string[] { "finger_pinky_meta_r/finger_pinky_0_r", "finger_pinky_1_r", "finger_pinky_2_r", "finger_pinky_r_end" };

    private static string[][] FINGER_HIERARCHIES = new string[][] { THUMB_HIERARCHY, INDEX_HIERARCHY, MIDDLE_HIERARCHY, RING_HIERARCHY, PINKY_HIERARCHY };

    // Use this for initialization
    void Start() {

        if(DetermineController.Instance.UseKnucklesControllers())
        {
            string prefixLeftHand = "vr_glove_left/" + HAND_MODEL_HIERARCHY_WRIST;
            string prefixRightHand = "vr_glove_right/" + HAND_MODEL_HIERARCHY_WRIST;
            FINGER_SEGMENT dictKey;

            for (int indexFinger = 0; indexFinger < FINGER_HIERARCHIES.Length; indexFinger++)
            {
                string fingerSegmentPath = "";

                for (int indexSegment = 0; indexSegment < FINGER_HIERARCHIES[indexFinger].Length; indexSegment++)
                {
                    fingerSegmentPath += "/" + FINGER_HIERARCHIES[indexFinger][indexSegment];
                    dictKey = (FINGER_SEGMENT)(indexFinger*4 + indexSegment);

                    Transform trackingTargetLeft = GameObject.Find(prefixLeftHand + fingerSegmentPath).transform;
                    this.trackingTargetsHandLeft.Add(dictKey, trackingTargetLeft);

                    Transform trackingTargetRight = GameObject.Find(prefixRightHand + fingerSegmentPath).transform;
                    this.trackingTargetsHandRight.Add(dictKey, trackingTargetRight);
                }
            }
        }
    }

    // Update is called once per frame
    void Update() {
       
    }

    public Transform GetRemotePoseTarget(HAND_SIDE side, FINGER_SEGMENT segment)
    {
        if (side == HAND_SIDE.LEFT)
        {
            return this.trackingTargetsHandLeft[segment].transform;
        }
        else
        {
            return this.trackingTargetsHandRight[segment].transform;
        }
    }
}
