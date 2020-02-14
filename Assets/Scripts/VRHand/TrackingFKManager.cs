using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class TrackingFKManager : MonoBehaviour {

    [SerializeField] private TrackingIKTargetManager trackingIKTargetManager;

    private Transform virtualtWrist;
    private Transform virtualtThumb1;
    private Transform virtualtThumb2;
    private Transform virtualtThumb3;
    private Transform virtualtThumb4;

    private Transform virtualtIndex1;
    private Transform virtualtIndex2;
    private Transform virtualtIndex3;
    private Transform virtualtIndex4;

    private Transform virtualtMiddle1;
    private Transform virtualtMiddle2;
    private Transform virtualtMiddle3;
    private Transform virtualtMiddle4;

    private Transform virtualtRing1;
    private Transform virtualtRing2;
    private Transform virtualtRing3;
    private Transform virtualtRing4;

    private Transform virtualtPinky1;
    private Transform virtualtPinky2;
    private Transform virtualtPinky3;
    private Transform virtualtPinky4;

    private Transform virtualtThumb1R;
    private Transform virtualtThumb2R;
    private Transform virtualtThumb3R;
    private Transform virtualtThumb4R;

    private Transform virtualtIndex1R;
    private Transform virtualtIndex2R;
    private Transform virtualtIndex3R;
    private Transform virtualtIndex4R;

    private Transform virtualtMiddle1R;
    private Transform virtualtMiddle2R;
    private Transform virtualtMiddle3R;
    private Transform virtualtMiddle4R;

    private Transform virtualtRing1R;
    private Transform virtualtRing2R;
    private Transform virtualtRing3R;
    private Transform virtualtRing4R;

    private Transform virtualtPinky1R;
    private Transform virtualtPinky2R;
    private Transform virtualtPinky3R;
    private Transform virtualtPinky4R;

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

    // Use this for initialization
    void Start() {
        // in eigene methode auslagern, dann durch die children durchgehen
        virtualtWrist = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r").transform;

        virtualtThumb1R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_thumb_0_r").transform;
        virtualtThumb2R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_thumb_0_r/finger_thumb_1_r").transform;
        virtualtThumb3R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_thumb_0_r/finger_thumb_1_r/finger_thumb_2_r").transform;
        virtualtThumb4R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_thumb_0_r/finger_thumb_1_r/finger_thumb_2_r/finger_thumb_r_end").transform;

        virtualtThumb1 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_thumb_0_r").transform;
        virtualtThumb2 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_thumb_0_r/finger_thumb_1_r").transform;
        virtualtThumb3 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_thumb_0_r/finger_thumb_1_r/finger_thumb_2_r").transform;
        virtualtThumb4 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_thumb_0_r/finger_thumb_1_r/finger_thumb_2_r/finger_thumb_r_end").transform;

        virtualtIndex1R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_index_meta_r/finger_index_0_r").transform;
        virtualtIndex2R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_index_meta_r/finger_index_0_r/finger_index_1_r").transform;
        virtualtIndex3R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_index_meta_r/finger_index_0_r/finger_index_1_r/finger_index_2_r").transform;
        virtualtIndex4R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_index_meta_r/finger_index_0_r/finger_index_1_r/finger_index_2_r/finger_index_r_end").transform;

        virtualtIndex1 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_index_meta_r/finger_index_0_r").transform;
        virtualtIndex2 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_index_meta_r/finger_index_0_r/finger_index_1_r").transform;
        virtualtIndex3 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_index_meta_r/finger_index_0_r/finger_index_1_r/finger_index_2_r").transform;
        virtualtIndex4 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_index_meta_r/finger_index_0_r/finger_index_1_r/finger_index_2_r/finger_index_r_end").transform;

        virtualtMiddle1R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_middle_meta_r/finger_middle_0_r").transform;
        virtualtMiddle2R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_middle_meta_r/finger_middle_0_r/finger_middle_1_r").transform;
        virtualtMiddle3R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_middle_meta_r/finger_middle_0_r/finger_middle_1_r/finger_middle_2_r").transform;
        virtualtMiddle4R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_middle_meta_r/finger_middle_0_r/finger_middle_1_r/finger_middle_2_r/finger_middle_r_end").transform;

        virtualtMiddle1 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_middle_meta_r/finger_middle_0_r").transform;
        virtualtMiddle2 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_middle_meta_r/finger_middle_0_r/finger_middle_1_r").transform;
        virtualtMiddle3 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_middle_meta_r/finger_middle_0_r/finger_middle_1_r/finger_middle_2_r").transform;
        virtualtMiddle4 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_middle_meta_r/finger_middle_0_r/finger_middle_1_r/finger_middle_2_r/finger_middle_r_end").transform;

        virtualtRing1R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_ring_meta_r/finger_ring_0_r").transform;
        virtualtRing2R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_ring_meta_r/finger_ring_0_r/finger_ring_1_r").transform;
        virtualtRing3R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_ring_meta_r/finger_ring_0_r/finger_ring_1_r/finger_ring_2_r").transform;
        virtualtRing4R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_ring_meta_r/finger_ring_0_r/finger_ring_1_r/finger_ring_2_r/finger_ring_r_end").transform;

        virtualtRing1 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_ring_meta_r/finger_ring_0_r").transform;
        virtualtRing2 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_ring_meta_r/finger_ring_0_r/finger_ring_1_r").transform;
        virtualtRing3 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_ring_meta_r/finger_ring_0_r/finger_ring_1_r/finger_ring_2_r").transform;
        virtualtRing4 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_ring_meta_r/finger_ring_0_r/finger_ring_1_r/finger_ring_2_r/finger_ring_r_end").transform;

        virtualtPinky1R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_pinky_meta_r/finger_pinky_0_r").transform;
        virtualtPinky2R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_pinky_meta_r/finger_pinky_0_r/finger_pinky_1_r").transform;
        virtualtPinky3R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_pinky_meta_r/finger_pinky_0_r/finger_pinky_1_r/finger_pinky_2_r").transform;
        virtualtPinky4R = GameObject.Find("vr_glove_right/vr_glove_model/Root/wrist_r/finger_pinky_meta_r/finger_pinky_0_r/finger_pinky_1_r/finger_pinky_2_r/finger_pinky_r_end").transform;

        virtualtPinky1 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_pinky_meta_r/finger_pinky_0_r").transform;
        virtualtPinky2 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_pinky_meta_r/finger_pinky_0_r/finger_pinky_1_r").transform;
        virtualtPinky3 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_pinky_meta_r/finger_pinky_0_r/finger_pinky_1_r/finger_pinky_2_r").transform;
        virtualtPinky4 = GameObject.Find("vr_glove_left/vr_glove_model/Root/wrist_r/finger_pinky_meta_r/finger_pinky_0_r/finger_pinky_1_r/finger_pinky_2_r/finger_pinky_r_end").transform;

        SetupTargets();
        
    }

    // Update is called once per frame
    void Update() {
        if (trackingIKTargetManager.IsReady())
        {
            //Vector3 rot = tThumb1.transform.rotation.eulerAngles;
            //rot = new Vector3(rot.x, rot.y-90f, rot.z);
            //TargetThumb1.transform.SetPositionAndRotation(tThumb1.transform.position, Quaternion.Euler(rot));
            //TargetThumb1.transform.SetPositionAndRotation(tThumb1.transform.position, tThumb1.transform.rotation);
            //TargetThumb1.transform.position = tThumb1.transform.position;
            TargetThumb1.transform.rotation = virtualtThumb1.transform.rotation;
            //Debug.Log(TargetThumb1.transform.rotation);

            TargetThumb2.transform.SetPositionAndRotation(virtualtThumb2.transform.position, virtualtThumb2.transform.rotation);
            TargetThumb3.transform.SetPositionAndRotation(virtualtThumb3.transform.position, virtualtThumb3.transform.rotation);
            TargetThumb4.transform.SetPositionAndRotation(virtualtThumb4.transform.position, virtualtThumb4.transform.rotation);

            TargetIndex1.transform.SetPositionAndRotation(virtualtIndex1.transform.position, virtualtIndex1.transform.rotation);
            TargetIndex2.transform.SetPositionAndRotation(virtualtIndex2.transform.position, virtualtIndex2.transform.rotation);
            TargetIndex3.transform.SetPositionAndRotation(virtualtIndex3.transform.position, virtualtIndex3.transform.rotation);
            TargetIndex4.transform.SetPositionAndRotation(virtualtIndex4.transform.position, virtualtIndex4.transform.rotation);

            TargetMiddle1.transform.SetPositionAndRotation(virtualtMiddle1.transform.position, virtualtMiddle1.transform.rotation);
            TargetMiddle2.transform.SetPositionAndRotation(virtualtMiddle2.transform.position, virtualtMiddle2.transform.rotation);
            TargetMiddle3.transform.SetPositionAndRotation(virtualtMiddle3.transform.position, virtualtMiddle3.transform.rotation);
            TargetMiddle4.transform.SetPositionAndRotation(virtualtMiddle4.transform.position, virtualtMiddle4.transform.rotation);

            TargetRing1.transform.SetPositionAndRotation(virtualtRing1.transform.position, virtualtRing1.transform.rotation);
            TargetRing2.transform.SetPositionAndRotation(virtualtRing2.transform.position, virtualtRing2.transform.rotation);
            TargetRing3.transform.SetPositionAndRotation(virtualtRing3.transform.position, virtualtRing3.transform.rotation);
            TargetRing4.transform.SetPositionAndRotation(virtualtRing4.transform.position, virtualtRing4.transform.rotation);

            TargetPinky1.transform.SetPositionAndRotation(virtualtPinky1.transform.position, virtualtPinky1.transform.rotation);
            TargetPinky2.transform.SetPositionAndRotation(virtualtPinky2.transform.position, virtualtPinky2.transform.rotation);
            TargetPinky3.transform.SetPositionAndRotation(virtualtPinky3.transform.position, virtualtPinky3.transform.rotation);
            TargetPinky4.transform.SetPositionAndRotation(virtualtPinky4.transform.position, virtualtPinky4.transform.rotation);

            TargetThumb1R.transform.rotation = virtualtThumb1R.transform.rotation;
            //TargetThumb1R.transform.SetPositionAndRotation(virtualtThumb1R.transform.position, virtualtThumb1R.transform.rotation);
            TargetThumb2R.transform.SetPositionAndRotation(virtualtThumb2R.transform.position, virtualtThumb2R.transform.rotation);
            TargetThumb3R.transform.SetPositionAndRotation(virtualtThumb3R.transform.position, virtualtThumb3R.transform.rotation);
            TargetThumb4R.transform.SetPositionAndRotation(virtualtThumb4R.transform.position, virtualtThumb4R.transform.rotation);

            TargetIndex1R.transform.SetPositionAndRotation(virtualtIndex1R.transform.position, virtualtIndex1R.transform.rotation);
            TargetIndex2R.transform.SetPositionAndRotation(virtualtIndex2R.transform.position, virtualtIndex2R.transform.rotation);
            TargetIndex3R.transform.SetPositionAndRotation(virtualtIndex3R.transform.position, virtualtIndex3R.transform.rotation);
            TargetIndex4R.transform.SetPositionAndRotation(virtualtIndex4R.transform.position, virtualtIndex4R.transform.rotation);

            TargetMiddle1R.transform.SetPositionAndRotation(virtualtMiddle1R.transform.position, virtualtMiddle1R.transform.rotation);
            TargetMiddle2R.transform.SetPositionAndRotation(virtualtMiddle2R.transform.position, virtualtMiddle2R.transform.rotation);
            TargetMiddle3R.transform.SetPositionAndRotation(virtualtMiddle3R.transform.position, virtualtMiddle3R.transform.rotation);
            TargetMiddle4R.transform.SetPositionAndRotation(virtualtMiddle4R.transform.position, virtualtMiddle4R.transform.rotation);

            TargetRing1R.transform.SetPositionAndRotation(virtualtRing1R.transform.position, virtualtRing1R.transform.rotation);
            TargetRing2R.transform.SetPositionAndRotation(virtualtRing2R.transform.position, virtualtRing2R.transform.rotation);
            TargetRing3R.transform.SetPositionAndRotation(virtualtRing3R.transform.position, virtualtRing3R.transform.rotation);
            TargetRing4R.transform.SetPositionAndRotation(virtualtRing4R.transform.position, virtualtRing4R.transform.rotation);

            TargetPinky1R.transform.SetPositionAndRotation(virtualtPinky1R.transform.position, virtualtPinky1R.transform.rotation);
            TargetPinky2R.transform.SetPositionAndRotation(virtualtPinky2R.transform.position, virtualtPinky2R.transform.rotation);
            TargetPinky3R.transform.SetPositionAndRotation(virtualtPinky3R.transform.position, virtualtPinky3R.transform.rotation);
            TargetPinky4R.transform.SetPositionAndRotation(virtualtPinky4R.transform.position, virtualtPinky4R.transform.rotation);
        }
    }

    private void SetupTargets()
    {
        GameObject empty = new GameObject();

        SetupTargetThumb1(empty.transform);
        SetupTargetThumb2(empty.transform);
        SetupTargetThumb3(empty.transform);
        SetupTargetThumb4(empty.transform);
        SetupTargetIndex1(empty.transform);
        SetupTargetIndex2(empty.transform);
        SetupTargetIndex3(empty.transform);
        SetupTargetIndex4(empty.transform);
        SetupTargetMiddle1(empty.transform);
        SetupTargetMiddle2(empty.transform);
        SetupTargetMiddle3(empty.transform);
        SetupTargetMiddle4(empty.transform);
        SetupTargetRing1(empty.transform);
        SetupTargetRing2(empty.transform);
        SetupTargetRing3(empty.transform);
        SetupTargetRing4(empty.transform);
        SetupTargetPinky1(empty.transform);
        SetupTargetPinky2(empty.transform);
        SetupTargetPinky3(empty.transform);
        SetupTargetPinky4(empty.transform);

        SetupTargetThumb1R(empty.transform);
        SetupTargetThumb2R(empty.transform);
        SetupTargetThumb3R(empty.transform);
        SetupTargetThumb4R(empty.transform);
        SetupTargetIndex1R(empty.transform);
        SetupTargetIndex2R(empty.transform);
        SetupTargetIndex3R(empty.transform);
        SetupTargetIndex4R(empty.transform);
        SetupTargetMiddle1R(empty.transform);
        SetupTargetMiddle2R(empty.transform);
        SetupTargetMiddle3R(empty.transform);
        SetupTargetMiddle4R(empty.transform);
        SetupTargetRing1R(empty.transform);
        SetupTargetRing2R(empty.transform);
        SetupTargetRing3R(empty.transform);
        SetupTargetRing4R(empty.transform);
        SetupTargetPinky1R(empty.transform);
        SetupTargetPinky2R(empty.transform);
        SetupTargetPinky3R(empty.transform);
        SetupTargetPinky4R(empty.transform);
    }

    private void SetupTargetThumb1(Transform trackingTarget)
    {
        TargetThumb1 = new GameObject("Left Target Thumb1");
        TargetThumb1.transform.parent = trackingTarget;
        //Debug.Log(string.Format("Target Thumb1: {0} {1} {2}", virtualtThumb1.localPosition.x, virtualtThumb1.localPosition.y, virtualtThumb1.localPosition.z));
        TargetThumb1.transform.localPosition = virtualtThumb1.localPosition;
        //TargetThumb1.transform.localRotation = new Quaternion();
        //TargetThumb1.transform.SetPositionAndRotation(tThumb1.transform.position, tThumb1.transform.rotation);
    }

    private void SetupTargetThumb2(Transform trackingTarget)
    {
        TargetThumb2 = new GameObject("Left Target Thumb2");
        TargetThumb2.transform.parent = trackingTarget;
        TargetThumb2.transform.localPosition = trackingTarget.position;
        TargetThumb2.transform.localRotation = trackingTarget.rotation;

        TargetThumb2.transform.SetPositionAndRotation(virtualtThumb2.transform.position, virtualtThumb2.transform.rotation);
    }

    private void SetupTargetThumb3(Transform trackingTarget)
    {
        TargetThumb3 = new GameObject("Left Target Thumb3");
        TargetThumb3.transform.parent = trackingTarget;
        TargetThumb3.transform.localPosition = trackingTarget.position;
        TargetThumb3.transform.localRotation = trackingTarget.rotation;

        TargetThumb3.transform.SetPositionAndRotation(virtualtThumb3.transform.position, virtualtThumb3.transform.rotation);
    }

    private void SetupTargetThumb4(Transform trackingTarget)
    {
        TargetThumb4 = new GameObject("Left Target Thumb4");
        TargetThumb4.transform.parent = trackingTarget;
        TargetThumb4.transform.localPosition = trackingTarget.position;
        TargetThumb4.transform.localRotation = trackingTarget.rotation;

        TargetThumb4.transform.SetPositionAndRotation(virtualtThumb4.transform.position, virtualtThumb4.transform.rotation);
    }

    private void SetupTargetIndex1(Transform tt1)
    {
        TargetIndex1 = new GameObject("Left Target Index1");
        TargetIndex1.transform.parent = tt1;
        TargetIndex1.transform.localPosition = tt1.position;
        TargetIndex1.transform.localRotation = tt1.rotation;

        TargetIndex1.transform.SetPositionAndRotation(virtualtIndex1.transform.position, virtualtIndex1.transform.rotation);
    }

    private void SetupTargetIndex2(Transform tt2)
    {
        TargetIndex2 = new GameObject("Left Target Index2");
        TargetIndex2.transform.parent = tt2;
        TargetIndex2.transform.localPosition = tt2.position;
        TargetIndex2.transform.localRotation = tt2.rotation;

        TargetIndex2.transform.SetPositionAndRotation(virtualtIndex2.transform.position, virtualtIndex2.transform.rotation);
    }

    private void SetupTargetIndex3(Transform tt3)
    {
        TargetIndex3 = new GameObject("Left Target Index3");
        TargetIndex3.transform.parent = tt3;
        TargetIndex3.transform.localPosition = tt3.position;
        TargetIndex3.transform.localRotation = tt3.rotation;

        TargetIndex3.transform.SetPositionAndRotation(virtualtIndex3.transform.position, virtualtIndex3.transform.rotation);
    }

    private void SetupTargetIndex4(Transform tt4)
    {
        TargetIndex4 = new GameObject("Left Target Index4");
        TargetIndex4.transform.parent = tt4;
        TargetIndex4.transform.localPosition = tt4.position;
        TargetIndex4.transform.localRotation = tt4.rotation;

        TargetIndex4.transform.SetPositionAndRotation(virtualtIndex4.transform.position, virtualtIndex4.transform.rotation);
    }

    private void SetupTargetMiddle1(Transform trackingTarget)
    {
        TargetMiddle1 = new GameObject("Left Target Middle1");
        TargetMiddle1.transform.parent = trackingTarget;
        TargetMiddle1.transform.localPosition = trackingTarget.position;
        TargetMiddle1.transform.localRotation = trackingTarget.rotation;

        TargetMiddle1.transform.SetPositionAndRotation(virtualtMiddle1.transform.position, virtualtMiddle1.transform.rotation);
    }

    private void SetupTargetMiddle2(Transform trackingTarget)
    {
        TargetMiddle2 = new GameObject("Left Target Middle2");
        TargetMiddle2.transform.parent = trackingTarget;
        TargetMiddle2.transform.localPosition = trackingTarget.position;
        TargetMiddle2.transform.localRotation = trackingTarget.rotation;

        TargetMiddle2.transform.SetPositionAndRotation(virtualtMiddle2.transform.position, virtualtMiddle2.transform.rotation);
    }

    private void SetupTargetMiddle3(Transform trackingTarget)
    {
        TargetMiddle3 = new GameObject("Left Target Middle3");
        TargetMiddle3.transform.parent = trackingTarget;
        TargetMiddle3.transform.localPosition = trackingTarget.position;
        TargetMiddle3.transform.localRotation = trackingTarget.rotation;

        TargetMiddle3.transform.SetPositionAndRotation(virtualtMiddle3.transform.position, virtualtMiddle3.transform.rotation);
    }

    private void SetupTargetMiddle4(Transform trackingTarget)
    {
        TargetMiddle4 = new GameObject("Left Target Middle4");
        TargetMiddle4.transform.parent = trackingTarget;
        TargetMiddle4.transform.localPosition = trackingTarget.position;
        TargetMiddle4.transform.localRotation = trackingTarget.rotation;

        TargetMiddle4.transform.SetPositionAndRotation(virtualtMiddle4.transform.position, virtualtMiddle4.transform.rotation);
    }

    private void SetupTargetRing1(Transform trackingTarget)
    {
        TargetRing1 = new GameObject("Left Target Ring1");
        TargetRing1.transform.parent = trackingTarget;
        TargetRing1.transform.localPosition = trackingTarget.position;
        TargetRing1.transform.localRotation = trackingTarget.rotation;

        TargetRing1.transform.SetPositionAndRotation(virtualtRing1.transform.position, virtualtRing1.transform.rotation);
    }

    private void SetupTargetRing2(Transform trackingTarget)
    {
        TargetRing2 = new GameObject("Left Target Ring2");
        TargetRing2.transform.parent = trackingTarget;
        TargetRing2.transform.localPosition = trackingTarget.position;
        TargetRing2.transform.localRotation = trackingTarget.rotation;

        TargetRing2.transform.SetPositionAndRotation(virtualtRing2.transform.position, virtualtRing2.transform.rotation);
    }

    private void SetupTargetRing3(Transform trackingTarget)
    {
        TargetRing3 = new GameObject("Left Target Ring3");
        TargetRing3.transform.parent = trackingTarget;
        TargetRing3.transform.localPosition = trackingTarget.position;
        TargetRing3.transform.localRotation = trackingTarget.rotation;

        TargetRing3.transform.SetPositionAndRotation(virtualtRing3.transform.position, virtualtRing3.transform.rotation);
    }

    private void SetupTargetRing4(Transform trackingTarget)
    {
        TargetRing4 = new GameObject("Left Target Ring4");
        TargetRing4.transform.parent = trackingTarget;
        TargetRing4.transform.localPosition = trackingTarget.position;
        TargetRing4.transform.localRotation = trackingTarget.rotation;

        TargetRing4.transform.SetPositionAndRotation(virtualtRing4.transform.position, virtualtRing4.transform.rotation);
    }

    private void SetupTargetPinky1(Transform trackingTarget)
    {
        TargetPinky1 = new GameObject("Left Target Pinky1");
        TargetPinky1.transform.parent = trackingTarget;
        TargetPinky1.transform.localPosition = trackingTarget.position;
        TargetPinky1.transform.localRotation = trackingTarget.rotation;

        TargetPinky1.transform.SetPositionAndRotation(virtualtPinky1.transform.position, virtualtPinky1.transform.rotation);
    }

    private void SetupTargetPinky2(Transform trackingTarget)
    {
        TargetPinky2 = new GameObject("Left Target Pinky2");
        TargetPinky2.transform.parent = trackingTarget;
        TargetPinky2.transform.localPosition = trackingTarget.position;
        TargetPinky2.transform.localRotation = trackingTarget.rotation;

        TargetPinky2.transform.SetPositionAndRotation(virtualtPinky2.transform.position, virtualtPinky2.transform.rotation);
    }

    private void SetupTargetPinky3(Transform trackingTarget)
    {
        TargetPinky3 = new GameObject("Left Target Pinky3");
        TargetPinky3.transform.parent = trackingTarget;
        TargetPinky3.transform.localPosition = trackingTarget.position;
        TargetPinky3.transform.localRotation = trackingTarget.rotation;

        TargetPinky3.transform.SetPositionAndRotation(virtualtPinky3.transform.position, virtualtPinky3.transform.rotation);
    }

    private void SetupTargetPinky4(Transform trackingTarget)
    {
        TargetPinky4 = new GameObject("Left Target Pinky4");
        TargetPinky4.transform.parent = trackingTarget;
        TargetPinky4.transform.localPosition = trackingTarget.position;
        TargetPinky4.transform.localRotation = trackingTarget.rotation;

        TargetPinky4.transform.SetPositionAndRotation(virtualtPinky4.transform.position, virtualtPinky4.transform.rotation);
    }

    private void SetupTargetThumb1R(Transform trackingTarget)
    {
        TargetThumb1R = new GameObject("Right Target Thumb1");
        TargetThumb1R.transform.parent = trackingTarget;
        TargetThumb1R.transform.localPosition = virtualtThumb1R.localPosition;

        //TargetThumb1R.transform.SetPositionAndRotation(virtualtThumb1R.transform.position, virtualtThumb1R.transform.rotation);
    }

    private void SetupTargetThumb2R(Transform trackingTarget)
    {
        TargetThumb2R = new GameObject("Right Target Thumb2");
        TargetThumb2R.transform.parent = trackingTarget;
        TargetThumb2R.transform.localPosition = trackingTarget.position;
        TargetThumb2R.transform.localRotation = trackingTarget.rotation;

        TargetThumb2R.transform.SetPositionAndRotation(virtualtThumb2R.transform.position, virtualtThumb2R.transform.rotation);
    }

    private void SetupTargetThumb3R(Transform trackingTarget)
    {
        TargetThumb3R = new GameObject("Right Target Thumb3");
        TargetThumb3R.transform.parent = trackingTarget;
        TargetThumb3R.transform.localPosition = trackingTarget.position;
        TargetThumb3R.transform.localRotation = trackingTarget.rotation;

        TargetThumb3R.transform.SetPositionAndRotation(virtualtThumb3R.transform.position, virtualtThumb3R.transform.rotation);
    }

    private void SetupTargetThumb4R(Transform trackingTarget)
    {
        TargetThumb4R = new GameObject("Right Target Thumb4");
        TargetThumb4R.transform.parent = trackingTarget;
        TargetThumb4R.transform.localPosition = trackingTarget.position;
        TargetThumb4R.transform.localRotation = trackingTarget.rotation;

        TargetThumb4R.transform.SetPositionAndRotation(virtualtThumb4R.transform.position, virtualtThumb4R.transform.rotation);
    }

    private void SetupTargetIndex1R(Transform tt1)
    {
        TargetIndex1R = new GameObject("Right Target Index1");
        TargetIndex1R.transform.parent = tt1;
        TargetIndex1R.transform.localPosition = tt1.position;
        TargetIndex1R.transform.localRotation = tt1.rotation;

        TargetIndex1R.transform.SetPositionAndRotation(virtualtIndex1R.transform.position, virtualtIndex1R.transform.rotation);
    }

    private void SetupTargetIndex2R(Transform tt2)
    {
        TargetIndex2R = new GameObject("Right Target Index2");
        TargetIndex2R.transform.parent = tt2;
        TargetIndex2R.transform.localPosition = tt2.position;
        TargetIndex2R.transform.localRotation = tt2.rotation;

        TargetIndex2R.transform.SetPositionAndRotation(virtualtIndex2R.transform.position, virtualtIndex2R.transform.rotation);
    }

    private void SetupTargetIndex3R(Transform tt3)
    {
        TargetIndex3R = new GameObject("Right Target Index3");
        TargetIndex3R.transform.parent = tt3;
        TargetIndex3R.transform.localPosition = tt3.position;
        TargetIndex3R.transform.localRotation = tt3.rotation;

        TargetIndex3R.transform.SetPositionAndRotation(virtualtIndex3R.transform.position, virtualtIndex3R.transform.rotation);
    }

    private void SetupTargetIndex4R(Transform tt4)
    {
        TargetIndex4R = new GameObject("Right Target Index4");
        TargetIndex4R.transform.parent = tt4;
        TargetIndex4R.transform.localPosition = tt4.position;
        TargetIndex4R.transform.localRotation = tt4.rotation;

        TargetIndex4R.transform.SetPositionAndRotation(virtualtIndex4R.transform.position, virtualtIndex4R.transform.rotation);
    }

    private void SetupTargetMiddle1R(Transform trackingTarget)
    {
        TargetMiddle1R = new GameObject("Right Target Middle1");
        TargetMiddle1R.transform.parent = trackingTarget;
        TargetMiddle1R.transform.localPosition = trackingTarget.position;
        TargetMiddle1R.transform.localRotation = trackingTarget.rotation;

        TargetMiddle1R.transform.SetPositionAndRotation(virtualtMiddle1R.transform.position, virtualtMiddle1R.transform.rotation);
    }

    private void SetupTargetMiddle2R(Transform trackingTarget)
    {
        TargetMiddle2R = new GameObject("Right Target Middle2");
        TargetMiddle2R.transform.parent = trackingTarget;
        TargetMiddle2R.transform.localPosition = trackingTarget.position;
        TargetMiddle2R.transform.localRotation = trackingTarget.rotation;

        TargetMiddle2R.transform.SetPositionAndRotation(virtualtMiddle2R.transform.position, virtualtMiddle2R.transform.rotation);
    }

    private void SetupTargetMiddle3R(Transform trackingTarget)
    {
        TargetMiddle3R = new GameObject("Right Target Middle3");
        TargetMiddle3R.transform.parent = trackingTarget;
        TargetMiddle3R.transform.localPosition = trackingTarget.position;
        TargetMiddle3R.transform.localRotation = trackingTarget.rotation;

        TargetMiddle3R.transform.SetPositionAndRotation(virtualtMiddle3R.transform.position, virtualtMiddle3R.transform.rotation);
    }

    private void SetupTargetMiddle4R(Transform trackingTarget)
    {
        TargetMiddle4R = new GameObject("Right Target Middle4");
        TargetMiddle4R.transform.parent = trackingTarget;
        TargetMiddle4R.transform.localPosition = trackingTarget.position;
        TargetMiddle4R.transform.localRotation = trackingTarget.rotation;

        TargetMiddle4R.transform.SetPositionAndRotation(virtualtMiddle4R.transform.position, virtualtMiddle4R.transform.rotation);
    }

    private void SetupTargetRing1R(Transform trackingTarget)
    {
        TargetRing1R = new GameObject("Right Target Ring1");
        TargetRing1R.transform.parent = trackingTarget;
        TargetRing1R.transform.localPosition = trackingTarget.position;
        TargetRing1R.transform.localRotation = trackingTarget.rotation;

        TargetRing1R.transform.SetPositionAndRotation(virtualtRing1R.transform.position, virtualtRing1R.transform.rotation);
    }

    private void SetupTargetRing2R(Transform trackingTarget)
    {
        TargetRing2R = new GameObject("Right Target Ring2");
        TargetRing2R.transform.parent = trackingTarget;
        TargetRing2R.transform.localPosition = trackingTarget.position;
        TargetRing2R.transform.localRotation = trackingTarget.rotation;

        TargetRing2R.transform.SetPositionAndRotation(virtualtRing2R.transform.position, virtualtRing2R.transform.rotation);
    }

    private void SetupTargetRing3R(Transform trackingTarget)
    {
        TargetRing3R = new GameObject("Right Target Ring3");
        TargetRing3R.transform.parent = trackingTarget;
        TargetRing3R.transform.localPosition = trackingTarget.position;
        TargetRing3R.transform.localRotation = trackingTarget.rotation;

        TargetRing3R.transform.SetPositionAndRotation(virtualtRing3R.transform.position, virtualtRing3R.transform.rotation);
    }

    private void SetupTargetRing4R(Transform trackingTarget)
    {
        TargetRing4R = new GameObject("Right Target Ring4");
        TargetRing4R.transform.parent = trackingTarget;
        TargetRing4R.transform.localPosition = trackingTarget.position;
        TargetRing4R.transform.localRotation = trackingTarget.rotation;

        TargetRing4R.transform.SetPositionAndRotation(virtualtRing4R.transform.position, virtualtRing4R.transform.rotation);
    }

    private void SetupTargetPinky1R(Transform trackingTarget)
    {
        TargetPinky1R = new GameObject("Right Target Pinky1");
        TargetPinky1R.transform.parent = trackingTarget;
        TargetPinky1R.transform.localPosition = trackingTarget.position;
        TargetPinky1R.transform.localRotation = trackingTarget.rotation;

        TargetPinky1R.transform.SetPositionAndRotation(virtualtPinky1R.transform.position, virtualtPinky1R.transform.rotation);
    }

    private void SetupTargetPinky2R(Transform trackingTarget)
    {
        TargetPinky2R = new GameObject("Right Target Pinky2");
        TargetPinky2R.transform.parent = trackingTarget;
        TargetPinky2R.transform.localPosition = trackingTarget.position;
        TargetPinky2R.transform.localRotation = trackingTarget.rotation;

        TargetPinky2R.transform.SetPositionAndRotation(virtualtPinky2R.transform.position, virtualtPinky2R.transform.rotation);
    }

    private void SetupTargetPinky3R(Transform trackingTarget)
    {
        TargetPinky3R = new GameObject("Right Target Pinky3");
        TargetPinky3R.transform.parent = trackingTarget;
        TargetPinky3R.transform.localPosition = trackingTarget.position;
        TargetPinky3R.transform.localRotation = trackingTarget.rotation;

        TargetPinky3R.transform.SetPositionAndRotation(virtualtPinky3R.transform.position, virtualtPinky3R.transform.rotation);
    }

    private void SetupTargetPinky4R(Transform trackingTarget)
    {
        TargetPinky4R = new GameObject("Right Target Pinky4");
        TargetPinky4R.transform.parent = trackingTarget;
        TargetPinky4R.transform.localPosition = trackingTarget.position;
        TargetPinky4R.transform.localRotation = trackingTarget.rotation;

        TargetPinky4R.transform.SetPositionAndRotation(virtualtPinky4R.transform.position, virtualtPinky4R.transform.rotation);
    }
    
    public Transform GetTargetThumb1()
    {
        //Debug.Log(TargetThumb1.transform.position);
        return TargetThumb1.transform;
    }
    
    /*
    public Transform GetTargetThumb1()
    {
        //Debug.Log(virtualtThumb1.position);
        return virtualtThumb1;
    }
    */
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
}
