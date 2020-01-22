using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ConfigJointUtility
{
    public static void CopyPasteComponent(Component pasteTo, Component toCopyFrom)
    {
        UnityEditorInternal.ComponentUtility.CopyComponent(toCopyFrom);
        UnityEditorInternal.ComponentUtility.PasteComponentValues(pasteTo);
    }

    public static Quaternion GetWorldToJointRotation(ConfigurableJoint joint)
    {
        //description of the joint space
        //the x axis of the joint space
        Vector3 jointXAxis = joint.axis.normalized;
        // the y axis of the joint space
        Vector3 jointYAxis = Vector3.Cross(joint.axis, joint.secondaryAxis).normalized;
        //the z axis of the joint space
        Vector3 jointZAxis = Vector3.Cross(jointYAxis, jointXAxis).normalized;
        /*
         * Z axis will be aligned with forward
         * X axis aligned with cross product between forward and upwards
         * Y axis aligned with cross product between Z and X.
         * --> rotates world coordinates to align with joint coordinates
        */

        //check whether there has been a rotation
        if (jointYAxis == Vector3.zero && jointZAxis == Vector3.zero)
        {
            jointYAxis.y += 0.00001f;
            jointYAxis.z += 0.00001f;
        }

            return Quaternion.LookRotation(jointYAxis, jointZAxis);
    }

    public static void SetJointSettings(ConfigurableJoint joint, JointSettings settings)
    {
    }

    public static void AddForce(ConfigurableJoint joint, Vector3 force)
    {
    }
    // TODO: use PDController instead!
    /// <summary>
    /// Creates a new ConfigJointMotionHandler that adds torque to the body part of the joint for a specified duration.
    /// </summary>
    /// <param name="joint">The joint of the bone.</param>
    /// <param name="rotation">The rotation that the joint should try to perform.</param>
    /// <param name="duration">The duration of the influence.</param>
    /// <param name="disableCurrentMotionHandler">Should the current ConfigJointMotionHandler be disabled? This will disable player input.</param>
    /// <param name="isWorldRotation">Is the rotation given in world space?</param>
    /// <param name="isJointRotation">Is the rotation given in joint space?</param>
    /// <param name="monoBehaviour">The monobehavior of the class calling this method. Needed for Coroutine handling.</param>
    public static void SwitchConfigJointMotionHandlerForDuration(ConfigurableJoint joint, Quaternion rotation, float duration, bool disableCurrentMotionHandler, bool isWorldRotation, bool isJointRotation, MonoBehaviour forCoroutine)
    {
        GameObject jointObject = joint.gameObject;
        ConfigJointMotionHandler motionHandler = jointObject.GetComponent<ConfigJointMotionHandler>();
        Quaternion originalOrientation = motionHandler.GetOriginalOrientation();
        bool hasInitialMotionHandler = motionHandler != null && jointObject.GetComponents<ConfigJointMotionHandler>().Length == 1;

        //only one rotation can be true, check to avoid false input
        if (isWorldRotation) isJointRotation = false;
        if (isJointRotation) isWorldRotation = false;

        //no interference from target rotation set by ConfigMotionHandler
        if (disableCurrentMotionHandler)
        {
            if (hasInitialMotionHandler)
            {
                forCoroutine.StartCoroutine(DisableConfigJointMotionHandler(motionHandler, duration));
            }
        }
        //nothing has to change, can be directly applied
        if (isJointRotation)
        {
            forCoroutine.StartCoroutine(SetTargetRotationForDuration(jointObject, rotation, duration));
        }
        //local rotation can be directly applied to new ConfigJointMotionHandler
        else if (!isWorldRotation)
        {
            CreateNewConfigMotionHandlerTarget(joint.gameObject, rotation, duration, originalOrientation, forCoroutine);
        }
        //convert world rotation into local one
        else
        {
            Quaternion localRotation = Quaternion.Inverse(jointObject.transform.rotation) * rotation;
            CreateNewConfigMotionHandlerTarget(jointObject, localRotation, duration, originalOrientation, forCoroutine);
        }
    }

    static void CreateNewConfigMotionHandlerTarget(GameObject jointObject, Quaternion localRotation, float duration, Quaternion originalOrientation, MonoBehaviour forCoroutine)
    {
        ConfigJointMotionHandler newMotionHandler = jointObject.AddComponent<ConfigJointMotionHandler>();
        GameObject tmpTarget = new GameObject();

        tmpTarget.transform.SetParent(jointObject.transform.parent);
        tmpTarget.transform.localScale = Vector3.one;
        tmpTarget.transform.localPosition = Vector3.zero;
        tmpTarget.transform.rotation = Quaternion.identity;
        tmpTarget.transform.localRotation = localRotation;
        newMotionHandler.target = tmpTarget;
        newMotionHandler.SetOriginalOrientation(originalOrientation);
        forCoroutine.StartCoroutine(RenoveConfigJointMotionHandler(newMotionHandler, tmpTarget, duration));
    }

    static IEnumerator DisableConfigJointMotionHandler(ConfigJointMotionHandler motionHandler, float seconds)
    {
        motionHandler.enabled = false;
        yield return new WaitForSeconds(seconds);
        motionHandler.enabled = true;
    }
    static IEnumerator RenoveConfigJointMotionHandler(ConfigJointMotionHandler motionHandler, GameObject target, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Object.Destroy(motionHandler);
        Object.Destroy(target);
    }

    static IEnumerator SetTargetRotationForDuration(GameObject jointObject, Quaternion jointRotation, float seconds)
    {
        ConfigJointMotionHandler newMotionHandler = jointObject.AddComponent<ConfigJointMotionHandler>();
        newMotionHandler.UseJointRotation(jointRotation, true);
        yield return new WaitForSeconds(seconds);
        Object.Destroy(newMotionHandler);
    }

    public static HumanBodyBones MapMixamoNameToHumanBodyBone(string joint)
    {
        switch (joint)
        {
            case "mixamorig_LeftArm_JointLink1":
            case "mixamorig_LeftArm_JointLink2":
            case "mixamorig_LeftArm": return HumanBodyBones.LeftUpperArm;

            case "mixamorig_RightArm_JointLink1":
            case "mixamorig_RightArm_JointLink2":
            case "mixamorig_RightArm": return HumanBodyBones.RightUpperArm;

            case "mixamorig_LeftForeArm": return HumanBodyBones.LeftLowerArm;
            case "mixamorig_RightForeArm": return HumanBodyBones.LeftLowerArm;

            case "mixamorig_LeftLeg": return HumanBodyBones.LeftLowerLeg;
            case "mixamorig_RightLeg": return HumanBodyBones.RightLowerLeg;

            case "mixamorig_LeftUpLeg": return HumanBodyBones.LeftUpperLeg;
            case "mixamorig_RightUpLeg": return HumanBodyBones.RightUpperLeg;

            case "mixamorig_LeftFoot": return HumanBodyBones.LeftFoot;
            case "mixamorig_RightFoot": return HumanBodyBones.RightFoot;

            case "mixamorig_Hips":
            default: return HumanBodyBones.Hips;
        }
    }

    public static string ConvertHumanBodyBonesKeyDictionaryToJson<T>(Dictionary<HumanBodyBones, T> dictionary)
    {
        string result = "";
        foreach (HumanBodyBones bone in dictionary.Keys)
        {
            result += JsonUtility.ToJson(dictionary[bone]) + "\n";
        }

        //remove last line break form json to avoid null reference when converting back to T
        return result.Substring(0, result.Length - 1);
    }

    public static HumanBodyBones GetBoneFromIndex(string index)
    {
        return (HumanBodyBones)int.Parse(index);
    }

    /// <summary>
    /// A safe way to access the exact ConfigurableJoint for the axis defined in the tuning process.
    /// </summary>
    /// <param name="name">The name found in the tuning mappings. Format: HumanBodyBones + Axis</param>
    /// <param name="gameObjectsOfRemoteAvatar"></param>
    /// <returns></returns>
    public static ConfigurableJoint GetRemoteJointOfCorrectAxisFromString(string name, Dictionary<HumanBodyBones, GameObject> gameObjectsOfRemoteAvatar)
    {
        char axis = name.Substring(name.Length - 1)[0];
        HumanBodyBones bone = (HumanBodyBones)System.Enum.Parse(typeof(HumanBodyBones), name.Remove(name.Length - 1));

        GameObject tmp;
        ConfigurableJoint[] joints;
        if(gameObjectsOfRemoteAvatar.TryGetValue(bone, out tmp))
        {
            joints = gameObjectsOfRemoteAvatar[bone].GetComponents<ConfigurableJoint>();
            
            if(joints.Length != 3)
            {
                throw new System.Exception(bone.ToString() + " has not 3 ConfigurableJoints. Make sure to use the MultipleJoint setup for the AvatarManager when tuning.");
            }

            Vector3 primaryAxis;
            switch (axis)
            {
                case 'X': primaryAxis = Vector3.right; break;
                case 'Y': primaryAxis = Vector3.up; break;
                case 'Z': primaryAxis = Vector3.forward; break;
                default: throw new System.Exception("Unknown Axis. You need to check your joint naming. Only the endings X,Y,Z as last character are supported");
            }

            foreach (ConfigurableJoint joint in joints)
            {
                //ignore the axis orientation
                if(joint.axis == primaryAxis || joint.axis == -primaryAxis)
                {
                    return joint;
                }
            }
        }
        else
        {
            throw new System.Exception("No joint has been found at the object of the bone " + bone.ToString() + " in the scene.");
        }

        return null;  
    }
}
