using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigJointManager : MonoBehaviour
{
    public bool configureJointsInEditor = true;
    public bool useBodyMass = false;
    bool useBodyMassPrev;
    [Header("Split Axis")]
    public bool useJointsMultipleTemplate = false;
    public bool splitJointTemplate = false;
    private bool splitJointTemplatePrev;
    [Header("Add Colliders")]
    public bool addSimpleColliders = false;
    private bool simpleCollidersPrev;
    public bool addMeshColliders = true;
    private bool meshCollidersPrev;

    public bool inputByManager = false;

    List<HumanBodyBones> usesLockedJoint = new List<HumanBodyBones>();

    public float maximumForce = 10000;

    [Header("Joint Angular Drive X")]
    public float springAngularX = 2500;
    public float damperAngularX = 500;
    [Header("Joint Angular Drive YZ")]
    public float springAngularYZ = 2500;
    public float damperAngularYZ = 500;

    JointDrive angularXDrive = new JointDrive();
    JointDrive angularYZDrive = new JointDrive();

    AvatarManager avatarManager;
    public JointSetup jointSetup;

    Dictionary<HumanBodyBones, GameObject> gameObjectsFromBone = new Dictionary<HumanBodyBones, GameObject>();
    Dictionary<HumanBodyBones, Quaternion> quaternionFromBoneAtStart = new Dictionary<HumanBodyBones, Quaternion>();
    Dictionary<HumanBodyBones, GameObject> templateFromBone = new Dictionary<HumanBodyBones, GameObject>();

    Animator templateAnimator;

    void Start()
    {
        meshCollidersPrev = addMeshColliders;
        simpleCollidersPrev = addSimpleColliders;
        splitJointTemplatePrev = splitJointTemplate;
        useBodyMassPrev = useBodyMass;

        angularXDrive.maximumForce = angularYZDrive.maximumForce = maximumForce;

        angularXDrive.positionSpring = springAngularX;
        angularXDrive.positionDamper = damperAngularX;

        angularYZDrive.positionSpring = springAngularYZ;
        angularYZDrive.positionDamper = damperAngularYZ;

        SetFixedJoints();

    }

    void FixedUpdate()
    {
        if (addMeshColliders != meshCollidersPrev)
        {
            jointSetup.ToggleMeshColliders(addMeshColliders);
        }
        else
        {
            if (addSimpleColliders != simpleCollidersPrev)
            {
                jointSetup.ToggleSimpleColliders(addSimpleColliders);
            }
        }
        /*
        if(!useJointsMultipleTemplate && (splitJointTemplate != splitJointTemplatePrev))
        {
            jointSetup.ToggleSplitJoints(splitJointTemplate);
            avatarManager.RecalculateStartOrientations();
        }
        */

        if(useBodyMass != useBodyMassPrev)
        {
            jointSetup.ToggleBodyMass(useBodyMass);
        }

        meshCollidersPrev = addMeshColliders;
        simpleCollidersPrev = addSimpleColliders;
        splitJointTemplatePrev = splitJointTemplate;
        useBodyMassPrev = useBodyMass;
    }

    void UpdateGameObjectsFromBone()
    {/*
        switch (avatarManager.GetSelectedBodyGroup())
        {
            case BodyGroups.BODYGROUP.ALL_COMBINED: gameObjectsFromBone = avatarManager.GetBodyGroupsRemote().AllCombined(); break;
            case BodyGroups.BODYGROUP.HEAD: gameObjectsFromBone = avatarManager.GetBodyGroupsRemote().Head(); break;
            case BodyGroups.BODYGROUP.LEFT_ARM: gameObjectsFromBone = avatarManager.GetBodyGroupsRemote().LeftArm(); break;
            case BodyGroups.BODYGROUP.LEFT_FOOT: gameObjectsFromBone = avatarManager.GetBodyGroupsRemote().LeftFoot(); break;
            case BodyGroups.BODYGROUP.LEFT_HAND: gameObjectsFromBone = avatarManager.GetBodyGroupsRemote().LeftHand(); break;
            case BodyGroups.BODYGROUP.LEFT_LEG: gameObjectsFromBone = avatarManager.GetBodyGroupsRemote().LeftLeg(); break;
            case BodyGroups.BODYGROUP.RIGHT_ARM: gameObjectsFromBone = avatarManager.GetBodyGroupsRemote().RightArm(); break;
            case BodyGroups.BODYGROUP.RIGHT_FOOT: gameObjectsFromBone = avatarManager.GetBodyGroupsRemote().RightFoot(); break;
            case BodyGroups.BODYGROUP.RIGHT_HAND: gameObjectsFromBone = avatarManager.GetBodyGroupsRemote().RightHand(); break;
            case BodyGroups.BODYGROUP.RIGHT_LEG: gameObjectsFromBone = avatarManager.GetBodyGroupsRemote().RightLeg(); break;
            case BodyGroups.BODYGROUP.TRUNK: gameObjectsFromBone = avatarManager.GetBodyGroupsRemote().Trunk(); break;
            case BodyGroups.BODYGROUP.TRUNK_HEAD: gameObjectsFromBone = avatarManager.GetBodyGroupsRemote().TrunkHead(); break;
        }
        */
        gameObjectsFromBone = avatarManager.GetGameObjectPerBoneRemoteAvatarDictionary();
    }

    public void SetupJoints()
    {
        //We only support the construction of individual joints from a single one or from the multijoint template but not both at the same time
        if (splitJointTemplate) useJointsMultipleTemplate = false;
        if (useJointsMultipleTemplate) splitJointTemplate = false;

        //
        if (addSimpleColliders) addMeshColliders = false;
        if (addMeshColliders) addSimpleColliders = false;

        GetAvatar();
        InitTemplateDict();

        jointSetup = new JointSetup(gameObjectsFromBone, templateFromBone, this);
        jointSetup.InitializeStructures();

    }
    /// <summary>
    /// Finds the GameObject tagged as avatar and assigns the AvatarManager reference
    /// </summary>
    void GetAvatar()
    {
        avatarManager = GameObject.FindGameObjectWithTag("Avatar").GetComponent<AvatarManager>();
        gameObjectsFromBone = avatarManager.GetGameObjectPerBoneRemoteAvatarDictionary();

        if (useJointsMultipleTemplate)
        {
            templateAnimator = GameObject.FindGameObjectWithTag("TemplateMultiple").GetComponent<Animator>();
        }
        else
        {
            templateAnimator = GameObject.FindGameObjectWithTag("Template").GetComponent<Animator>();
        }

    }

    public void setMassOfBone(HumanBodyBones bone, float mass = 1)
    {
    }

    public void SetTagetRotation(HumanBodyBones bone, Quaternion targetRotation)
    {
        if (!usesLockedJoint.Contains(bone) && gameObjectsFromBone.ContainsKey(bone) && gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>() != null)
        {
            ConfigurableJoint[] joints = gameObjectsFromBone[bone].GetComponents<ConfigurableJoint>();

            for (int i = 0; i < joints.Length; i++)
            {
                joints[i].targetRotation = CalculateJointRotation(joints[i], bone, targetRotation);
            }
            //gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().targetPosition = originalJointTransforms[bone].position - target.position;
        }
    }

    /// <summary>
    /// Calculates the desired target rotation in joint space (based on https://gist.github.com/mstevenson/4958837)
    /// </summary>
    /// <param name="joint"></param>
    /// <param name="bone"></param>
    /// <param name="targetRotation"></param>
    /// <returns></returns>
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
        

        //description of the joint space
        //the x axis of the joint space
        Vector3 jointXAxis = joint.axis.normalized;
        // the y axis of the joint space
        Vector3 jointYAxis = Vector3.Cross(joint.axis, joint.secondaryAxis).normalized;
        //the z axis of the joint space
        Vector3 jointZAxis = Vector3.Cross(jointYAxis, jointXAxis).normalized;
        */

        /*
         * Z axis will be aligned with forward
         * X axis aligned with cross product between forward and upwards
         * Y axis aligned with cross product between Z and X.
         * --> rotates world coordinates to align with joint coordinates
        */
        Quaternion worldToJointSpace = LocalPhysicsToolkit.GetWorldToJointRotation(joint);

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

    public JointDrive GetAngularXDrive()
    {
        return angularXDrive;
    }

    public JointDrive GetAngularYZDrive()
    {
        return angularYZDrive;
    }

    public void SetAngularXDrive(JointDrive jointDrive)
    {
        angularXDrive = jointDrive;
    }

    public void SetAngularYZDrive(JointDrive jointDrive)
    {
        angularYZDrive = jointDrive;
    }

    public AvatarManager GetAvatarManager()
    {
        return avatarManager;
    }

    void GetChildRigidbody(Transform parent, List<Rigidbody> rbs)
    {
        foreach(Transform lower in parent)
        {
            Rigidbody rb = lower.gameObject.GetComponent<Rigidbody>();
            if(rb != null)
            {
                rbs.Add(rb);
            }
            GetChildRigidbody(lower, rbs);
        }
    }

    public void LockAvatarJointsExceptCurrent(ConfigurableJoint freeJoint)
    {
        List<Rigidbody> underPhysicsControl = new List<Rigidbody>();
        underPhysicsControl.Add(freeJoint.gameObject.GetComponent<Rigidbody>());
        Transform top = freeJoint.gameObject.transform;
        GetChildRigidbody(top, underPhysicsControl);

        foreach(HumanBodyBones bone in gameObjectsFromBone.Keys)
        {
            
            Rigidbody rb = gameObjectsFromBone[bone].GetComponent<Rigidbody>();

            if(rb != null && !underPhysicsControl.Contains(rb))
            {
                rb.isKinematic = true;
            }
            
            
            if (gameObjectsFromBone[bone].Equals(freeJoint.gameObject)) {
                foreach (ConfigurableJoint joint in gameObjectsFromBone[bone].GetComponents<ConfigurableJoint>())
                {
                    if (freeJoint != joint)
                    {
                        
                        joint.angularXMotion = ConfigurableJointMotion.Locked;
                    }
                    else
                    {
                        joint.angularXMotion = ConfigurableJointMotion.Free;
                    }
                }
            }
        }
    }

    public void UnlockAvatarJoints()
    {
        foreach (HumanBodyBones bone in gameObjectsFromBone.Keys)
        {
            Rigidbody rb = gameObjectsFromBone[bone].GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }

            foreach (ConfigurableJoint joint in gameObjectsFromBone[bone].GetComponents<ConfigurableJoint>())
            {
                joint.angularXMotion = ConfigurableJointMotion.Limited;
            }
        }
    }

    public Dictionary<HumanBodyBones, GameObject> GetTemplateAvatar()
    {
        return templateFromBone;
    }

    public List<HumanBodyBones> GetFixedJoints()
    {
        return usesLockedJoint;
    }

    public void SetFixedJoints()
    {
        if (usesLockedJoint.Count == 0)
        {
            usesLockedJoint.Add(HumanBodyBones.Hips);
            usesLockedJoint.Add(HumanBodyBones.RightShoulder);
            usesLockedJoint.Add(HumanBodyBones.LeftShoulder);
        }
    }

    public ConfigurableJoint GetJointInTemplate(HumanBodyBones bone, Vector3 axis)
    {
        GameObject tmp;
        if(templateFromBone.TryGetValue(bone, out tmp))
        {
            ConfigurableJoint[] joints = tmp.GetComponents<ConfigurableJoint>();
            foreach(ConfigurableJoint joint in joints)
            {
                if(joint.axis == axis)
                {
                    return joint;
                }
            }
        }
        throw new Exception("No joint in template found");
    }
}