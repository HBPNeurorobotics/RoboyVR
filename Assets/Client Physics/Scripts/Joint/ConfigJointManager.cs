using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigJointManager : MonoBehaviour
{
    bool useMultipleTemplate;
    bool splitJointTemplate;

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
    Dictionary<HumanBodyBones, Quaternion> quaternionFromBoneAtStart = new Dictionary<HumanBodyBones, Quaternion>();
    Dictionary<HumanBodyBones, GameObject> templateFromBone = new Dictionary<HumanBodyBones, GameObject>();

    Animator templateAnimator;
    /// <summary>
    /// Assigns ConfigurableJoints configured by the AvatarManager
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="angX"></param>
    /// <param name="angYZ"></param>
    /// <param name="useIndividualAxes"></param>
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

        SetupJoints();

    }
    /// <summary>
    /// Assigns ConfigurableJoints configured by the editor
    /// </summary>
    /// <param name="useIndividualAxes"></param>
    public ConfigJointManager()
    {
        SetupJoints();
    }



    void SetupJoints()
    {
        GetAvatar();

        if (useMultipleTemplate)
        {
            //jointAngleLimits = ReadJointAngleLimitsFromJson();
        }
        else
        {
            InitTemplateDict();
        }

        JointSetup setup = new JointSetup(gameObjectsFromBone, templateFromBone, avatarManager, this);

    }
    /// <summary>
    /// Finds the GameObject tagged as avatar and assigns the AvatarManager reference
    /// </summary>
    void GetAvatar()
    {
        avatarManager = GameObject.FindGameObjectWithTag("Avatar").GetComponent<AvatarManager>();
        gameObjectsFromBone = avatarManager.GetGameObjectPerBoneAvatarDictionary();
        splitJointTemplate = avatarManager.ShouldSplitJoint();

        this.useMultipleTemplate = avatarManager.UseMultipleJointTemplate();
        if (useMultipleTemplate)
        {
            //templateAnimator = GameObject.FindGameObjectWithTag("TemplateMultiple").GetComponent<Animator>();
        }
        else
        {
            templateAnimator = GameObject.FindGameObjectWithTag("Template").GetComponent<Animator>();
        }

    }
  
    public void setMassOfBone(HumanBodyBones bone, float mass = 1)
    {
    }

    public void SetTagetTransform(HumanBodyBones bone, Transform target)
    {
        if (!usesFixedJoint.Contains(bone) && gameObjectsFromBone.ContainsKey(bone) && gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>() != null)
        {
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
        /*
         * if (useIndividualAxes)
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
        */
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
                                    quaternionFromBoneAtStart[bone] *
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

    public void SetStartOrientation()
    {
        quaternionFromBoneAtStart = avatarManager.GetGameObjectPerBoneRemoteAvatarDictionaryAtStart();
    }
}