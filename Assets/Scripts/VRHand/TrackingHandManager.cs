using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class TrackingHandManager : MonoBehaviour {

    [SerializeField] private TrackingIKTargetManager trackingIKTargetManager;
    [SerializeField] private SteamVR_Behaviour_Skeleton leftHand;
    [SerializeField] private SteamVR_Behaviour_Skeleton rightHand;

    private Dictionary<HumanBodyBones, Transform> trackingTargets = new Dictionary<HumanBodyBones, Transform>();

    // Use this for initialization
    void Start()
    {
        if (DetermineController.Instance.UseKnucklesControllers())
        {
            InitializeMapBones2TransformsSteamVRKnucklesConntrollers();
        }
    }

    // Update is called once per frame
    void Update() {
    }

    public Transform GetRemotePoseTarget(HumanBodyBones bone)
    {
        if (this.trackingTargets.ContainsKey(bone)) {
            return this.trackingTargets[bone];
        }
        else
        {
            return null;
        }
    }

    private void InitializeMapBones2TransformsSteamVRKnucklesConntrollers()
    {
        // left hand
        trackingTargets.Add(HumanBodyBones.LeftThumbProximal, leftHand.thumbProximal);
        trackingTargets.Add(HumanBodyBones.LeftThumbIntermediate, leftHand.thumbMiddle);
        trackingTargets.Add(HumanBodyBones.LeftThumbDistal, leftHand.thumbDistal);

        trackingTargets.Add(HumanBodyBones.LeftIndexProximal, leftHand.indexProximal);
        trackingTargets.Add(HumanBodyBones.LeftIndexIntermediate, leftHand.indexMiddle);
        trackingTargets.Add(HumanBodyBones.LeftIndexDistal, leftHand.indexDistal);

        trackingTargets.Add(HumanBodyBones.LeftMiddleProximal, leftHand.middleProximal);
        trackingTargets.Add(HumanBodyBones.LeftMiddleIntermediate, leftHand.middleMiddle);
        trackingTargets.Add(HumanBodyBones.LeftMiddleDistal, leftHand.middleDistal);

        trackingTargets.Add(HumanBodyBones.LeftRingProximal, leftHand.ringProximal);
        trackingTargets.Add(HumanBodyBones.LeftRingIntermediate, leftHand.ringMiddle);
        trackingTargets.Add(HumanBodyBones.LeftRingDistal, leftHand.ringDistal);

        trackingTargets.Add(HumanBodyBones.LeftLittleProximal, leftHand.pinkyProximal);
        trackingTargets.Add(HumanBodyBones.LeftLittleIntermediate, leftHand.pinkyMiddle);
        trackingTargets.Add(HumanBodyBones.LeftLittleDistal, leftHand.pinkyDistal);

        // right hand
        trackingTargets.Add(HumanBodyBones.RightThumbProximal, rightHand.thumbProximal);
        trackingTargets.Add(HumanBodyBones.RightThumbIntermediate, rightHand.thumbMiddle);
        trackingTargets.Add(HumanBodyBones.RightThumbDistal, rightHand.thumbDistal);

        trackingTargets.Add(HumanBodyBones.RightIndexProximal, rightHand.indexProximal);
        trackingTargets.Add(HumanBodyBones.RightIndexIntermediate, rightHand.indexMiddle);
        trackingTargets.Add(HumanBodyBones.RightIndexDistal, rightHand.indexDistal);

        trackingTargets.Add(HumanBodyBones.RightMiddleProximal, rightHand.middleProximal);
        trackingTargets.Add(HumanBodyBones.RightMiddleIntermediate, rightHand.middleMiddle);
        trackingTargets.Add(HumanBodyBones.RightMiddleDistal, rightHand.middleDistal);

        trackingTargets.Add(HumanBodyBones.RightRingProximal, rightHand.ringProximal);
        trackingTargets.Add(HumanBodyBones.RightRingIntermediate, rightHand.ringMiddle);
        trackingTargets.Add(HumanBodyBones.RightRingDistal, rightHand.ringDistal);

        trackingTargets.Add(HumanBodyBones.RightLittleProximal, rightHand.pinkyProximal);
        trackingTargets.Add(HumanBodyBones.RightLittleIntermediate, rightHand.pinkyMiddle);
        trackingTargets.Add(HumanBodyBones.RightLittleDistal, rightHand.pinkyDistal);
    }
}
