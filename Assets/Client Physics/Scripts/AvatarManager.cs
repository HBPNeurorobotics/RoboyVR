using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(ConfigJointManager))]
[RequireComponent(typeof(BoneMeshContainer))]
public class AvatarManager : MonoBehaviour
{
    public bool useJoints = true;

    [Header("PD Control")]
    public float PDKp = 1;
    public float PDKd = 1;
    public float Kp = 8;
    public float Ki = 0;
    public float Kd = .05f;

    BodyGroups.BODYGROUP bodyGroup = BodyGroups.BODYGROUP.ALL_COMBINED;

    BodyGroups bodyGroupsRemote;
    BodyGroups bodyGroupsTarget;

    ConfigJointManager configJointManager;

    Animator animatorRemoteAvatar;
    Animator animatorLocalAvatar;

    //The bones of the character that physiscs should be applied to
    Dictionary<HumanBodyBones, GameObject> gameObjectPerBoneRemoteAvatar = new Dictionary<HumanBodyBones, GameObject>();
    Dictionary<HumanBodyBones, Quaternion> orientationPerBoneRemoteAvatarAtStart = new Dictionary<HumanBodyBones, Quaternion>();
    Dictionary<GameObject, HumanBodyBones> bonesPerGameObjectRemoteAvatar = new Dictionary<GameObject, HumanBodyBones>();
    //The bones of the character that the remote avatar imitates
    Dictionary<HumanBodyBones, GameObject> gameObjectPerBoneLocalAvatar = new Dictionary<HumanBodyBones, GameObject>();

    List<HumanBodyBones> bonesInOrder = new List<HumanBodyBones>();

    // Use this for initialization
    void Start()
    {
        InitializeBodyStructures();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!useJoints)
        {
            UpdatePDControllers();
        }
        else
        {
            //UpdatePDControllers();
            if (configJointManager.inputByManager)
            {
                //UpdateJoints();
                UpdateJointsRecursive(gameObjectPerBoneRemoteAvatar[HumanBodyBones.Hips].transform);
                
            }
        }
        //UpdateVacuumBreatherPIDControllers();
        //UpdateJoints();
        //UpdateMerchVRPIDControllers();
        if (Input.GetKey(KeyCode.F))
        {
            gameObjectPerBoneRemoteAvatar[HumanBodyBones.LeftLowerArm].GetComponent<Rigidbody>().AddForce(Vector3.down * 100, ForceMode.Force);
        }

    }

    /// <summary>
    ///     Maps all HumanBodyBones (assigned in the Animator) to their GameObjects in the scene in order to get access to all components.
    ///     Adds Rigidbody to both bodies, adds PDController to the avatar if useJoints is false and a single ConfigJointManager otherwise.
    /// </summary>
    public void InitializeBodyStructures()
    {


        if (gameObjectPerBoneLocalAvatar.Keys.Count == 0 && gameObjectPerBoneRemoteAvatar.Keys.Count == 0)
        {
            animatorRemoteAvatar = GetComponentInChildren<Animator>();
            animatorLocalAvatar = GameObject.FindGameObjectWithTag("Target").GetComponent<Animator>();
            foreach (HumanBodyBones bone in System.Enum.GetValues(typeof(HumanBodyBones)))
            {
                //LastBone is not mapped to a bodypart, we need to skip it.
                if (bone != HumanBodyBones.LastBone)
                {
                    Transform boneTransformRemoteAvatar = animatorRemoteAvatar.GetBoneTransform(bone);
                    Transform boneTransformLocalAvatar = animatorLocalAvatar.GetBoneTransform(bone);
                    //We have to skip unassigned bodyparts.
                    if (boneTransformRemoteAvatar != null && boneTransformLocalAvatar != null)
                    {
                        //build Dictionaries
                        gameObjectPerBoneRemoteAvatar.Add(bone, boneTransformRemoteAvatar.gameObject);
                        gameObjectPerBoneLocalAvatar.Add(bone, boneTransformLocalAvatar.gameObject);

                        Quaternion tmp = new Quaternion();
                        tmp = boneTransformRemoteAvatar.localRotation;
                        orientationPerBoneRemoteAvatarAtStart.Add(bone, tmp);

                        bonesPerGameObjectRemoteAvatar.Add(boneTransformRemoteAvatar.gameObject, bone);

                        AssignRigidbodys(bone);

                        if (!useJoints)
                        {
                            AssignPDController(bone);
                        }

                        //AssignVacuumBreatherPIDController(bone);
                        //AssignMerchVRPIDController(bone);
                    }
                }
            }

            bodyGroupsRemote = new BodyGroups(gameObjectPerBoneRemoteAvatar);
            bodyGroupsTarget = new BodyGroups(gameObjectPerBoneLocalAvatar);

            if (useJoints)
            {
                configJointManager = GetComponent<ConfigJointManager>();
                configJointManager.SetFixedJoints();
                configJointManager.SetupJoints();
                SetupOrder(gameObjectPerBoneRemoteAvatar[HumanBodyBones.Hips].transform);
                bonesInOrder.Reverse();
            }
        }
    }

    /// <summary>
    ///     A method to return the Rigidbody of the GameObject that corresponds to a certain bodypart. 
    ///     Use this to gain access to the velocity of the bodypart.
    /// </summary>
    Rigidbody GetRigidbodyFromBone(bool fromAvatar, HumanBodyBones boneID)
    {
        GameObject obj;
        if ((fromAvatar ? gameObjectPerBoneRemoteAvatar : gameObjectPerBoneLocalAvatar).TryGetValue(boneID, out obj))
        {
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                return rb;
            }
            else
            {
                Debug.Log("No rigidbody is assigned to the bone " + boneID + "\nMake sure to run AvatarManager.Initialize first.");
                return null;
            }
        }
        else
        {
            Debug.Log("No object is assigned to the bone " + boneID);
            return null;
        }
    }

    void AssignRigidbodys(HumanBodyBones bone)
    {
        gameObjectPerBoneRemoteAvatar[bone].AddComponent<Rigidbody>();
        gameObjectPerBoneRemoteAvatar[bone].GetComponent<Rigidbody>().useGravity = false;

        gameObjectPerBoneLocalAvatar[bone].AddComponent<Rigidbody>();
        gameObjectPerBoneLocalAvatar[bone].GetComponent<Rigidbody>().useGravity = false;
    }

    /*
    void AssignMerchVRPIDController(HumanBodyBones bone)
    {
        gameObjectPerBoneRemoteAvatar[bone].AddComponent<PIDControllerCombined>();
        gameObjectPerBoneRemoteAvatar[bone].GetComponent<PIDControllerCombined>().pidPosition = new PIDControllerPos(gameObjectPerBoneRemoteAvatar[bone], gameObjectPerBoneRemoteAvatar[bone], 38, 5, 8, new Vector3(0, 1, 0));
        gameObjectPerBoneRemoteAvatar[bone].GetComponent<PIDControllerCombined>().pidRotation = new PIDControllerRot(gameObjectPerBoneRemoteAvatar[bone], 50, 5, 10);
        gameObjectPerBoneRemoteAvatar[bone].GetComponent<PIDControllerCombined>().pidVelocity = new PIDControllerVel(gameObjectPerBoneRemoteAvatar[bone], 35, 0, 0.6f, new Vector3(1, 0, 1), 100);
    }
    */

    void AssignPDController(HumanBodyBones bone)
    {
        PDController pd = gameObjectPerBoneRemoteAvatar[bone].AddComponent<PDController>();
        pd.rigidbody = gameObjectPerBoneRemoteAvatar[bone].GetComponent<Rigidbody>();
        pd.oldVelocity = gameObjectPerBoneRemoteAvatar[bone].GetComponent<PDController>().rigidbody.velocity;
        pd.proportionalGain = PDKp;
        pd.derivativeGain = PDKd;
    }
    void AssignVacuumBreatherPIDController(HumanBodyBones bone)
    {
        gameObjectPerBoneRemoteAvatar[bone].AddComponent<VacuumBreather.ControlledObject>();
        gameObjectPerBoneRemoteAvatar[bone].GetComponent<VacuumBreather.ControlledObject>().Kp = Kp;
        gameObjectPerBoneRemoteAvatar[bone].GetComponent<VacuumBreather.ControlledObject>().Ki = Ki;
        gameObjectPerBoneRemoteAvatar[bone].GetComponent<VacuumBreather.ControlledObject>().Kd = Kd;
    }

    Dictionary<HumanBodyBones, GameObject> SafeCopyOfRemoteAvatarDictionary()
    {
        //Dictionary<HumanBodyBones, GameObject> copy = gameObjectPerBoneRemoteAvatar.ToDictionary(k => k.Key, k => k.Value);
        //return copy;
        return new Dictionary<HumanBodyBones, GameObject>(gameObjectPerBoneRemoteAvatar);
        /*
        List<JointTransformContainer> jointTransformContainers = new List<JointTransformContainer>();
        JointTransformContainer container;
        foreach (HumanBodyBones bone in gameObjectPerBoneTarget.Keys)
        {
            container = new JointTransformContainer(bone, gameObjectPerBoneTarget[bone].transform);
            jointTransformContainers.Add(container);
        }
        return jointTransformContainers;  
        */

        /*
        Dictionary<HumanBodyBones, GameObject> tmp = new Dictionary<HumanBodyBones, GameObject>();
        foreach(HumanBodyBones bone in gameObjectPerBoneTarget.Keys)
        {
            tmp.Add(bone, gameObjectPerBoneTarget[bone]);
        }
        return tmp;
        */

    }

    void UpdatePDControllers()
    {
        foreach (HumanBodyBones bone in gameObjectPerBoneRemoteAvatar.Keys)
        {
            UpdateSpecificPDController(bone);
        }
    }

    void UpdateSpecificPDController(HumanBodyBones bone)
    {
        Rigidbody targetRb = GetRigidbodyFromBone(false, bone);
        if (targetRb != null)
        {
            gameObjectPerBoneRemoteAvatar[bone].GetComponent<PDController>().SetDestination(gameObjectPerBoneLocalAvatar[bone].transform, targetRb.velocity);
        }
    }

    void UpdateVacuumBreatherPIDControllers()
    {
        foreach (HumanBodyBones bone in gameObjectPerBoneRemoteAvatar.Keys)
        {
            gameObjectPerBoneRemoteAvatar[bone].GetComponent<VacuumBreather.ControlledObject>().DesiredOrientation = gameObjectPerBoneLocalAvatar[bone].transform.rotation;
        }
    }

    /*void UpdateMerchVRPIDControllers()
    {
        foreach (HumanBodyBones bone in gameObjectPerBoneRemoteAvatar.Keys)
        {
            gameObjectPerBoneRemoteAvatar[bone].GetComponent<PIDControllerCombined>().pidPosition.UpdateTarget(gameObjectPerBoneTarget[bone].transform.position);
            gameObjectPerBoneRemoteAvatar[bone].GetComponent<PIDControllerCombined>().pidRotation.UpdateTarget(gameObjectPerBoneTarget[bone].transform.rotation);
            gameObjectPerBoneRemoteAvatar[bone].GetComponent<PIDControllerCombined>().pidVelocity.UpdateTarget(gameObjectPerBoneTarget[bone].GetComponent<Rigidbody>().velocity, 1);
        }
    }
    */


    void UpdateJoints()
    {
        //Transform rootBone = gameObjectPerBoneRemoteAvatar[HumanBodyBones.Hips].transform;
        //UpdateJointsRecursive(rootBone);
        foreach (HumanBodyBones bone in gameObjectPerBoneRemoteAvatar.Keys)
        {
            //gameObjectPerBoneRemoteAvatar[bone].GetComponent<Rigidbody>().freezeRotation = false;
            Rigidbody targetRigidbody = gameObjectPerBoneLocalAvatar[bone].GetComponent<Rigidbody>();
            configJointManager.SetTagetTransform(bone, gameObjectPerBoneLocalAvatar[bone].transform);
        }
    }

    void SetupOrder(Transform boneTransform)
    {
        HumanBodyBones tmpBone;
        if (bonesPerGameObjectRemoteAvatar.TryGetValue(boneTransform.gameObject, out tmpBone))
        {

            GameObject tmp;
            if (gameObjectPerBoneRemoteAvatar.TryGetValue(tmpBone, out tmp))
            {

                if (tmp.GetComponent<ConfigurableJoint>() != null)
                {
                    Rigidbody targetRb = GetRigidbodyFromBone(false, tmpBone);
                    if (targetRb != null)
                    {
                        bonesInOrder.Add(bonesPerGameObjectRemoteAvatar[boneTransform.gameObject]);
                    }
                }
            }

        }
        foreach (Transform child in boneTransform)
        {
            SetupOrder(child);
        }
    }

    void UpdateJointsRecursive(Transform boneTransform)
    {
        foreach (HumanBodyBones bone in bonesInOrder)
        {
            configJointManager.SetTagetTransform(bone, gameObjectPerBoneLocalAvatar[bone].transform);
        }
    }
    /*
    void WaitUntilRotationComplete(HumanBodyBones bone, Quaternion rotation)
    {
        if (gameObjectPerBoneRemoteAvatar[bone].transform.rotation.x != rotation.x && gameObjectPerBoneRemoteAvatar[bone].transform.rotation.y != rotation.y && gameObjectPerBoneRemoteAvatar[bone].transform.rotation.z != rotation.z)
        {
            WaitUntilRotationComplete(bone, rotation);
        }
        else
        {
            gameObjectPerBoneRemoteAvatar[bone].GetComponent<ConfigurableJoint>().connectedBody.freezeRotation = false;
        }
    }
    */

    public void RecalculateStartOrientations()
    {
        foreach (HumanBodyBones bone in gameObjectPerBoneRemoteAvatar.Keys)
        {
            Transform boneTransformLocalAvatar = animatorLocalAvatar.GetBoneTransform(bone);
            Quaternion tmp = new Quaternion();
            tmp = boneTransformLocalAvatar.rotation;
            orientationPerBoneRemoteAvatarAtStart[bone] = tmp;
            configJointManager.SetStartOrientation();
        }
    }

    public Dictionary<HumanBodyBones, GameObject> GetGameObjectPerBoneRemoteAvatarDictionary()
    {
        return gameObjectPerBoneRemoteAvatar;
    }

    public Dictionary<HumanBodyBones, Quaternion> GetGameObjectPerBoneRemoteAvatarDictionaryAtStart()
    {
        return orientationPerBoneRemoteAvatarAtStart;
    }

    public Dictionary<HumanBodyBones, GameObject> GetGameObjectPerBoneLocalAvatarDictionary()
    {
        return gameObjectPerBoneLocalAvatar;
    }

    public BodyGroups GetBodyGroupsRemote()
    {
        return bodyGroupsRemote;
    }
    public BodyGroups GetBodyGroupsTarget()
    {
        return bodyGroupsTarget;
    }

    public BodyGroups.BODYGROUP GetSelectedBodyGroup()
    {
        return bodyGroup;
    }

    public void LockAvatarJointsExceptCurrent(ConfigurableJoint joint)
    {
        configJointManager.LockAvatarJointsExceptCurrent(joint);
    }

    public List<HumanBodyBones> GetFixedJoints()
    {
        if (useJoints)
        {
            return configJointManager.GetFixedJoints();
        }
        else
        {
            throw new Exception("You are trying to access the ConfigurableJoints, but useJoints is set to false");
        }
    }
}
