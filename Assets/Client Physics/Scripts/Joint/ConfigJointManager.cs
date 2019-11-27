using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigJointManager : MonoBehaviour
{
    bool useIndividualAxes;

    List<HumanBodyBones> usesFixedJoint = new List<HumanBodyBones>();
    [SerializeField]
    Dictionary<HumanBodyBones, JointAngleContainer> jointAngleLimits = new Dictionary<HumanBodyBones, JointAngleContainer>();

    [Header("Position Drives")]
    public JointDrive xDrive;
    public JointDrive yDrive;
    public JointDrive zDrive;

    [Header("Angular Drives")]
    public JointDrive angularXDrive;
    public JointDrive angularYZDrive;

    AvatarManager avatarManager;
    Dictionary<HumanBodyBones, GameObject> gameObjectsFromBone = new Dictionary<HumanBodyBones, GameObject>();
    //TODO: These values should not change at runtime 
    Dictionary<HumanBodyBones, GameObject> gameObjectsFromBoneAtStart = new Dictionary<HumanBodyBones, GameObject>();
    Dictionary<HumanBodyBones, GameObject> templateFromBone = new Dictionary<HumanBodyBones, GameObject>();

    Animator templateAnimator;

    public ConfigJointManager(JointDrive x, JointDrive y, JointDrive z, JointDrive angX, JointDrive angYZ, bool useIndividualAxes)
    {
        usesFixedJoint.Add(HumanBodyBones.Hips);
        usesFixedJoint.Add(HumanBodyBones.Spine);
        usesFixedJoint.Add(HumanBodyBones.UpperChest);
        usesFixedJoint.Add(HumanBodyBones.LeftShoulder);
        usesFixedJoint.Add(HumanBodyBones.RightShoulder);
        usesFixedJoint.Add(HumanBodyBones.Neck);

        xDrive = x;
        yDrive = y;
        zDrive = z;

        angularXDrive = angX;
        angularYZDrive = angYZ;

        this.useIndividualAxes = useIndividualAxes;
        if (useIndividualAxes)
        {
            templateAnimator = GameObject.FindGameObjectWithTag("TemplateIndividual").GetComponent<Animator>();
        }
        else
        {
            templateAnimator = GameObject.FindGameObjectWithTag("Template").GetComponent<Animator>();
        }
    }

    void AssignJoint(HumanBodyBones bone)
    {

        if (gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>() == null)
        {
            gameObjectsFromBone[bone].AddComponent<ConfigurableJoint>();
            if (useIndividualAxes) {
                gameObjectsFromBone[bone].AddComponent<ConfigurableJoint>();
                gameObjectsFromBone[bone].AddComponent<ConfigurableJoint>();
            }
            gameObjectsFromBone[bone].GetComponent<Rigidbody>().useGravity = false;
        }
    }

    public void SetupJoints()
    {
        GetAvatar();
        InitTemplateDict();

        jointAngleLimits = ReadJointAngleLimitsFromFile();

        foreach (HumanBodyBones bone in gameObjectsFromBone.Keys)
        {
            AssignJoint(bone);
            //BodyMass bm = new BodyMass(72, avatarManager);
            SetJointFromTemplate(bone);
        }

    }

    void SetFixedJoint(HumanBodyBones bone)
    {
            switch (bone)
                {
                    case HumanBodyBones.Spine:
                        gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.Hips].GetComponent<Rigidbody>();
                        break;
                    case HumanBodyBones.UpperChest:
                        gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.Chest].GetComponent<Rigidbody>();
                        break;
                    case HumanBodyBones.LeftShoulder:
                        gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.UpperChest].GetComponent<Rigidbody>();
                        break;
                    case HumanBodyBones.RightShoulder:
                        gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.UpperChest].GetComponent<Rigidbody>();
                        break;
                    case HumanBodyBones.Neck:
                        gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.UpperChest].GetComponent<Rigidbody>();
                        break;
                }
    }

    void GetAvatar()
    {
        avatarManager = GameObject.FindGameObjectWithTag("Avatar").GetComponent<AvatarManager>();
        gameObjectsFromBone = avatarManager.GetGameObjectPerBoneAvatarDictionary();
    }

    Dictionary<HumanBodyBones, JointAngleContainer> ReadJointAngleLimitsFromFile()
    {
        TextAsset file = avatarManager.angles;
        string[] lines = file.text.Split('\n');

        Dictionary<HumanBodyBones, JointAngleContainer> jointAngleLimits = new Dictionary<HumanBodyBones, JointAngleContainer>();

        foreach(string line in lines)
        {
            if (line.Length > 0)
            {
                JointAngleContainer container = JsonUtility.FromJson<JointAngleContainer>(line);
                jointAngleLimits.Add(container.bone, container);
            }
        }

        return jointAngleLimits;

    }

    void SetJointFromTemplate(HumanBodyBones bone)
    {
        if (gameObjectsFromBone.ContainsKey(bone) && templateFromBone.ContainsKey(bone))
        {
            if (gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>() != null && templateFromBone[bone].GetComponent<ConfigurableJoint>() != null)
            {
                ConfigurableJoint[] jointsOfRemoteBone = gameObjectsFromBone[bone].GetComponents<ConfigurableJoint>();
                ConfigurableJoint[] jointsOfTemplateBone = templateFromBone[bone].GetComponents<ConfigurableJoint>();                

                if (jointsOfRemoteBone.Length == jointsOfTemplateBone.Length)
                {

                    for (int i = 0; i < jointsOfTemplateBone.Length; i++)
                    {
                        UnityEditorInternal.ComponentUtility.CopyComponent(jointsOfTemplateBone[i]);
                        UnityEditorInternal.ComponentUtility.PasteComponentValues(jointsOfRemoteBone[i]);

                        SetConnectedBody(bone);
                    }
                }
            }
        }
    }

    void SetConnectedBody(HumanBodyBones bone)
    {
        //Save original position
        
        foreach (ConfigurableJoint joint in gameObjectsFromBone[bone].GetComponents<ConfigurableJoint>())
        {
            
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            
            joint.angularXMotion = ConfigurableJointMotion.Limited;
            joint.angularYMotion = ConfigurableJointMotion.Limited;
            joint.angularZMotion = ConfigurableJointMotion.Limited;
            
            joint.configuredInWorldSpace = false;

            joint.enableCollision = false;
            joint.enablePreprocessing = false;

            switch (bone)
            {
                #region Left Arm

                case HumanBodyBones.LeftUpperArm:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.LeftShoulder].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftLowerArm:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.LeftUpperArm].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftHand:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.LeftLowerArm].GetComponent<Rigidbody>());
                    break;
                #region Left Hand
                //Left Thumb
                case HumanBodyBones.LeftThumbProximal:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.LeftHand].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftThumbIntermediate:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.LeftThumbProximal].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftThumbDistal:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.LeftThumbIntermediate].GetComponent<Rigidbody>());
                    break;

                //Left Index Finger
                case HumanBodyBones.LeftIndexProximal:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.LeftHand].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftIndexIntermediate:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.LeftIndexProximal].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftIndexDistal:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.LeftIndexIntermediate].GetComponent<Rigidbody>());
                    break;

                //Left Middle Finger
                case HumanBodyBones.LeftMiddleProximal:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.LeftHand].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftMiddleIntermediate:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.LeftMiddleProximal].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftMiddleDistal:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.LeftMiddleIntermediate].GetComponent<Rigidbody>());
                    break;

                //Left Ring Finger
                case HumanBodyBones.LeftRingProximal:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.LeftHand].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftRingIntermediate:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.LeftRingProximal].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftRingDistal:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.LeftRingIntermediate].GetComponent<Rigidbody>());
                    break;

                //Left Little Finger
                case HumanBodyBones.LeftLittleProximal:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.LeftHand].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftLittleIntermediate:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.LeftLittleProximal].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftLittleDistal:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.LeftLittleIntermediate].GetComponent<Rigidbody>());
                    break;
                #endregion
                #endregion

                #region Right Arm

                case HumanBodyBones.RightUpperArm:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.RightShoulder].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightLowerArm:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.RightUpperArm].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightHand:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.RightLowerArm].GetComponent<Rigidbody>());
                    break;

                #region Right Hand

                //Right Thumb
                case HumanBodyBones.RightThumbProximal:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.RightHand].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightThumbIntermediate:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.RightThumbProximal].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightThumbDistal:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.RightThumbIntermediate].GetComponent<Rigidbody>());
                    break;

                //Right Index Finger
                case HumanBodyBones.RightIndexProximal:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.RightHand].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightIndexIntermediate:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.RightIndexProximal].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightIndexDistal:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.RightIndexIntermediate].GetComponent<Rigidbody>());
                    break;

                //Right Middle Finger
                case HumanBodyBones.RightMiddleProximal:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.RightHand].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightMiddleIntermediate:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.RightMiddleProximal].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightMiddleDistal:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.RightMiddleIntermediate].GetComponent<Rigidbody>());
                    break;

                //Right Ring Finger
                case HumanBodyBones.RightRingProximal:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.RightHand].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightRingIntermediate:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.RightRingProximal].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightRingDistal:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.RightRingIntermediate].GetComponent<Rigidbody>());
                    break;

                //Right Little Finger
                case HumanBodyBones.RightLittleProximal:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.RightHand].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightLittleIntermediate:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.RightLittleProximal].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightLittleDistal:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.RightLittleIntermediate].GetComponent<Rigidbody>());
                    break;
                #endregion
                #endregion

                #region Torso

                case HumanBodyBones.LeftShoulder:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.UpperChest].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightShoulder:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.UpperChest].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.Neck:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.UpperChest].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.Head:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.UpperChest].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.UpperChest:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.Chest].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.Chest:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.Spine].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.Spine:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.Hips].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.Hips:
                    Rigidbody rb = GameObject.FindGameObjectWithTag("Anchor").GetComponent<Rigidbody>();
                    joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = ConfigurableJointMotion.Locked;
                    ConfigureJoint(bone, joint, rb);
                    break;


                #endregion

                #region Left Leg
                case HumanBodyBones.LeftUpperLeg:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.Hips].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftLowerLeg:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.LeftUpperLeg].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftFoot:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.LeftLowerLeg].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftToes:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.LeftFoot].GetComponent<Rigidbody>());
                    break;
                #endregion

                #region Right Leg
                case HumanBodyBones.RightUpperLeg:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.Hips].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightLowerLeg:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.RightUpperLeg].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightFoot:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.RightLowerLeg].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightToes:
                    ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.RightFoot].GetComponent<Rigidbody>());
                    break;
                #endregion

                default: break;
            }
        }
    }

    public void ConfigureJoint(HumanBodyBones bone, ConfigurableJoint joint, Rigidbody connectedBody) {

        //TODO: DRIVE VALUES

        joint.xDrive = xDrive;
        joint.yDrive = yDrive;
        joint.zDrive = zDrive;

        joint.angularXDrive = angularXDrive;
        joint.angularYZDrive = angularYZDrive;

        //Connected Body
        joint.connectedBody = connectedBody;

        //This will only be used if there are no individual rotations/velocities assigned by the AvatarManager
        if (!avatarManager.usesActiveInput())
        {
            if (usesFixedJoint.Contains(bone))
            {
                //joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = ConfigurableJointMotion.Locked;
            }
            else
            {
                AssignTargetToImitatePassive(bone);
            }
        }
        else
        {
            AssignOriginalTransforms(bone);
        }
    }

    void AssignTargetToImitatePassive(HumanBodyBones bone)
    {
        ConfigJointMotionHandler rotationHelper = gameObjectsFromBone[bone].AddComponent<ConfigJointMotionHandler>();
        rotationHelper.target = avatarManager.GetGameObjectPerBoneTargetDictionary()[bone];
    }

    void AssignOriginalTransforms(HumanBodyBones bone)
    {
        gameObjectsFromBoneAtStart = avatarManager.GetGameObjectPerBoneRemoteAvatarDictionaryAtStart();
    }

    public void setMassOfBone(HumanBodyBones bone, float mass = 1)
    {
    }

    public void SetTagetTransform(HumanBodyBones bone, Transform target, Vector3 targetVelocity, Vector3 targetAngularVelocity)
    {
        if (!usesFixedJoint.Contains(bone) && gameObjectsFromBone.ContainsKey(bone) && gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>() != null)
        {
            
            /*
            gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().targetVelocity = targetVelocity;
            gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().targetAngularVelocity = targetAngularVelocity;
            */

            ConfigurableJoint[] joints = gameObjectsFromBone[bone].GetComponents<ConfigurableJoint>();

            for (int i = 0; i < joints.Length; i++)
            {
                joints[i].targetRotation = CalculateJointRotation(joints[i], bone, target.localRotation);
            }
            //gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().targetPosition = originalJointTransforms[bone].position - target.position;
        }
    }

    Quaternion CalculateJointRotation(ConfigurableJoint joint, HumanBodyBones bone, Quaternion targetRotation)
    {
        
        //useIndividualAxes --> treat each joint rotation individually
        if (useIndividualAxes)
        {
            Vector3 isolatedEuler = targetRotation.eulerAngles;
            //rotation around x-Axis -> primaryAxis y and z set to 0
            if (joint.axis.y == 0 && joint.axis.z == 0)
            {
                isolatedEuler.y = isolatedEuler.z = 0;
                if (bone.Equals(HumanBodyBones.RightLowerArm))
                {
                    Debug.Log("Rotation around X " + isolatedEuler);
                }
            }
            //rotation around y-Axis -> primaryAxis x and z set to 0
            else if (joint.axis.x == 0 && joint.axis.z == 0)
            {
                isolatedEuler.x = isolatedEuler.z = 0;
                if (bone.Equals(HumanBodyBones.RightLowerArm))
                {
                    Debug.Log("Rotation around Y " + isolatedEuler);
                }
            }
            //rotation around z-Axis -> primaryAxis x and y set to 0
            else if (joint.axis.x == 0 && joint.axis.y == 0)
            {
                isolatedEuler.x = isolatedEuler.y = 0;
                if (bone.Equals(HumanBodyBones.RightLowerArm))
                {
                    Debug.Log("Rotation around Z " + isolatedEuler);
                }
            }

            targetRotation = Quaternion.Euler(isolatedEuler);
        }

        /*
        //dummy value (no rotation), this will always be changed since the bone will always be present in the list
        Transform transformAtStart = avatarManager.gameObject.transform;
        foreach(JointTransformContainer container in transformsFromBoneAtStart)
        {
            if (container.GetBone().Equals(bone))
            {
                transformAtStart = container.GetStart();
                break;
            }
        }

        if (bone.Equals(HumanBodyBones.RightUpperArm))
        {
            Debug.Log(transformAtStart.localRotation);
        }
        */

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
        Quaternion worldToJointSpace = Quaternion.LookRotation(jointYAxis, jointZAxis);
        /* 
         * turn joint space to align with world
         * perform rotation in world
         * turn joint back into joint space
        */

        Quaternion resultRotation = Quaternion.Inverse(worldToJointSpace) *
                                    Quaternion.Inverse(targetRotation) *
                                    gameObjectsFromBoneAtStart[bone].transform.localRotation *
                                    worldToJointSpace;
        return resultRotation;
    }

    void InitTemplateDict()
    {
        foreach (HumanBodyBones bone in System.Enum.GetValues(typeof(HumanBodyBones)))
        {
            //LastBone is not mapped to a bodypart, we need to skip it.
            if (bone != HumanBodyBones.LastBone)
            {
                Transform boneTransformTemplate = templateAnimator.GetBoneTransform(bone);
                //We have to skip unassigned bodyparts.
                if (boneTransformTemplate != null)
                {
                    if (boneTransformTemplate.gameObject.GetComponent<ConfigurableJoint>() != null)
                    {
                        //build Dictionary
                        templateFromBone.Add(bone, boneTransformTemplate.gameObject);
                    }
                }
            }
        }
    }
}