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
    private bool _initialized = false;

    private int _lastTrackedFrame = -1;

    private Dictionary<string, Vector3> _jointToAngles = new Dictionary<string, Vector3>();

    private List<JointMapping> _jointMappings = new List<JointMapping>();

    public Dictionary<string, Vector3> GetJointToRadianMapping()
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

        const string HIPS_NAME = "mixamorig_Hips";
        const string SPINE2_NAME = "mixamorig_Spine2";

        const string L_SHOULDER_NAME = "mixamorig_LeftShoulder";
        const string R_SHOULDER_NAME = "mixamorig_RightShoulder";

        const string L_ARM_NAME = "mixamorig_LeftArm";
        const string R_ARM_NAME = "mixamorig_RightArm";

        const string L_FORE_ARM_NAME = "mixamorig_LeftForeArm";
        const string R_FORE_ARM_NAME = "mixamorig_RightForeArm";

        const string L_UP_LEG_NAME = "mixamorig_LeftUpLeg";
        const string R_UP_LEG_NAME = "mixamorig_RightUpLeg";

        const string L_LEG_NAME = "mixamorig_LeftLeg";
        const string R_LEG_NAME = "mixamorig_RightLeg";

        const string L_FOOT_NAME = "mixamorig_LeftFoot";
        const string R_FOOT_NAME = "mixamorig_RightFoot";

        // Find all relevant transforms in the hierarchy
        Transform hips = FindChildTransformRecursive(transform, HIPS_NAME);
        Transform spine2 = FindChildTransformRecursive(transform, SPINE2_NAME);

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
            mappingUpdateFunc: euler_angles => UpdateArmMapping(L_ARM_NAME, euler_angles)));

        _jointMappings.Add(new JointMapping(
            parent: rightShoulder,
            child: rightArm,
            mappingUpdateFunc: euler_angles => UpdateArmMapping(R_ARM_NAME, euler_angles)));

        // ForeArms
        _jointMappings.Add(new JointMapping(
            parent: leftArm,
            child: leftForeArm,
            mappingUpdateFunc: euler_angles => UpdateForeArmMapping(L_FORE_ARM_NAME, euler_angles)));

        _jointMappings.Add(new JointMapping(
            parent: rightArm,
            child: rightForeArm,
            mappingUpdateFunc: euler_angles => UpdateForeArmMapping(R_FORE_ARM_NAME, euler_angles)));

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

        // Shoulders
        _jointMappings.Add(new JointMapping(
            parent: spine2,
            child: leftShoulder,
            mappingUpdateFunc: euler_angles => UpdateOtherMapping(L_SHOULDER_NAME, euler_angles)));

        _jointMappings.Add(new JointMapping(
            parent: spine2,
            child: rightShoulder,
            mappingUpdateFunc: euler_angles => UpdateOtherMapping(R_SHOULDER_NAME, euler_angles)));
    }

    private void UpdateArmMapping(string armName, Vector3 euler_angles)
    {
        string joint_x_axis = armName + "_x";
        string joint_y_axis = armName + "_y";
        string joint_z_axis = armName + "_z";

        euler_angles = euler_angles * Mathf.Deg2Rad;

        _jointToAngles[joint_x_axis] = new Vector3(euler_angles.x, 0, 0);
        _jointToAngles[joint_y_axis] = new Vector3(-euler_angles.z, 0, 0);
        _jointToAngles[joint_z_axis] = new Vector3(euler_angles.y, 0, 0);
    }

    private void UpdateForeArmMapping(string foreArmName, Vector3 euler_angles)
    {
        euler_angles = new Vector3(euler_angles.y, euler_angles.x, euler_angles.z);
        euler_angles = euler_angles * Mathf.Deg2Rad;

        _jointToAngles[foreArmName] = new Vector3(euler_angles.x, euler_angles.y, euler_angles.z);
    }

    private void UpdateOtherMapping(string jointName, Vector3 euler_angles)
    {
        euler_angles = euler_angles * Mathf.Deg2Rad;

        _jointToAngles[jointName] = new Vector3(euler_angles.x, euler_angles.y, euler_angles.z);
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
            if (child.name.Contains(name))
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