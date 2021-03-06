﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides access to a collection of useful functions
/// </summary>
public static class LocalPhysicsToolkit
{
    static int depth = 0;

    /// <summary>
    /// Used to copy the values of a component and paste them in the component of another GameObject 
    /// </summary>
    /// <param name="pasteTo">Component of the target GameObject. Its values will be overwritten.</param>
    /// <param name="toCopyFrom">Component of the original GameObject, these values will be copied.</param>
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

        //check whether there has been a rotation, insert a small artificial rotation so that Quaternion.LookRotation will not throw an error.
        if (jointYAxis == Vector3.zero && jointZAxis == Vector3.zero)
        {
            jointYAxis.y += 0.00001f;
            jointYAxis.z += 0.00001f;
        }

        return Quaternion.LookRotation(jointYAxis, jointZAxis);
    }

    /// <summary>
    /// A workaround since Unity cannot convert Dictionary to Json
    /// </summary>
    /// <typeparam name="K">The type of the dictionary's key</typeparam>
    /// <typeparam name="V">The type of the dictionary's value</typeparam>
    /// <param name="dictionary">The dictionary that needs to be converted</param>
    /// <returns></returns>
    public static string ConvertDictionaryToJson<K, V>(Dictionary<K, V> dictionary)
    {
        string result = "";
        foreach (K bone in dictionary.Keys)
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
    /// <param name="gameObjectsOfRemoteAvatar">the remote avatars dictionary provided by the AvatarManager</param>
    /// <returns></returns>
    public static ConfigurableJoint GetRemoteJointOfCorrectAxisFromString(string name, Dictionary<HumanBodyBones, GameObject> gameObjectsOfRemoteAvatar)
    {
        char axis = name.Substring(name.Length - 1)[0];
        HumanBodyBones bone = (HumanBodyBones)System.Enum.Parse(typeof(HumanBodyBones), name.Remove(name.Length - 1));

        GameObject tmp;
        //get body part
        if(gameObjectsOfRemoteAvatar.TryGetValue(bone, out tmp))
        { 
            ConfigurableJoint[] joints;
            joints = tmp.GetComponents<ConfigurableJoint>();
            
            if(joints.Length != 3)
            {
                throw new System.Exception(bone.ToString() + " has not 3 ConfigurableJoints. Make sure to use the MultipleJoint setup for the AvatarManager when tuning.");
            }

            Vector3 primaryAxis;
            // get correct primary axis of the joint
            switch (axis)
            {
                case 'X': primaryAxis = Vector3.right; break;
                case 'Y': primaryAxis = Vector3.up; break;
                case 'Z': primaryAxis = Vector3.forward; break;
                default: throw new System.Exception("Unknown Axis. You need to check your joint naming. Only the endings X,Y,Z as last character are supported");
            }

            //search for a joint in the body part that has the correct axis
            foreach (ConfigurableJoint joint in joints)
            {
                //ignore the axis orientation
                if(joint.axis.Equals(primaryAxis) || joint.axis.Equals(-primaryAxis))
                {
                    return joint;
                }
            }

            throw new System.Exception(bone.ToString() + ": No joint axis " + primaryAxis + " found.");
        }
        else
        {
            throw new System.Exception("No object for the bone " + bone.ToString() + " has been found in the scene.");
        }
    }

    /// <summary>
    /// Finds the level of the GameObject in the skeleton hierarchy
    /// </summary>
    /// <param name="boneInScene"></param>
    /// <param name="rootBone"></param>
    /// <returns></returns>
    public static int GetDepthOfBone(Transform boneInScene, Transform rootBone)
    {
        depth = 0;
        GetDepthOfBoneHelper(boneInScene, rootBone);
        return depth;
    }

    static void GetDepthOfBoneHelper(Transform bone, Transform rootBone)
    {
        Transform parent = bone.parent;

        if (parent != null && !parent.Equals(rootBone))
        {
            depth++;
            GetDepthOfBoneHelper(parent, rootBone);
        }
        return;
    }


    //////////////////////////////////////////////////////////////////// LEGACY: CONFIGJOINTMOTIONHANDLER FOR DURATION ////////////////////////////////////////////////////////////////////////
    #region experimental

    /// <summary>
    /// Creates a new ConfigJointMotionHandler that adds torque to the body part of the joint for a specified duration. Experimental stage.
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

    /// <summary>
    /// Helper class for SwitchConfigJointMotionHandlerForDuration
    /// </summary>
    /// <param name="jointObject"></param>
    /// <param name="localRotation"></param>
    /// <param name="duration"></param>
    /// <param name="originalOrientation"></param>
    /// <param name="forCoroutine"></param>
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

    /// <summary>
    /// Helper class for CreateNewConfigMotionHandlerTarget
    /// </summary>
    /// <param name="motionHandler"></param>
    /// <param name="seconds"></param>
    /// <returns></returns>
    static IEnumerator DisableConfigJointMotionHandler(ConfigJointMotionHandler motionHandler, float seconds)
    {
        motionHandler.enabled = false;
        yield return new WaitForSeconds(seconds);
        motionHandler.enabled = true;
    }
    /// <summary>
    /// Helper class for SwitchConfigJointMotionHandlerForDuration
    /// </summary>
    /// <param name="motionHandler"></param>
    /// <param name="target"></param>
    /// <param name="seconds"></param>
    /// <returns></returns>
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
    #endregion
}
