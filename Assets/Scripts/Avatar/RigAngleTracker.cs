using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

    private bool _initialized = false;

    private int _lastTrackedFrame = -1;

    private Dictionary<string, float> _jointToRadians = new Dictionary<string, float>();

    private Dictionary<string, JointMapping> _jointMappings = new Dictionary<string, JointMapping>();

    Dictionary<string, int> jointDepths = new Dictionary<string, int>();

    public Dictionary<string, float> GetJointToRadianMapping()
    {
        if (!_initialized)
        {
            Debug.Log("Mapping");
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

        if (!UserAvatarService.Instance.use_gazebo)
        {
            UserAvatarService.Instance._avatarManager.InitializeBodyStructures();
        }

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
        if (UserAvatarService.Instance.use_gazebo)
        {
            SetJointMapptingsGazeboRemote();
        }
        else
        {
            SetJointMappingsNonGazebo(UserAvatarService.Instance._avatarManager.GetGameObjectPerBoneRemoteAvatarDictionary(), false);
        }
    }

    private void CreateMappingForLocalAvatar()
    {
        if (UserAvatarService.Instance.use_gazebo)
        {
            SetJointMappingsGazeboLocal();
        }
        else
        {
            SetJointMappingsNonGazebo(UserAvatarService.Instance._avatarManager.GetGameObjectPerBoneLocalAvatarDictionary(), true);
        }
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
        Z,
        InvertedZ
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
                case MappedEulerAngle.Z:
                    Child.transform.localEulerAngles = new Vector3(
                        Child.transform.localEulerAngles.x,
                        Child.transform.localEulerAngles.y,
                        angle);
                    break;
            }
        }
    }

    void SetJointMappingsNonGazebo(Dictionary<HumanBodyBones, GameObject> gameObjectsPerBone, bool isLocal)
    {
        Dictionary<string, JointMapping> mappings = new Dictionary<string, JointMapping>();
        jointDepths.Clear();
        foreach (HumanBodyBones bone in gameObjectsPerBone.Keys)
        {
            //we cannot tune a locked joint
            if (!UserAvatarService.Instance._avatarManager.GetFixedJoints().Contains(bone))
            {
                GameObject tmp;
                if (gameObjectsPerBone.TryGetValue(bone, out tmp))
                {
                    char index = 'X';
                    for (int i = 0; i < 3; i++)
                    {
                        ConfigurableJoint joint = tmp.GetComponent<ConfigurableJoint>();

                        if (joint != null)
                        {
                            Transform parent = joint.connectedBody.transform;
                            SetJointMappingsNonGazeboHelper(mappings, tmp, parent, bone, index, i, gameObjectsPerBone[HumanBodyBones.Hips].transform);
                        }
                        else
                        {
                            if (isLocal)
                            {
                                Transform parent = tmp.transform.parent.transform;
                                SetJointMappingsNonGazeboHelper(mappings, tmp, parent, bone, index, i, gameObjectsPerBone[HumanBodyBones.Hips].transform);
                            }
                        }
                    }
                }
            }
        }
        _jointMappings = EnforceJointMappingsHierarchy(mappings);


    }
    /// <summary>
    /// This will sort the joint mappings according to their depth inside the avatar hierarchy. The tuning will start with the upper legs and will then move to the first spine joint, back to the lower legs followed by the second spine joint etc.
    /// This ensures that each limb has been tuned from the torso towards the extremeties.
    /// </summary>
    /// <param name="mappings"></param>
    /// <returns></returns>
    Dictionary<string, JointMapping> EnforceJointMappingsHierarchy(Dictionary<string, JointMapping> mappings)
    {
        Dictionary<string, JointMapping> sortedMappings = new Dictionary<string, JointMapping>();
        
        foreach (KeyValuePair<string, int> joint in jointDepths.OrderBy(x => x.Value)) 
        {
            sortedMappings.Add(joint.Key, mappings[joint.Key]);
        }
        return sortedMappings;
    }

    void SetJointMappingsGazeboLocal()
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

        _jointMappings[L_HAND_NAME] = new JointMapping(leftForeArm, leftHand, false, MappedEulerAngle.InvertedZ);

        _jointMappings[R_HAND_NAME] = new JointMapping(rightForeArm, rightHand, false, MappedEulerAngle.InvertedZ);

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

    void SetJointMapptingsGazeboRemote()
    {
        // These four joint links are unique to the remote avatar. The local avatar doesn't have them

        const string L_ARM_JLINK1_NAME = "mixamorig_LeftArm_JointLink1";
        const string L_ARM_JLINK2_NAME = "mixamorig_LeftArm_JointLink2";

        const string R_ARM_JLINK1_NAME = "mixamorig_RightArm_JointLink1";
        const string R_ARM_JLINK2_NAME = "mixamorig_RightArm_JointLink2";

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
        Transform rightHand = FindChildTransformRecursive(transform, R_HAND_NAME);

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

        _jointMappings[L_HAND_NAME] =
            new JointMapping(leftForeArm, leftHand, true, MappedEulerAngle.Y);

        _jointMappings[R_HAND_NAME] =
            new JointMapping(rightForeArm, rightHand, true, MappedEulerAngle.Y);

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

    void SetJointMappingsNonGazeboHelper(Dictionary<string, JointMapping> mappings, GameObject obj,Transform jointParent, HumanBodyBones bone, int index, int iteration, Transform rootBone)
    {
        Transform parent = jointParent;
        Transform child = obj.transform;

        string key = bone.ToString() + (char)(index + iteration);
        JointMapping value = new JointMapping(parent, child, false, (MappedEulerAngle)iteration);
        mappings.Add(key, value);
        //we store at what relative depth inside of the avatar hierarchy the joint is located
        jointDepths.Add(key, LocalPhysicsToolkit.GetDepthOfBone(obj.transform, rootBone));
    }
}