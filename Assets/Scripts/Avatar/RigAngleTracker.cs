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

    private bool _initialized = false;

    private int _lastTrackedFrame = -1;

    private Dictionary<string, float> _jointToAngles = new Dictionary<string, float>();

    private List<JointMapping> _jointMappings = new List<JointMapping>();

    public Dictionary<string, float> GetJointToRadianMapping()
    {
        if (!_initialized)
        {
            BuildJointMapping();
            _initialized = true;
        }

        if (_lastTrackedFrame != Time.frameCount)
        {
            UpdateJointAngles();
            _lastTrackedFrame = Time.frameCount;
        }

        return _jointToAngles;
    }

    private void UpdateJointAngles()
    {
        foreach (var mapping in _jointMappings)
        {
            Quaternion rot_diff = Quaternion.Inverse(mapping.Parent.rotation) * mapping.Child.rotation;

            Vector3 euler_angles = rot_diff.eulerAngles;
            euler_angles.x = euler_angles.x % 360;
            euler_angles.y = euler_angles.y % 360;
            euler_angles.z = euler_angles.z % 360;

            euler_angles.x = euler_angles.x > 180 ? euler_angles.x - 360 : euler_angles.x;
            euler_angles.y = euler_angles.y > 180 ? euler_angles.y - 360 : euler_angles.y;
            euler_angles.z = euler_angles.z > 180 ? euler_angles.z - 360 : euler_angles.z;

            mapping.MappingUpdateFunc(euler_angles);
        }
    }

    /// <summary>
    /// Builds a mapping of joint relations so we don't have to do that again every frame.
    /// This is a very expensive operation, but it is only executed once, when an avatar is spawned.
    /// We cannot use the joint hierarchy here as the hierarchy is destroyed when our avatar
    /// is spawned by Gazebo remotely.
    /// </summary>
    private void BuildJointMapping()
    {
        var isRemoteAvatar = gameObject.name != "avatar_rig";

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
        // These joint links are unique to the remote avatar. The local avatar doesn't have them
        const string L_ARM_JLINK1_NAME = "mixamorig_LeftArm_JointLink1";
        const string L_ARM_JLINK2_NAME = "mixamorig_LeftArm_JointLink2";

        const string R_ARM_JLINK1_NAME = "mixamorig_RightArm_JointLink1";
        const string R_ARM_JLINK2_NAME = "mixamorig_RightArm_JointLink2";

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

        Transform leftUpLeg = FindChildTransformRecursive(transform, L_UP_LEG_NAME);
        Transform rightUpLeg = FindChildTransformRecursive(transform, R_UP_LEG_NAME);

        Transform leftLeg = FindChildTransformRecursive(transform, L_LEG_NAME);
        Transform rightLeg = FindChildTransformRecursive(transform, R_LEG_NAME);

        Transform leftFoot = FindChildTransformRecursive(transform, L_FOOT_NAME);
        Transform rightFoot = FindChildTransformRecursive(transform, R_FOOT_NAME);

        // Left Arm

        _jointMappings.Add(new JointMapping(
            parent: leftArm_jlink1,
            child: leftShoulder,
            mappingUpdateFunc: euler_angles =>
            {
                euler_angles *= Mathf.Deg2Rad;
                _jointToAngles[L_ARM_NAME + "_z"] = -euler_angles.z;
            }));

        _jointMappings.Add(new JointMapping(
            parent: leftArm_jlink2,
            child: leftArm_jlink1,
            mappingUpdateFunc: euler_angles =>
            {
                euler_angles *= Mathf.Deg2Rad;
                _jointToAngles[L_ARM_NAME + "_x"] = euler_angles.x;
            }));

        _jointMappings.Add(new JointMapping(
            parent: leftArm,
            child: leftArm_jlink2,
            mappingUpdateFunc: euler_angles =>
            {
                euler_angles *= Mathf.Deg2Rad;
                _jointToAngles[L_ARM_NAME + "_y"] = euler_angles.y;
            }));

        // Right Arm

        _jointMappings.Add(new JointMapping(
            parent: rightArm_jlink1,
            child: rightShoulder,
            mappingUpdateFunc: euler_angles =>
            {
                euler_angles *= Mathf.Deg2Rad;
                _jointToAngles[R_ARM_NAME + "_z"] = -euler_angles.z;
            }));

        _jointMappings.Add(new JointMapping(
            parent: rightArm_jlink2,
            child: rightArm_jlink1,
            mappingUpdateFunc: euler_angles =>
            {
                euler_angles *= Mathf.Deg2Rad;
                _jointToAngles[R_ARM_NAME + "_x"] = euler_angles.x;
            }));

        _jointMappings.Add(new JointMapping(
            parent: rightArm,
            child: rightArm_jlink2,
            mappingUpdateFunc: euler_angles =>
            {
                euler_angles *= Mathf.Deg2Rad;
                _jointToAngles[R_ARM_NAME + "_y"] = euler_angles.y;
            }));

        // ForeArms

        _jointMappings.Add(new JointMapping(
            parent: leftForeArm,
            child: leftArm,
            mappingUpdateFunc: euler_angles =>
            {
                euler_angles *= Mathf.Deg2Rad;
                _jointToAngles[L_FORE_ARM_NAME] = -euler_angles.z;
            }));

        _jointMappings.Add(new JointMapping(
            parent: rightForeArm,
            child: rightArm,
            mappingUpdateFunc: euler_angles => 
            {
                euler_angles *= Mathf.Deg2Rad;
                _jointToAngles[R_FORE_ARM_NAME] = -euler_angles.z;
            }));

        // Upper Legs

        _jointMappings.Add(new JointMapping(
            parent: leftUpLeg,
            child: hips,
            mappingUpdateFunc: euler_angles => UpdateOtherMapping(L_UP_LEG_NAME, euler_angles)));

        _jointMappings.Add(new JointMapping(
            parent: rightUpLeg,
            child: hips,
            mappingUpdateFunc: euler_angles => UpdateOtherMapping(R_UP_LEG_NAME, euler_angles)));

        // Lower Legs

        _jointMappings.Add(new JointMapping(
            parent: leftLeg,
            child: leftUpLeg,
            mappingUpdateFunc: euler_angles => UpdateOtherMapping(L_LEG_NAME, euler_angles)));

        _jointMappings.Add(new JointMapping(
            parent: rightLeg,
            child: rightUpLeg,
            mappingUpdateFunc: euler_angles => UpdateOtherMapping(R_LEG_NAME, euler_angles)));

        // Feet

        _jointMappings.Add(new JointMapping(
            parent: leftFoot,
            child: leftLeg,
            mappingUpdateFunc: euler_angles => UpdateOtherMapping(L_FOOT_NAME, euler_angles)));

        _jointMappings.Add(new JointMapping(
            parent: rightFoot,
            child: rightLeg,
            mappingUpdateFunc: euler_angles => UpdateOtherMapping(R_FOOT_NAME, euler_angles)));
    }

    private void CreateMappingForLocalAvatar()
    {
        // Note: With a bunch of clever string manipulation, it would be possible to get rid of all "left"/"right" distinctions here.
        // In all honesty, I just was too lazy to do it.

        /*
         * Hierachy (symmetry):
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

        Transform leftUpLeg = FindChildTransformRecursive(transform, L_UP_LEG_NAME);
        Transform rightUpLeg = FindChildTransformRecursive(transform, R_UP_LEG_NAME);

        Transform leftLeg = FindChildTransformRecursive(transform, L_LEG_NAME);
        Transform rightLeg = FindChildTransformRecursive(transform, R_LEG_NAME);

        Transform leftFoot = FindChildTransformRecursive(transform, L_FOOT_NAME);
        Transform rightFoot = FindChildTransformRecursive(transform, R_FOOT_NAME);

        // Update Functions. These were extracted from UserAvatarService "GetJointPIDPositionTargetsJointStatesMsg"

        // Arms

        _jointMappings.Add(new JointMapping(
            parent: leftShoulder,
            child: leftArm,
            mappingUpdateFunc: euler_angles => UpdateArmMappingLocal(L_ARM_NAME, euler_angles)));

        _jointMappings.Add(new JointMapping(
            parent: rightShoulder,
            child: rightArm,
            mappingUpdateFunc: euler_angles => UpdateArmMappingLocal(R_ARM_NAME, euler_angles)));

        // ForeArms

        _jointMappings.Add(new JointMapping(
            parent: leftArm,
            child: leftForeArm,
            mappingUpdateFunc: euler_angles => UpdateForeArmMappingLocal(L_FORE_ARM_NAME, euler_angles)));

        _jointMappings.Add(new JointMapping(
            parent: rightArm,
            child: rightForeArm,
            mappingUpdateFunc: euler_angles => UpdateForeArmMappingLocal(R_FORE_ARM_NAME, euler_angles)));

        // Upper Legs

        _jointMappings.Add(new JointMapping(
            parent: hips,
            child: leftUpLeg,
            mappingUpdateFunc: euler_angles => UpdateOtherMapping(L_UP_LEG_NAME, euler_angles)));

        _jointMappings.Add(new JointMapping(
            parent: hips,
            child: rightUpLeg,
            mappingUpdateFunc: euler_angles => UpdateOtherMapping(R_UP_LEG_NAME, euler_angles)));

        // Lower Legs

        _jointMappings.Add(new JointMapping(
            parent: leftUpLeg,
            child: leftLeg,
            mappingUpdateFunc: euler_angles => UpdateOtherMapping(L_LEG_NAME, euler_angles)));

        _jointMappings.Add(new JointMapping(
            parent: rightUpLeg,
            child: rightLeg,
            mappingUpdateFunc: euler_angles => UpdateOtherMapping(R_LEG_NAME, euler_angles)));

        // Feet

        _jointMappings.Add(new JointMapping(
            parent: leftLeg,
            child: leftFoot,
            mappingUpdateFunc: euler_angles => UpdateOtherMapping(L_FOOT_NAME, euler_angles)));

        _jointMappings.Add(new JointMapping(
            parent: rightLeg,
            child: rightFoot,
            mappingUpdateFunc: euler_angles => UpdateOtherMapping(R_FOOT_NAME, euler_angles)));
    }

    /// <summary>
    /// This method only works for the local avatar since there is only one arm link for the shoulder.
    /// On the remote avatar, there are 3 arm links per shoulder, so it needs to be handled differently.
    /// </summary>
    private void UpdateArmMappingLocal(string armName, Vector3 euler_angles)
    {
        string joint_x_axis = armName + "_x";
        string joint_y_axis = armName + "_y";
        string joint_z_axis = armName + "_z";

        euler_angles = euler_angles * Mathf.Deg2Rad;

        _jointToAngles[joint_x_axis] = euler_angles.x;

        _jointToAngles[joint_y_axis] = -euler_angles.z;
        _jointToAngles[joint_z_axis] = euler_angles.y;
    }

    /// <summary>
    /// This version  also only works for a local avatar. Don't know why tbh.
    /// </summary>
    private void UpdateForeArmMappingLocal(string foreArmName, Vector3 euler_angles)
    {
        euler_angles = new Vector3(euler_angles.y, euler_angles.x, euler_angles.z);
        euler_angles = euler_angles * Mathf.Deg2Rad;

        _jointToAngles[foreArmName] = euler_angles.x;
    }

    private void UpdateOtherMapping(string jointName, Vector3 euler_angles)
    {
        euler_angles = euler_angles * Mathf.Deg2Rad;

        _jointToAngles[jointName] = euler_angles.x;
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

    private struct JointMapping
    {
        public Transform Parent;
        public Transform Child;
        public Action<Vector3> MappingUpdateFunc;

        public JointMapping(Transform parent, Transform child, Action<Vector3> mappingUpdateFunc)
        {
            Parent = parent;
            Child = child;
            MappingUpdateFunc = mappingUpdateFunc;
        }
    }
}