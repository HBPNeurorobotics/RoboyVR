using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Applied to the avatar_rig GameObject. Allows us to efficiently track
/// all joint angles without stringly typed shenanigans.
///
/// This tracker will only work with avatar_ybot!
///
/// This thing is lazily initialized and is designed not to rely on the
/// Unity Update cycle. This means you can use it directly after in-
/// stantiating it.
/// </summary>
public class RigAngleTracker : MonoBehaviour
{
    // Joint names that exist for both local and remote avatars
    private const string HIPS_NAME = "mixamorig_Hips";

    private const string L_SHOULDER_NAME = "mixamorig_LeftShoulder";
    private const string R_SHOULDER_NAME = "mixamorig_RightShoulder";

    private const string L_ARM_NAME = "mixamorig_LeftArm";
    private const string R_ARM_NAME = "mixamorig_RightArm";

    private const string L_FORE_ARM_NAME = "mixamorig_LeftForeArm";
    private const string R_FORE_ARM_NAME = "mixamorig_RightForeArm";

    private const string L_UP_LEG_NAME = "mixamorig_LeftUpLeg";
    private const string R_UP_LEG_NAME = "mixamorig_RightUpLeg";

    private const string L_LEG_NAME = "mixamorig_LeftLeg";
    private const string R_LEG_NAME = "mixamorig_RightLeg";

    private const string L_FOOT_NAME = "mixamorig_LeftFoot";
    private const string R_FOOT_NAME = "mixamorig_RightFoot";

    private const string L_HAND_NAME = "mixamorig_LeftHand";
    private const string R_HAND_NAME = "mixamorig_RightHand";

    // Bachelor Thesis VRHand 

    private const string L_HAND_THUMB1_NAME = "mixamorig_LeftHandThumb1";
    private const string L_HAND_THUMB2_NAME = "mixamorig_LeftHandThumb2";
    private const string L_HAND_THUMB3_NAME = "mixamorig_LeftHandThumb3";
    private const string L_HAND_THUMB4_NAME = "mixamorig_LeftHandThumb4";

    private const string L_HAND_INDEX1_NAME = "mixamorig_LeftHandIndex1";
    private const string L_HAND_INDEX2_NAME = "mixamorig_LeftHandIndex2";
    private const string L_HAND_INDEX3_NAME = "mixamorig_LeftHandIndex3";
    private const string L_HAND_INDEX4_NAME = "mixamorig_LeftHandIndex4";

    private const string L_HAND_MIDDLE1_NAME = "mixamorig_LeftHandMiddle1";
    private const string L_HAND_MIDDLE2_NAME = "mixamorig_LeftHandMiddle2";
    private const string L_HAND_MIDDLE3_NAME = "mixamorig_LeftHandMiddle3";
    private const string L_HAND_MIDDLE4_NAME = "mixamorig_LeftHandMiddle4";

    private const string L_HAND_RING1_NAME = "mixamorig_LeftHandRing1";
    private const string L_HAND_RING2_NAME = "mixamorig_LeftHandRing2";
    private const string L_HAND_RING3_NAME = "mixamorig_LeftHandRing3";
    private const string L_HAND_RING4_NAME = "mixamorig_LeftHandRing4";

    private const string L_HAND_PINKY1_NAME = "mixamorig_LeftHandPinky1";
    private const string L_HAND_PINKY2_NAME = "mixamorig_LeftHandPinky2";
    private const string L_HAND_PINKY3_NAME = "mixamorig_LeftHandPinky3";
    private const string L_HAND_PINKY4_NAME = "mixamorig_LeftHandPinky4";

    private const string R_HAND_THUMB1_NAME = "mixamorig_RightHandThumb1";
    private const string R_HAND_THUMB2_NAME = "mixamorig_RightHandThumb2";
    private const string R_HAND_THUMB3_NAME = "mixamorig_RightHandThumb3";
    private const string R_HAND_THUMB4_NAME = "mixamorig_RightHandThumb4";

    private const string R_HAND_INDEX1_NAME = "mixamorig_RightHandIndex1";
    private const string R_HAND_INDEX2_NAME = "mixamorig_RightHandIndex2";
    private const string R_HAND_INDEX3_NAME = "mixamorig_RightHandIndex3";
    private const string R_HAND_INDEX4_NAME = "mixamorig_RightHandIndex4";

    private const string R_HAND_MIDDLE1_NAME = "mixamorig_RightHandMiddle1";
    private const string R_HAND_MIDDLE2_NAME = "mixamorig_RightHandMiddle2";
    private const string R_HAND_MIDDLE3_NAME = "mixamorig_RightHandMiddle3";
    private const string R_HAND_MIDDLE4_NAME = "mixamorig_RightHandMiddle4";

    private const string R_HAND_RING1_NAME = "mixamorig_RightHandRing1";
    private const string R_HAND_RING2_NAME = "mixamorig_RightHandRing2";
    private const string R_HAND_RING3_NAME = "mixamorig_RightHandRing3";
    private const string R_HAND_RING4_NAME = "mixamorig_RightHandRing4";

    private const string R_HAND_PINKY1_NAME = "mixamorig_RightHandPinky1";
    private const string R_HAND_PINKY2_NAME = "mixamorig_RightHandPinky2";
    private const string R_HAND_PINKY3_NAME = "mixamorig_RightHandPinky3";
    private const string R_HAND_PINKY4_NAME = "mixamorig_RightHandPinky4";

    // This array contains all names for the Handjoints, it is for later use in UserAvatarService
    public string[] handJointName = new string[] {"mixamorig_LeftHand_JointLink1", "mixamorig_LeftHand_JointLink2", "mixamorig_RightHand_JointLink1", "mixamorig_RightHand_JointLink2",
                                                    "mixamorig_LeftHandThumb1_JointLink1", "mixamorig_LeftHandThumb1_JointLink2", "mixamorig_RightHandThumb1_JointLink1", "mixamorig_RightHandThumb1_JointLink2",
                                                    L_HAND_NAME, R_HAND_NAME,
                                                    "mixamorig_LeftHandThumb1_x", "mixamorig_LeftHandThumb1_y", "mixamorig_LeftHandThumb1_z",
                                                    "mixamorig_RightHandThumb1_x", "mixamorig_RightHandThumb1_y", "mixamorig_RightHandThumb1_z",
                                                    L_HAND_THUMB1_NAME, L_HAND_THUMB2_NAME, L_HAND_THUMB3_NAME, L_HAND_THUMB4_NAME,
                                                    L_HAND_INDEX1_NAME, L_HAND_INDEX2_NAME, L_HAND_INDEX3_NAME, L_HAND_INDEX4_NAME,
                                                    L_HAND_MIDDLE1_NAME, L_HAND_MIDDLE2_NAME, L_HAND_MIDDLE3_NAME, L_HAND_MIDDLE4_NAME,
                                                    L_HAND_RING1_NAME, L_HAND_RING2_NAME, L_HAND_RING3_NAME, L_HAND_RING4_NAME,
                                                    L_HAND_PINKY1_NAME, L_HAND_PINKY2_NAME, L_HAND_PINKY3_NAME, L_HAND_PINKY4_NAME,
                                                    R_HAND_THUMB1_NAME, R_HAND_THUMB2_NAME, R_HAND_THUMB3_NAME, R_HAND_THUMB4_NAME,
                                                    R_HAND_INDEX1_NAME, R_HAND_INDEX2_NAME, R_HAND_INDEX3_NAME, R_HAND_INDEX4_NAME,
                                                    R_HAND_MIDDLE1_NAME, R_HAND_MIDDLE2_NAME, R_HAND_MIDDLE3_NAME, R_HAND_MIDDLE4_NAME,
                                                    R_HAND_RING1_NAME, R_HAND_RING2_NAME, R_HAND_RING3_NAME, R_HAND_RING4_NAME,
                                                    R_HAND_PINKY1_NAME, R_HAND_PINKY2_NAME, R_HAND_PINKY3_NAME, R_HAND_PINKY4_NAME,};

    private bool _initialized = false;

    private int _lastTrackedFrame = -1;

    private Dictionary<string, float> _jointToRadians = new Dictionary<string, float>();

    private Dictionary<string, JointMapping> _jointMappings = new Dictionary<string, JointMapping>();

    public Dictionary<string, float> GetJointToRadianMapping()
    {
        if (!_initialized)
        {
            BuildJointMapping();
            _initialized = true;
        }

        if (_lastTrackedFrame != Time.frameCount)
        {
            UpdateJointToRadians();
            _lastTrackedFrame = Time.frameCount;
        }
        
        return _jointToRadians;
    }

    private void UpdateJointToRadians()
    {
        foreach (var mapping in _jointMappings)
        {
            _jointToRadians[mapping.Key] = mapping.Value.GetJointRadians();
        }
    }

    public void SetJointEulerAngle(string joint, float angle)
    {
        if (!_initialized)
        {
            BuildJointMapping();
            _initialized = true;
        }

        _jointMappings[joint].SetJointEulerAngle(angle);
    }

    public void SetJointRadians(string joint, float rad)
    {
        if (!_initialized)
        {
            BuildJointMapping();
            _initialized = true;
        }

        _jointMappings[joint].SetJointRadians(rad);
    }

    /// <summary>
    /// Builds a mapping of joint relations so we don't have to do that again every frame.
    /// This is a very expensive operation, but it is only executed once, when an avatar is spawned.
    /// We cannot use the joint hierarchy here as the hierarchy is destroyed when our avatar
    /// is spawned by Gazebo remotely.
    /// </summary>
    private void BuildJointMapping()
    {
        var isRemoteAvatar = gameObject.name != "local_avatar";

        if (isRemoteAvatar)
        {
            CreateMappingForRemoteAvatar();
        }
        else
        {
            CreateMappingForLocalAvatar();
        }
        
    }

    private void CreateMappingForRemoteAvatar()
    {
        // These four (now 16) joint links are unique to the remote avatar. The local avatar doesn't have them

        const string L_ARM_JLINK1_NAME = "mixamorig_LeftArm_JointLink1";
        const string L_ARM_JLINK2_NAME = "mixamorig_LeftArm_JointLink2";

        const string R_ARM_JLINK1_NAME = "mixamorig_RightArm_JointLink1";
        const string R_ARM_JLINK2_NAME = "mixamorig_RightArm_JointLink2";

        const string L_HAND_JLINK1_NAME = "mixamorig_LeftHand_JointLink1";
        const string L_HAND_JLINK2_NAME = "mixamorig_LeftHand_JointLink2";

        const string R_HAND_JLINK1_NAME = "mixamorig_RightHand_JointLink1";
        const string R_HAND_JLINK2_NAME = "mixamorig_RightHand_JointLink2";

        const string L_HAND_THUMB1_JLINK1_NAME = "mixamorig_LeftHandThumb1_JointLink1";
        const string L_HAND_THUMB1_JLINK2_NAME = "mixamorig_LeftHandThumb1_JointLink2";

        const string R_HAND_THUMB1_JLINK1_NAME = "mixamorig_RightHandThumb1_JointLink1";
        const string R_HAND_THUMB1_JLINK2_NAME = "mixamorig_RightHandThumb1_JointLink2";

        const string L_UP_LEG_JLINK1_NAME = "mixamorig_LeftUpLeg_JointLink1";
        const string L_UP_LEG_JLINK2_NAME = "mixamorig_LeftUpLeg_JointLink2";

        const string R_UP_LEG_JLINK1_NAME = "mixamorig_RightUpLeg_JointLink1";
        const string R_UP_LEG_JLINK2_NAME = "mixamorig_RightUpLeg_JointLink2";

        Transform hips = FindChildTransformRecursive(transform, HIPS_NAME);

        Transform leftShoulder = FindChildTransformRecursive(transform, L_SHOULDER_NAME);
        Transform rightShoulder = FindChildTransformRecursive(transform, R_SHOULDER_NAME);

        Transform leftArm = FindChildTransformRecursive(transform, L_ARM_NAME);
        Transform leftArm_jlink1 = FindChildTransformRecursive(transform, L_ARM_JLINK1_NAME);
        Transform leftArm_jlink2 = FindChildTransformRecursive(transform, L_ARM_JLINK2_NAME);

        Transform rightArm = FindChildTransformRecursive(transform, R_ARM_NAME);
        Transform rightArm_jlink1 = FindChildTransformRecursive(transform, R_ARM_JLINK1_NAME);
        Transform rightArm_jlink2 = FindChildTransformRecursive(transform, R_ARM_JLINK2_NAME);

        Transform leftForeArm = FindChildTransformRecursive(transform, L_FORE_ARM_NAME);
        Transform rightForeArm = FindChildTransformRecursive(transform, R_FORE_ARM_NAME);

        Transform leftHand = FindChildTransformRecursive(transform, L_HAND_NAME);
        Transform leftHand_jlink1 = FindChildTransformRecursive(transform, L_HAND_JLINK1_NAME);
        Transform leftHand_jlink2 = FindChildTransformRecursive(transform, L_HAND_JLINK2_NAME);

        Transform rightHand = FindChildTransformRecursive(transform, R_HAND_NAME);
        Transform rightHand_jlink1 = FindChildTransformRecursive(transform, R_HAND_JLINK1_NAME);
        Transform rightHand_jlink2 = FindChildTransformRecursive(transform, R_HAND_JLINK2_NAME);

        Transform leftUpLeg = FindChildTransformRecursive(transform, L_UP_LEG_NAME);
        Transform leftUpLeg_jlink1 = FindChildTransformRecursive(transform, L_UP_LEG_JLINK1_NAME);
        Transform leftUpLeg_jlink2 = FindChildTransformRecursive(transform, L_UP_LEG_JLINK2_NAME);

        Transform rightUpLeg = FindChildTransformRecursive(transform, R_UP_LEG_NAME);
        Transform rightUpLeg_jlink1 = FindChildTransformRecursive(transform, R_UP_LEG_JLINK1_NAME);
        Transform rightUpLeg_jlink2 = FindChildTransformRecursive(transform, R_UP_LEG_JLINK2_NAME);

        Transform leftLeg = FindChildTransformRecursive(transform, L_LEG_NAME);
        Transform rightLeg = FindChildTransformRecursive(transform, R_LEG_NAME);

        Transform leftFoot = FindChildTransformRecursive(transform, L_FOOT_NAME);
        Transform rightFoot = FindChildTransformRecursive(transform, R_FOOT_NAME);

        // Bachelor Thesis VRHand
        Transform leftThumb1 = FindChildTransformRecursive(transform, L_HAND_THUMB1_NAME);
        Transform leftThumb1_jlink1 = FindChildTransformRecursive(transform, L_HAND_THUMB1_JLINK1_NAME);
        Transform leftThumb1_jlink2 = FindChildTransformRecursive(transform, L_HAND_THUMB1_JLINK2_NAME);

        Transform leftThumb2 = FindChildTransformRecursive(transform, L_HAND_THUMB2_NAME);
        Transform leftThumb3 = FindChildTransformRecursive(transform, L_HAND_THUMB3_NAME);
        Transform leftThumb4 = FindChildTransformRecursive(transform, L_HAND_THUMB4_NAME);

        Transform leftIndex1 = FindChildTransformRecursive(transform, L_HAND_INDEX1_NAME);
        Transform leftIndex2 = FindChildTransformRecursive(transform, L_HAND_INDEX2_NAME);
        Transform leftIndex3 = FindChildTransformRecursive(transform, L_HAND_INDEX3_NAME);
        Transform leftIndex4 = FindChildTransformRecursive(transform, L_HAND_INDEX4_NAME);

        Transform leftMiddle1 = FindChildTransformRecursive(transform, L_HAND_MIDDLE1_NAME);
        Transform leftMiddle2 = FindChildTransformRecursive(transform, L_HAND_MIDDLE2_NAME);
        Transform leftMiddle3 = FindChildTransformRecursive(transform, L_HAND_MIDDLE3_NAME);
        Transform leftMiddle4 = FindChildTransformRecursive(transform, L_HAND_MIDDLE4_NAME);

        Transform leftRing1 = FindChildTransformRecursive(transform, L_HAND_RING1_NAME);
        Transform leftRing2 = FindChildTransformRecursive(transform, L_HAND_RING2_NAME);
        Transform leftRing3 = FindChildTransformRecursive(transform, L_HAND_RING3_NAME);
        Transform leftRing4 = FindChildTransformRecursive(transform, L_HAND_RING4_NAME);

        Transform leftPinky1 = FindChildTransformRecursive(transform, L_HAND_PINKY1_NAME);
        Transform leftPinky2 = FindChildTransformRecursive(transform, L_HAND_PINKY2_NAME);
        Transform leftPinky3 = FindChildTransformRecursive(transform, L_HAND_PINKY3_NAME);
        Transform leftPinky4 = FindChildTransformRecursive(transform, L_HAND_PINKY4_NAME);

        Transform rightThumb1 = FindChildTransformRecursive(transform, R_HAND_THUMB1_NAME);
        Transform rightThumb1_jlink1 = FindChildTransformRecursive(transform, R_HAND_THUMB1_JLINK1_NAME);
        Transform rightThumb1_jlink2 = FindChildTransformRecursive(transform, R_HAND_THUMB1_JLINK2_NAME);

        Transform rightThumb2 = FindChildTransformRecursive(transform, R_HAND_THUMB2_NAME);
        Transform rightThumb3 = FindChildTransformRecursive(transform, R_HAND_THUMB3_NAME);
        Transform rightThumb4 = FindChildTransformRecursive(transform, R_HAND_THUMB4_NAME);

        Transform rightIndex1 = FindChildTransformRecursive(transform, R_HAND_INDEX1_NAME);
        Transform rightIndex2 = FindChildTransformRecursive(transform, R_HAND_INDEX2_NAME);
        Transform rightIndex3 = FindChildTransformRecursive(transform, R_HAND_INDEX3_NAME);
        Transform rightIndex4 = FindChildTransformRecursive(transform, R_HAND_INDEX4_NAME);

        Transform rightMiddle1 = FindChildTransformRecursive(transform, R_HAND_MIDDLE1_NAME);
        Transform rightMiddle2 = FindChildTransformRecursive(transform, R_HAND_MIDDLE2_NAME);
        Transform rightMiddle3 = FindChildTransformRecursive(transform, R_HAND_MIDDLE3_NAME);
        Transform rightMiddle4 = FindChildTransformRecursive(transform, R_HAND_MIDDLE4_NAME);

        Transform rightRing1 = FindChildTransformRecursive(transform, R_HAND_RING1_NAME);
        Transform rightRing2 = FindChildTransformRecursive(transform, R_HAND_RING2_NAME);
        Transform rightRing3 = FindChildTransformRecursive(transform, R_HAND_RING3_NAME);
        Transform rightRing4 = FindChildTransformRecursive(transform, R_HAND_RING4_NAME);

        Transform rightPinky1 = FindChildTransformRecursive(transform, R_HAND_PINKY1_NAME);
        Transform rightPinky2 = FindChildTransformRecursive(transform, R_HAND_PINKY2_NAME);
        Transform rightPinky3 = FindChildTransformRecursive(transform, R_HAND_PINKY3_NAME);
        Transform rightPinky4 = FindChildTransformRecursive(transform, R_HAND_PINKY4_NAME);

        // Left Arm

        _jointMappings[L_ARM_NAME + "_x"] =
            new JointMapping(leftArm_jlink1, leftArm_jlink2, true, MappedEulerAngle.X);

        _jointMappings[L_ARM_NAME + "_y"] =
            new JointMapping(leftArm_jlink2, leftArm, true, MappedEulerAngle.Y);

        _jointMappings[L_ARM_NAME + "_z"] =
            new JointMapping(leftShoulder, leftArm_jlink1, true, MappedEulerAngle.InvertedZ);

        // Right Arm

        _jointMappings[R_ARM_NAME + "_x"] =
            new JointMapping(rightArm_jlink1, rightArm_jlink2, true, MappedEulerAngle.X);

        _jointMappings[R_ARM_NAME + "_y"] =
            new JointMapping(rightArm_jlink2, rightArm, true, MappedEulerAngle.Y);

        _jointMappings[R_ARM_NAME + "_z"] =
            new JointMapping(rightShoulder, rightArm_jlink1, true, MappedEulerAngle.InvertedZ);

        // ForeArms

        _jointMappings[L_FORE_ARM_NAME] =
            new JointMapping(leftArm, leftForeArm, true, MappedEulerAngle.InvertedZ);

        _jointMappings[R_FORE_ARM_NAME] =
            new JointMapping(rightArm, rightForeArm, true, MappedEulerAngle.InvertedZ);

        // Hands
        /*
        _jointMappings[L_HAND_NAME] =
            new JointMapping(leftForeArm, leftHand, true, MappedEulerAngle.Y);
        */
        /*
        _jointMappings[R_HAND_NAME] =
            new JointMapping(rightForeArm, rightHand, true, MappedEulerAngle.Y);
        */

        _jointMappings[L_HAND_NAME + "_x"] =
            new JointMapping(leftHand_jlink1, leftHand_jlink2, true, MappedEulerAngle.X);

        _jointMappings[L_HAND_NAME + "_y"] =
            new JointMapping(leftHand_jlink2, leftHand, true, MappedEulerAngle.Y);

        _jointMappings[L_HAND_NAME + "_z"] =
            new JointMapping(leftForeArm, leftHand_jlink1, true, MappedEulerAngle.InvertedZ);

        _jointMappings[R_HAND_NAME + "_x"] =
            new JointMapping(rightHand_jlink1, rightHand_jlink2, true, MappedEulerAngle.X);

        _jointMappings[R_HAND_NAME + "_y"] =
            new JointMapping(rightHand_jlink2, rightHand, true, MappedEulerAngle.Y);

        _jointMappings[R_HAND_NAME + "_z"] =
            new JointMapping(rightForeArm, rightHand_jlink1, true, MappedEulerAngle.InvertedZ);

        // Bachelor Thesis VRHand
        // Thumb Finger
        _jointMappings[L_HAND_THUMB1_NAME + "_x"] =
            new JointMapping(leftThumb1_jlink1, leftThumb1_jlink2, true, MappedEulerAngle.X);

        _jointMappings[L_HAND_THUMB1_NAME + "_y"] =
            new JointMapping(leftThumb1_jlink2, leftThumb1, true, MappedEulerAngle.Y);

        _jointMappings[L_HAND_THUMB1_NAME + "_z"] =
            new JointMapping(leftHand, leftThumb1_jlink1, true, MappedEulerAngle.InvertedZ);

        _jointMappings[L_HAND_THUMB2_NAME] =
            new JointMapping(leftThumb1, leftThumb2, true, MappedEulerAngle.Y);

        _jointMappings[L_HAND_THUMB3_NAME] =
            new JointMapping(leftThumb2, leftThumb3, true, MappedEulerAngle.Y);

        _jointMappings[L_HAND_THUMB4_NAME] =
            new JointMapping(leftThumb3, leftThumb4, true, MappedEulerAngle.Y);
        
        _jointMappings[R_HAND_THUMB1_NAME + "_x"] =
            new JointMapping(rightThumb1_jlink1, rightThumb1_jlink2, true, MappedEulerAngle.X);

        _jointMappings[R_HAND_THUMB1_NAME + "_y"] =
            new JointMapping(rightThumb1_jlink2, rightThumb1, true, MappedEulerAngle.Y);

        _jointMappings[R_HAND_THUMB1_NAME + "_z"] =
            new JointMapping(rightHand, rightThumb1_jlink1, true, MappedEulerAngle.InvertedZ);

        _jointMappings[R_HAND_THUMB2_NAME] =
            new JointMapping(rightThumb1, rightThumb2, true, MappedEulerAngle.Y);   // Was Y before, change back to 0 1 0 in Model.Sdf

        _jointMappings[R_HAND_THUMB3_NAME] =
            new JointMapping(rightThumb2, rightThumb3, true, MappedEulerAngle.Y);

        _jointMappings[R_HAND_THUMB4_NAME] =
            new JointMapping(rightThumb3, rightThumb4, true, MappedEulerAngle.Y);
        
        // Index Finger
        _jointMappings[L_HAND_INDEX1_NAME] =
            new JointMapping(leftHand, leftIndex1, true, MappedEulerAngle.Y);

        _jointMappings[L_HAND_INDEX2_NAME] =
            new JointMapping(leftIndex1, leftIndex2, true, MappedEulerAngle.Y);

        _jointMappings[L_HAND_INDEX3_NAME] =
            new JointMapping(leftIndex2, leftIndex3, true, MappedEulerAngle.Y);

        _jointMappings[L_HAND_INDEX4_NAME] =
            new JointMapping(leftIndex3, leftIndex4, true, MappedEulerAngle.Y);

        _jointMappings[R_HAND_INDEX1_NAME] =
            new JointMapping(rightHand, rightIndex1, true, MappedEulerAngle.Y);

        _jointMappings[R_HAND_INDEX2_NAME] =
            new JointMapping(rightIndex1, rightIndex2, true, MappedEulerAngle.Y);

        _jointMappings[R_HAND_INDEX3_NAME] =
            new JointMapping(rightIndex2, rightIndex3, true, MappedEulerAngle.Y);

        _jointMappings[R_HAND_INDEX4_NAME] =
            new JointMapping(rightIndex3, rightIndex4, true, MappedEulerAngle.Y);

        // Middle Finger
        _jointMappings[L_HAND_MIDDLE1_NAME] =
            new JointMapping(leftHand, leftMiddle1, true, MappedEulerAngle.Y);

        _jointMappings[L_HAND_MIDDLE2_NAME] =
            new JointMapping(leftMiddle1, leftMiddle2, true, MappedEulerAngle.Y);

        _jointMappings[L_HAND_MIDDLE3_NAME] =
            new JointMapping(leftMiddle2, leftMiddle3, true, MappedEulerAngle.Y);

        _jointMappings[L_HAND_MIDDLE4_NAME] =
            new JointMapping(leftMiddle3, leftMiddle4, true, MappedEulerAngle.Y);

        _jointMappings[R_HAND_MIDDLE1_NAME] =
            new JointMapping(rightHand, rightMiddle1, true, MappedEulerAngle.Y);

        _jointMappings[R_HAND_MIDDLE2_NAME] =
            new JointMapping(rightMiddle1, rightMiddle2, true, MappedEulerAngle.Y);

        _jointMappings[R_HAND_MIDDLE3_NAME] =
            new JointMapping(rightMiddle2, rightMiddle3, true, MappedEulerAngle.Y);

        _jointMappings[R_HAND_MIDDLE4_NAME] =
            new JointMapping(rightMiddle3, rightMiddle4, true, MappedEulerAngle.Y);

        // Ring Finger
        _jointMappings[L_HAND_RING1_NAME] =
            new JointMapping(leftHand, leftRing1, true, MappedEulerAngle.Y);

        _jointMappings[L_HAND_RING2_NAME] =
            new JointMapping(leftRing1, leftRing2, true, MappedEulerAngle.Y);

        _jointMappings[L_HAND_RING3_NAME] =
            new JointMapping(leftRing2, leftRing3, true, MappedEulerAngle.Y);

        _jointMappings[L_HAND_RING4_NAME] =
            new JointMapping(leftRing3, leftRing4, true, MappedEulerAngle.Y);

        _jointMappings[R_HAND_RING1_NAME] =
            new JointMapping(rightHand, rightRing1, true, MappedEulerAngle.Y);

        _jointMappings[R_HAND_RING2_NAME] =
            new JointMapping(rightRing1, rightRing2, true, MappedEulerAngle.Y);

        _jointMappings[R_HAND_RING3_NAME] =
            new JointMapping(rightRing2, rightRing3, true, MappedEulerAngle.Y);

        _jointMappings[R_HAND_RING4_NAME] =
            new JointMapping(rightRing3, rightRing4, true, MappedEulerAngle.Y);

        // Pinky Finger
        _jointMappings[L_HAND_PINKY1_NAME] =
            new JointMapping(leftHand, leftPinky1, true, MappedEulerAngle.Y);

        _jointMappings[L_HAND_PINKY2_NAME] =
            new JointMapping(leftPinky1, leftPinky2, true, MappedEulerAngle.Y);

        _jointMappings[L_HAND_PINKY3_NAME] =
            new JointMapping(leftPinky2, leftPinky3, true, MappedEulerAngle.Y);

        _jointMappings[L_HAND_PINKY4_NAME] =
            new JointMapping(leftPinky3, leftPinky4, true, MappedEulerAngle.Y);

        _jointMappings[R_HAND_PINKY1_NAME] =
            new JointMapping(rightHand, rightPinky1, true, MappedEulerAngle.Y);

        _jointMappings[R_HAND_PINKY2_NAME] =
            new JointMapping(rightPinky1, rightPinky2, true, MappedEulerAngle.Y);

        _jointMappings[R_HAND_PINKY3_NAME] =
            new JointMapping(rightPinky2, rightPinky3, true, MappedEulerAngle.Y);

        _jointMappings[R_HAND_PINKY4_NAME] =
            new JointMapping(rightPinky3, rightPinky4, true, MappedEulerAngle.Y);

        // Left Upper Leg

        _jointMappings[L_UP_LEG_NAME + "_x"] =
            new JointMapping(leftUpLeg_jlink1, leftUpLeg_jlink2, true, MappedEulerAngle.X);

        _jointMappings[L_UP_LEG_NAME + "_y"] =
            new JointMapping(leftUpLeg_jlink2, leftUpLeg, true, MappedEulerAngle.Y);

        _jointMappings[L_UP_LEG_NAME + "_z"] =
            new JointMapping(hips, leftUpLeg_jlink1, true, MappedEulerAngle.InvertedZ);
        
        // Right Upper Leg

        _jointMappings[R_UP_LEG_NAME + "_x"] =
            new JointMapping(rightUpLeg_jlink1, rightUpLeg_jlink2, true, MappedEulerAngle.X);

        _jointMappings[R_UP_LEG_NAME + "_y"] =
            new JointMapping(rightUpLeg_jlink2, rightUpLeg, true, MappedEulerAngle.Y);

        _jointMappings[R_UP_LEG_NAME + "_z"] =
            new JointMapping(hips, rightUpLeg_jlink1, true, MappedEulerAngle.InvertedZ);

        // Lower Legs

        _jointMappings[L_LEG_NAME] = new JointMapping(leftUpLeg, leftLeg, true, MappedEulerAngle.X);

        _jointMappings[R_LEG_NAME] = new JointMapping(rightUpLeg, rightLeg, true, MappedEulerAngle.X);

        // Feet

        _jointMappings[L_FOOT_NAME] = new JointMapping(leftLeg, leftFoot, true, MappedEulerAngle.X);

        _jointMappings[R_FOOT_NAME] = new JointMapping(rightLeg, rightFoot, true, MappedEulerAngle.X);
    }

    private void CreateMappingForLocalAvatar()
    {
        // Note: With a bunch of clever string manipulation, it would be possible to get rid of all "left"/"right" distinctions here.
        // In all honesty, I  was just too lazy to do it.

        /*
         * Hierarchy (symmetry):
         * - HIPS
         *   - UP_LEG
         *     - LEG
         *       - FOOT
         *   - [Various un-tracked spiny things]
         *     - SPINE2
         *       - SHOULDER
         *         - ARM
         *           - FORE_ARM
         */

        // Find all relevant transforms in the hierarchy
        Transform hips = FindChildTransformRecursive(transform, HIPS_NAME);

        Transform leftShoulder = FindChildTransformRecursive(transform, L_SHOULDER_NAME);
        Transform rightShoulder = FindChildTransformRecursive(transform, R_SHOULDER_NAME);

        Transform leftArm = FindChildTransformRecursive(transform, L_ARM_NAME);
        Transform rightArm = FindChildTransformRecursive(transform, R_ARM_NAME);

        Transform leftForeArm = FindChildTransformRecursive(transform, L_FORE_ARM_NAME);
        Transform rightForeArm = FindChildTransformRecursive(transform, R_FORE_ARM_NAME);

        Transform leftHand = FindChildTransformRecursive(transform, L_HAND_NAME);
        Transform rightHand = FindChildTransformRecursive(transform, R_HAND_NAME);

        Transform leftUpLeg = FindChildTransformRecursive(transform, L_UP_LEG_NAME);
        Transform rightUpLeg = FindChildTransformRecursive(transform, R_UP_LEG_NAME);

        Transform leftLeg = FindChildTransformRecursive(transform, L_LEG_NAME);
        Transform rightLeg = FindChildTransformRecursive(transform, R_LEG_NAME);

        Transform leftFoot = FindChildTransformRecursive(transform, L_FOOT_NAME);
        Transform rightFoot = FindChildTransformRecursive(transform, R_FOOT_NAME);

        // Bachelor Thesis VRHand
        Transform leftThumb1 = FindChildTransformRecursive(transform, L_HAND_THUMB1_NAME);
        Transform leftThumb2 = FindChildTransformRecursive(transform, L_HAND_THUMB2_NAME);
        Transform leftThumb3 = FindChildTransformRecursive(transform, L_HAND_THUMB3_NAME);
        Transform leftThumb4 = FindChildTransformRecursive(transform, L_HAND_THUMB4_NAME);

        Transform leftIndex1 = FindChildTransformRecursive(transform, L_HAND_INDEX1_NAME);
        Transform leftIndex2 = FindChildTransformRecursive(transform, L_HAND_INDEX2_NAME);
        Transform leftIndex3 = FindChildTransformRecursive(transform, L_HAND_INDEX3_NAME);
        Transform leftIndex4 = FindChildTransformRecursive(transform, L_HAND_INDEX4_NAME);

        Transform leftMiddle1 = FindChildTransformRecursive(transform, L_HAND_MIDDLE1_NAME);
        Transform leftMiddle2 = FindChildTransformRecursive(transform, L_HAND_MIDDLE2_NAME);
        Transform leftMiddle3 = FindChildTransformRecursive(transform, L_HAND_MIDDLE3_NAME);
        Transform leftMiddle4 = FindChildTransformRecursive(transform, L_HAND_MIDDLE4_NAME);

        Transform leftRing1 = FindChildTransformRecursive(transform, L_HAND_RING1_NAME);
        Transform leftRing2 = FindChildTransformRecursive(transform, L_HAND_RING2_NAME);
        Transform leftRing3 = FindChildTransformRecursive(transform, L_HAND_RING3_NAME);
        Transform leftRing4 = FindChildTransformRecursive(transform, L_HAND_RING4_NAME);

        Transform leftPinky1 = FindChildTransformRecursive(transform, L_HAND_PINKY1_NAME);
        Transform leftPinky2 = FindChildTransformRecursive(transform, L_HAND_PINKY2_NAME);
        Transform leftPinky3 = FindChildTransformRecursive(transform, L_HAND_PINKY3_NAME);
        Transform leftPinky4 = FindChildTransformRecursive(transform, L_HAND_PINKY4_NAME);

        Transform rightThumb1 = FindChildTransformRecursive(transform, R_HAND_THUMB1_NAME);
        Transform rightThumb2 = FindChildTransformRecursive(transform, R_HAND_THUMB2_NAME);
        Transform rightThumb3 = FindChildTransformRecursive(transform, R_HAND_THUMB3_NAME);
        Transform rightThumb4 = FindChildTransformRecursive(transform, R_HAND_THUMB4_NAME);

        Transform rightIndex1 = FindChildTransformRecursive(transform, R_HAND_INDEX1_NAME);
        Transform rightIndex2 = FindChildTransformRecursive(transform, R_HAND_INDEX2_NAME);
        Transform rightIndex3 = FindChildTransformRecursive(transform, R_HAND_INDEX3_NAME);
        Transform rightIndex4 = FindChildTransformRecursive(transform, R_HAND_INDEX4_NAME);

        Transform rightMiddle1 = FindChildTransformRecursive(transform, R_HAND_MIDDLE1_NAME);
        Transform rightMiddle2 = FindChildTransformRecursive(transform, R_HAND_MIDDLE2_NAME);
        Transform rightMiddle3 = FindChildTransformRecursive(transform, R_HAND_MIDDLE3_NAME);
        Transform rightMiddle4 = FindChildTransformRecursive(transform, R_HAND_MIDDLE4_NAME);

        Transform rightRing1 = FindChildTransformRecursive(transform, R_HAND_RING1_NAME);
        Transform rightRing2 = FindChildTransformRecursive(transform, R_HAND_RING2_NAME);
        Transform rightRing3 = FindChildTransformRecursive(transform, R_HAND_RING3_NAME);
        Transform rightRing4 = FindChildTransformRecursive(transform, R_HAND_RING4_NAME);

        Transform rightPinky1 = FindChildTransformRecursive(transform, R_HAND_PINKY1_NAME);
        Transform rightPinky2 = FindChildTransformRecursive(transform, R_HAND_PINKY2_NAME);
        Transform rightPinky3 = FindChildTransformRecursive(transform, R_HAND_PINKY3_NAME);
        Transform rightPinky4 = FindChildTransformRecursive(transform, R_HAND_PINKY4_NAME);

        // Update Functions. These were extracted from UserAvatarService "GetJointPIDPositionTargetsJointStatesMsg"

        // Left Arm

        _jointMappings[L_ARM_NAME + "_x"] =
            new JointMapping(leftShoulder, leftArm, false, MappedEulerAngle.X);

        _jointMappings[L_ARM_NAME + "_y"] =
            new JointMapping(leftShoulder, leftArm, false, MappedEulerAngle.InvertedZ);

        _jointMappings[L_ARM_NAME + "_z"] =
            new JointMapping(leftShoulder, leftArm, false, MappedEulerAngle.Y);

        // Right Arm

        _jointMappings[R_ARM_NAME + "_x"] =
            new JointMapping(rightShoulder, rightArm, false, MappedEulerAngle.X);

        _jointMappings[R_ARM_NAME + "_y"] =
            new JointMapping(rightShoulder, rightArm, false, MappedEulerAngle.InvertedZ);

        _jointMappings[R_ARM_NAME + "_z"] =
            new JointMapping(rightShoulder, rightArm, false, MappedEulerAngle.Y);

        // ForeArms

        _jointMappings[L_FORE_ARM_NAME] = new JointMapping(leftArm, leftForeArm, false, MappedEulerAngle.Y);

        _jointMappings[R_FORE_ARM_NAME] = new JointMapping(rightArm, rightForeArm, false, MappedEulerAngle.Y);

        // Hands

        //_jointMappings[L_HAND_NAME] = new JointMapping(leftForeArm, leftHand, false, MappedEulerAngle.InvertedZ);

        _jointMappings[L_HAND_NAME + "_x"] =
            new JointMapping(leftForeArm, leftHand, false, MappedEulerAngle.X);

        _jointMappings[L_HAND_NAME + "_y"] =
            new JointMapping(leftForeArm, leftHand, false, MappedEulerAngle.InvertedZ);

        _jointMappings[L_HAND_NAME + "_z"] =
            new JointMapping(leftForeArm, leftHand, false, MappedEulerAngle.Y);

        //_jointMappings[R_HAND_NAME] = new JointMapping(rightForeArm, rightHand, false, MappedEulerAngle.InvertedZ);

        _jointMappings[R_HAND_NAME + "_x"] =
            new JointMapping(rightForeArm, rightHand, false, MappedEulerAngle.X);

        _jointMappings[R_HAND_NAME + "_y"] =
            new JointMapping(rightForeArm, rightHand, false, MappedEulerAngle.InvertedZ);

        _jointMappings[R_HAND_NAME + "_z"] =
            new JointMapping(rightForeArm, rightHand, false, MappedEulerAngle.Y);

        // Bachelor Thesis VRHand
        // Thumb Finger
        _jointMappings[L_HAND_THUMB1_NAME + "_x"] =
            new JointMapping(leftHand, leftThumb1, false, MappedEulerAngle.X);

        _jointMappings[L_HAND_THUMB1_NAME + "_y"] =
            new JointMapping(leftHand, leftThumb1, false, MappedEulerAngle.InvertedZ);

        _jointMappings[L_HAND_THUMB1_NAME + "_z"] =
            new JointMapping(leftHand, leftThumb1, false, MappedEulerAngle.Y);

        _jointMappings[L_HAND_THUMB2_NAME] = new JointMapping(leftThumb1, leftThumb2, false, MappedEulerAngle.InvertedZ);

        _jointMappings[L_HAND_THUMB3_NAME] = new JointMapping(leftThumb2, leftThumb3, false, MappedEulerAngle.InvertedZ);

        _jointMappings[L_HAND_THUMB4_NAME] = new JointMapping(leftThumb3, leftThumb4, false, MappedEulerAngle.InvertedZ);
        
        _jointMappings[R_HAND_THUMB1_NAME + "_x"] =
            new JointMapping(rightHand, rightThumb1, false, MappedEulerAngle.X);

        _jointMappings[R_HAND_THUMB1_NAME + "_y"] =
            new JointMapping(rightHand, rightThumb1, false, MappedEulerAngle.InvertedZ);

        _jointMappings[R_HAND_THUMB1_NAME + "_z"] =
            new JointMapping(rightHand, rightThumb1, false, MappedEulerAngle.Y);

        _jointMappings[R_HAND_THUMB2_NAME] = new JointMapping(rightThumb1, rightThumb2, false, MappedEulerAngle.InvertedZ);

        _jointMappings[R_HAND_THUMB3_NAME] = new JointMapping(rightThumb2, rightThumb3, false, MappedEulerAngle.InvertedZ);

        _jointMappings[R_HAND_THUMB4_NAME] = new JointMapping(rightThumb3, rightThumb4, false, MappedEulerAngle.InvertedZ);
        
        // Index Finger
        _jointMappings[L_HAND_INDEX1_NAME] = new JointMapping(leftHand, leftIndex1, false, MappedEulerAngle.InvertedZ);

        _jointMappings[L_HAND_INDEX2_NAME] = new JointMapping(leftIndex1, leftIndex2, false, MappedEulerAngle.InvertedZ);

        _jointMappings[L_HAND_INDEX3_NAME] = new JointMapping(leftIndex2, leftIndex3, false, MappedEulerAngle.InvertedZ);

        _jointMappings[L_HAND_INDEX4_NAME] = new JointMapping(leftIndex3, leftIndex4, false, MappedEulerAngle.InvertedZ);

        _jointMappings[R_HAND_INDEX1_NAME] = new JointMapping(rightHand, rightIndex1, false, MappedEulerAngle.InvertedZ);

        _jointMappings[R_HAND_INDEX2_NAME] = new JointMapping(rightIndex1, rightIndex2, false, MappedEulerAngle.InvertedZ);

        _jointMappings[R_HAND_INDEX3_NAME] = new JointMapping(rightIndex2, rightIndex3, false, MappedEulerAngle.InvertedZ);

        _jointMappings[R_HAND_INDEX4_NAME] = new JointMapping(rightIndex3, rightIndex4, false, MappedEulerAngle.InvertedZ);

        // Middle Finger
        _jointMappings[L_HAND_MIDDLE1_NAME] = new JointMapping(leftHand, leftMiddle1, false, MappedEulerAngle.InvertedZ);

        _jointMappings[L_HAND_MIDDLE2_NAME] = new JointMapping(leftMiddle1, leftMiddle2, false, MappedEulerAngle.InvertedZ);

        _jointMappings[L_HAND_MIDDLE3_NAME] = new JointMapping(leftMiddle2, leftMiddle3, false, MappedEulerAngle.InvertedZ);

        _jointMappings[L_HAND_MIDDLE4_NAME] = new JointMapping(leftMiddle3, leftMiddle4, false, MappedEulerAngle.InvertedZ);

        _jointMappings[R_HAND_MIDDLE1_NAME] = new JointMapping(rightHand, rightMiddle1, false, MappedEulerAngle.InvertedZ);

        _jointMappings[R_HAND_MIDDLE2_NAME] = new JointMapping(rightMiddle1, rightMiddle2, false, MappedEulerAngle.InvertedZ);

        _jointMappings[R_HAND_MIDDLE3_NAME] = new JointMapping(rightMiddle2, rightMiddle3, false, MappedEulerAngle.InvertedZ);

        _jointMappings[R_HAND_MIDDLE4_NAME] = new JointMapping(rightMiddle3, rightMiddle4, false, MappedEulerAngle.InvertedZ);

        // Ring Finger
        _jointMappings[L_HAND_RING1_NAME] = new JointMapping(leftHand, leftRing1, false, MappedEulerAngle.InvertedZ);

        _jointMappings[L_HAND_RING2_NAME] = new JointMapping(leftRing1, leftRing2, false, MappedEulerAngle.InvertedZ);

        _jointMappings[L_HAND_RING3_NAME] = new JointMapping(leftRing2, leftRing3, false, MappedEulerAngle.InvertedZ);

        _jointMappings[L_HAND_RING4_NAME] = new JointMapping(leftRing3, leftRing4, false, MappedEulerAngle.InvertedZ);

        _jointMappings[R_HAND_RING1_NAME] = new JointMapping(rightHand, rightRing1, false, MappedEulerAngle.InvertedZ);

        _jointMappings[R_HAND_RING2_NAME] = new JointMapping(rightRing1, rightRing2, false, MappedEulerAngle.InvertedZ);

        _jointMappings[R_HAND_RING3_NAME] = new JointMapping(rightRing2, rightRing3, false, MappedEulerAngle.InvertedZ);

        _jointMappings[R_HAND_RING4_NAME] = new JointMapping(rightRing3, rightRing4, false, MappedEulerAngle.InvertedZ);

        // Pinky Finger
        _jointMappings[L_HAND_PINKY1_NAME] = new JointMapping(leftHand, leftPinky1, false, MappedEulerAngle.InvertedZ);

        _jointMappings[L_HAND_PINKY2_NAME] = new JointMapping(leftPinky1, leftPinky2, false, MappedEulerAngle.InvertedZ);

        _jointMappings[L_HAND_PINKY3_NAME] = new JointMapping(leftPinky2, leftPinky3, false, MappedEulerAngle.InvertedZ);

        _jointMappings[L_HAND_PINKY4_NAME] = new JointMapping(leftPinky3, leftPinky4, false, MappedEulerAngle.InvertedZ);

        _jointMappings[R_HAND_PINKY1_NAME] = new JointMapping(rightHand, rightPinky1, false, MappedEulerAngle.InvertedZ);

        _jointMappings[R_HAND_PINKY2_NAME] = new JointMapping(rightPinky1, rightPinky2, false, MappedEulerAngle.InvertedZ);

        _jointMappings[R_HAND_PINKY3_NAME] = new JointMapping(rightPinky2, rightPinky3, false, MappedEulerAngle.InvertedZ);

        _jointMappings[R_HAND_PINKY4_NAME] = new JointMapping(rightPinky3, rightPinky4, false, MappedEulerAngle.InvertedZ);

        // Left Upper Leg

        _jointMappings[L_UP_LEG_NAME + "_x"] =
            new JointMapping(hips, leftUpLeg, false, MappedEulerAngle.X);

        _jointMappings[L_UP_LEG_NAME + "_y"] =
            new JointMapping(hips, leftUpLeg, false, MappedEulerAngle.InvertedZ);

        _jointMappings[L_UP_LEG_NAME + "_z"] =
            new JointMapping(hips, leftUpLeg, false, MappedEulerAngle.Y);

        // Right Upper Leg

        _jointMappings[R_UP_LEG_NAME + "_x"] =
            new JointMapping(hips, rightUpLeg, false, MappedEulerAngle.X);

        _jointMappings[R_UP_LEG_NAME + "_y"] =
            new JointMapping(hips, rightUpLeg, false, MappedEulerAngle.InvertedZ);

        _jointMappings[R_UP_LEG_NAME + "_z"] =
            new JointMapping(hips, rightUpLeg, false, MappedEulerAngle.Y);

        // Lower Legs

        _jointMappings[L_LEG_NAME] = new JointMapping(leftUpLeg, leftLeg, false, MappedEulerAngle.X);

        _jointMappings[R_LEG_NAME] = new JointMapping(rightUpLeg, rightLeg, false, MappedEulerAngle.X);

        // Feet

        _jointMappings[L_FOOT_NAME] = new JointMapping(leftLeg, leftFoot, false, MappedEulerAngle.X);

        _jointMappings[R_FOOT_NAME] = new JointMapping(rightLeg, rightFoot, false, MappedEulerAngle.X);
    }

    /// <summary>
    /// Breadth first search for a child transform with no depth limit.
    /// The name has to be CONTAINED in the Transform/GO name as an exact match (Case-sensitive).
    /// Returns null when no child could be found.
    /// </summary>
    private Transform FindChildTransformRecursive(Transform parent, string name)
    {
        // Search at current depth 1
        foreach (Transform child in parent)
        {
            // Note that we use EndsWith here instead of Contains, since the remote robot will spawn
            // in with some secondary geometry that we do not want to return here.
            // The actual joints always end with the given joint name, hence EndsWith
            if (child.name.EndsWith(name))
            {
                return child;
            }
        }

        // We found nothing, so we have to search recursively
        foreach (Transform child in parent)
        {
            // Prevent infinite recursion by searching ourselves
            if (child == parent)
            {
                continue;
            }

            var result = FindChildTransformRecursive(child, name);

            // Short-circuit return
            if (null != result)
            {
                return result;
            }
        }

        // We didn do find nuddin
        return null;
    }

    /// <summary>
    /// This stuff is a bit hard to get behind, but let's try:
    /// - If you change X to rotate the joint on the local avatar, use MappedEulerAngle.X for both local and remote avatars
    /// - If you change Y to rotate the joint on the local avatar, use MappedEulerAngle.Y for local and MappedEulerAngle.InvertedZ for remote
    /// - If you change Z to rotate the joint on the local avatar, use MappedEulerAngle.InvertedZ for local and MappedEulerAngle.Y for remote
    /// </summary>
    private enum MappedEulerAngle
    {
        X,
        Y,
        InvertedZ,
        InvertedX,
        InvertedY,
        Z
    }

    /// <summary>
    /// Describes the relation between
    /// - The rotational difference between two Unity transforms
    /// - And a single 1-DoF Joint in Gazebo
    ///
    /// Basically, this allows you to talk to Unity transforms as if they were
    /// Gazebo joints; Most notably, you can get and set the current joint angle.
    /// </summary>
    private struct JointMapping
    {
        public readonly Transform Parent;
        public readonly Transform Child;

        /// <summary>
        /// Whether the rotation from the getter should be inverted. Typically,
        /// this is false for local avatars, and true for remote avatars.
        /// The reason for this is a coordinate-system discrepancy between Unity and Gazebo.
        /// This setting does NOT affect the setter (as you should not be modifying
        /// the remote avatar's joints anyway - Let Gazebo do that).
        /// </summary>
        public readonly bool InvertRotation;
        
        public readonly MappedEulerAngle MappedEulerAngle;

        public JointMapping(Transform parent, Transform child, bool invertRotation, MappedEulerAngle mappedEulerAngle)
        {
            Parent = parent;
            Child = child;
            InvertRotation = invertRotation;
            MappedEulerAngle = mappedEulerAngle;
        }

        public float GetJointRadians()
        {
            Quaternion rot_diff = InvertRotation ?
                Quaternion.Inverse(Child.rotation) * Parent.rotation :
                Quaternion.Inverse(Parent.rotation) * Child.rotation;

            Vector3 euler_angles = rot_diff.eulerAngles;
            euler_angles.x = euler_angles.x % 360;
            euler_angles.y = euler_angles.y % 360;
            euler_angles.z = euler_angles.z % 360;

            euler_angles.x = euler_angles.x > 180 ? euler_angles.x - 360 : euler_angles.x;
            euler_angles.y = euler_angles.y > 180 ? euler_angles.y - 360 : euler_angles.y;
            euler_angles.z = euler_angles.z > 180 ? euler_angles.z - 360 : euler_angles.z;

            euler_angles *= Mathf.Deg2Rad;

            switch (MappedEulerAngle)
            {
                case MappedEulerAngle.X:
                    return euler_angles.x;

                case MappedEulerAngle.Y:
                    return euler_angles.y;

                case MappedEulerAngle.InvertedZ:
                    return -euler_angles.z;

                case MappedEulerAngle.InvertedX:
                    return -euler_angles.x;

                case MappedEulerAngle.InvertedY:
                    return -euler_angles.y;

                case MappedEulerAngle.Z:
                    return euler_angles.z;

                default:
                    throw new NotImplementedException();
            }
        }

        public void SetJointRadians(float rad)
        {
            SetJointEulerAngle(rad * Mathf.Rad2Deg);
        }

        public void SetJointEulerAngle(float angle)
        {
            if (InvertRotation)
            {
                throw new InvalidOperationException("Cannot set joint angle on a joint with \"InvertRotation=true\". " +
                                                    "Are you attempting to manipulate the remote avatar instead of the local one?");
            }

            switch (MappedEulerAngle)
            {
                case MappedEulerAngle.X:
                    Child.transform.localEulerAngles = new Vector3(
                        angle, 
                        Child.transform.localEulerAngles.y, 
                        Child.transform.localEulerAngles.z);
                    break;

                case MappedEulerAngle.Y:
                    Child.transform.localEulerAngles = new Vector3(
                        Child.transform.localEulerAngles.x,
                        angle,
                        Child.transform.localEulerAngles.z);
                    break;

                case MappedEulerAngle.InvertedZ:
                    Child.transform.localEulerAngles = new Vector3(
                        Child.transform.localEulerAngles.x,
                        Child.transform.localEulerAngles.y,
                        -angle);
                    break;
            }
        }
    }
}