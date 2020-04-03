using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(ConfigJointManager))]
[RequireComponent(typeof(BoneMeshContainer))]
public class AvatarManager : MonoBehaviour
{
    public GameObject joints, surface;
    public float height = 1.7855f;
    public bool useJoints = true;
    [NonSerialized]
    public bool tuningInProgress; 
    public bool showLocalAvatar;
    bool initialized;

    [Header("PD Control - Only used when useJoints is unchecked")]
    public float PDKp = 125;
    public float PDKd = 20;

    //could be used to initialize only for specific body parts, e.g. arm.
    BodyGroups.BODYGROUP bodyGroup = BodyGroups.BODYGROUP.ALL_COMBINED;

    BodyGroups bodyGroupsRemote;
    BodyGroups bodyGroupsTarget;

    ConfigJointManager configJointManager;
    LatencyHandler latencyHandler;

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
        latencyHandler = GetComponent<LatencyHandler>();
        //InitializeBodyStructures();
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
            if (initialized)
            {
                if (!tuningInProgress && latencyHandler.latency_ms == 0)
                {
                    UpdateJoints();
                    //UpdateJointsRecursive(gameObjectPerBoneRemoteAvatar[HumanBodyBones.Hips].transform);

                }
            }
        }

        if (Input.GetKey(KeyCode.S))
        {
            InitializeBodyStructures();
        }
        if (!initialized)
        {
            transform.localScale = UserAvatarService.Instance.transform.localScale = height / 1.7855f * Vector3.one;
        }
    }

    /// <summary>
    /// Hides the local avatar from camera view of the VR user. 
    /// </summary>
    /// <param name="toHide"></param>
    void SetInvisibleLocalAvatar(Transform toHide)
    {
        //layer 24 gets ignored by hmd camera
        toHide.gameObject.layer = 24;

        foreach (Transform child in toHide)
        {
            SetInvisibleLocalAvatar(child);
        }
    }

    /// <summary>
    ///     Maps all HumanBodyBones (assigned in the Animator) to their GameObjects in the scene in order to get access to all components.
    ///     Adds Rigidbody to both bodies, adds PDController to the avatar if useJoints is not chosen and initializes a ConfigJointManager otherwise.
    /// </summary>
    public void InitializeBodyStructures()
    {

        if (!initialized)
        {

            //hide local avatar in camera
            if (gameObjectPerBoneLocalAvatar.Keys.Count == 0 && gameObjectPerBoneRemoteAvatar.Keys.Count == 0)
            {
                animatorRemoteAvatar = GetComponentInChildren<Animator>();
                animatorLocalAvatar = GameObject.FindGameObjectWithTag("Target").GetComponent<Animator>();

                if (!showLocalAvatar)
                {
                    SetInvisibleLocalAvatar(animatorLocalAvatar.gameObject.transform);
                }

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
                        }
                    }
                }

                bodyGroupsRemote = new BodyGroups(gameObjectPerBoneRemoteAvatar);
                bodyGroupsTarget = new BodyGroups(gameObjectPerBoneLocalAvatar);

                if (useJoints)
                {
                    configJointManager = GetComponent<ConfigJointManager>();
                    configJointManager.SetupJoints();
                    SetupOrder(gameObjectPerBoneRemoteAvatar[HumanBodyBones.Hips].transform);
                    bonesInOrder.Reverse();
                }
            }
            animatorLocalAvatar.gameObject.GetComponent<UserAvatarIKControl>().coordStartAnchor = gameObjectPerBoneLocalAvatar[HumanBodyBones.Hips].transform.position.y;
            joints.SetActive(true);
            surface.SetActive(true);
            initialized = true;
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

    void AssignPDController(HumanBodyBones bone)
    {
        PDController pd = gameObjectPerBoneRemoteAvatar[bone].AddComponent<PDController>();
        pd.rigidbody = gameObjectPerBoneRemoteAvatar[bone].GetComponent<Rigidbody>();
        pd.oldVelocity = gameObjectPerBoneRemoteAvatar[bone].GetComponent<PDController>().rigidbody.velocity;
        pd.proportionalGain = PDKp;
        pd.derivativeGain = PDKd;
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

    /// <summary>
    /// Sends the target rotation of all joints to the ConfigJointManager
    /// </summary>
    void UpdateJoints()
    {
        foreach (HumanBodyBones bone in gameObjectPerBoneRemoteAvatar.Keys)
        {
            configJointManager.SetTagetRotation(bone, gameObjectPerBoneLocalAvatar[bone].transform.localRotation);
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

    #region getters
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

    public ConfigurableJoint GetJointInTemplate(HumanBodyBones bone, Vector3 axis)
    {
        return configJointManager.GetJointInTemplate(bone, axis);
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

    #endregion

    #region tuning functionalities
    public void LockAvatarJointsExceptCurrent(ConfigurableJoint joint)
    {
        configJointManager.LockAvatarJointsExceptCurrent(joint);
    }

    public void UnlockAvatarJoints()
    {
        configJointManager.UnlockAvatarJoints();
    }

    public bool IsJointUnneeded(string joint)
    {
        ConfigurableJoint remoteJoint = LocalPhysicsToolkit.GetRemoteJointOfCorrectAxisFromString(joint, gameObjectPerBoneRemoteAvatar);
        return remoteJoint.lowAngularXLimit.limit == 0 && remoteJoint.highAngularXLimit.limit == 0;
    }
    #endregion

    #region legacy

    public bool isInitialized()
    {
        return initialized;
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

    void UpdateJointsRecursive(Transform boneTransform)
    {
        foreach (HumanBodyBones bone in bonesInOrder)
        {
            configJointManager.SetTagetRotation(bone, gameObjectPerBoneLocalAvatar[bone].transform.localRotation);
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

    /// <summary>
    /// Sets start orientation when joints have been removed and then readded. Might need to be looked into again.
    /// </summary>
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
    #endregion
}
