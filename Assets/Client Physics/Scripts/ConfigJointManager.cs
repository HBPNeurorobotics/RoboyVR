using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigJointManager : MonoBehaviour
{
    bool useIndividualAxes;

    List<HumanBodyBones> noJoints = new List<HumanBodyBones>();
    Dictionary<HumanBodyBones, Transform> originalJointTransforms = new Dictionary<HumanBodyBones, Transform>();
    SoftJointLimit jointLimit = new SoftJointLimit();

    [Header("Position Drives")]
    public JointDrive xDrive;
    public JointDrive yDrive;
    public JointDrive zDrive;

    [Header("Angular Drives")]
    public JointDrive angularXDrive;
    public JointDrive angularYZDrive;

    AvatarManager avatarManager;
    Dictionary<HumanBodyBones, GameObject> gameObjectsFromBone = new Dictionary<HumanBodyBones, GameObject>();
    Dictionary<HumanBodyBones, GameObject> templateFromBone = new Dictionary<HumanBodyBones, GameObject>();

    Animator templateAnimator;

    public ConfigJointManager(JointDrive x, JointDrive y, JointDrive z, JointDrive angX, JointDrive angYZ, bool useIndividualAxes)
    {
        //noJoints.Add(HumanBodyBones.Hips);
        noJoints.Add(HumanBodyBones.Spine);
        noJoints.Add(HumanBodyBones.UpperChest);
        noJoints.Add(HumanBodyBones.LeftShoulder);
        noJoints.Add(HumanBodyBones.RightShoulder);
        noJoints.Add(HumanBodyBones.Neck);

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
        }
    }

    public void SetupJoints()
    {
        GetAvatar();
        InitTemplateDict();
        foreach (HumanBodyBones bone in gameObjectsFromBone.Keys)
        {
            if (!noJoints.Contains(bone))
            {
                AssignJoint(bone);
                //BodyMass bm = new BodyMass(72, avatarManager);
                SetJointFromTemplate(bone);
                SetJoint(bone);
            }
        }

    }

    void GetAvatar()
    {
        avatarManager = GameObject.FindGameObjectWithTag("Avatar").GetComponent<AvatarManager>();
        gameObjectsFromBone = avatarManager.GetGameObjectPerBoneAvatarDictionary();
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
                    }
                }
            }
        }
    }

    void SetJoint(HumanBodyBones bone)
    {
        //Save original position
        originalJointTransforms.Add(bone, gameObjectsFromBone[bone].transform);
        foreach (ConfigurableJoint joint in gameObjectsFromBone[bone].GetComponents<ConfigurableJoint>())
        {
            
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            
            joint.angularXMotion = ConfigurableJointMotion.Free;
            joint.angularYMotion = ConfigurableJointMotion.Free;
            joint.angularZMotion = ConfigurableJointMotion.Free;
            
            joint.configuredInWorldSpace = false;

            joint.enableCollision = false;
            joint.enablePreprocessing = false;

            switch (bone)
            {
                #region Left Arm

                case HumanBodyBones.LeftUpperArm:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.Chest].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftLowerArm:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.LeftUpperArm].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftHand:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.LeftLowerArm].GetComponent<Rigidbody>());
                    break;
                #region Left Hand
                //Left Thumb
                case HumanBodyBones.LeftThumbProximal:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.LeftHand].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftThumbIntermediate:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.LeftThumbProximal].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftThumbDistal:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.LeftThumbIntermediate].GetComponent<Rigidbody>());
                    break;

                //Left Index Finger
                case HumanBodyBones.LeftIndexProximal:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.LeftHand].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftIndexIntermediate:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.LeftIndexProximal].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftIndexDistal:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.LeftIndexIntermediate].GetComponent<Rigidbody>());
                    break;

                //Left Middle Finger
                case HumanBodyBones.LeftMiddleProximal:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.LeftHand].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftMiddleIntermediate:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.LeftMiddleProximal].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftMiddleDistal:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.LeftMiddleIntermediate].GetComponent<Rigidbody>());
                    break;

                //Left Ring Finger
                case HumanBodyBones.LeftRingProximal:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.LeftLowerArm].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftRingIntermediate:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.LeftRingProximal].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftRingDistal:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.LeftRingIntermediate].GetComponent<Rigidbody>());
                    break;

                //Left Little Finger
                case HumanBodyBones.LeftLittleProximal:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.LeftHand].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftLittleIntermediate:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.LeftLittleProximal].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftLittleDistal:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.LeftLittleIntermediate].GetComponent<Rigidbody>());
                    break;
                #endregion
                #endregion

                #region Right Arm

                case HumanBodyBones.RightUpperArm:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.Chest].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightLowerArm:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.RightUpperArm].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightHand:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.RightLowerArm].GetComponent<Rigidbody>());
                    break;

                #region Right Hand

                //Right Thumb
                case HumanBodyBones.RightThumbProximal:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.RightHand].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightThumbIntermediate:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.RightThumbProximal].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightThumbDistal:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.RightThumbIntermediate].GetComponent<Rigidbody>());
                    break;

                //Right Index Finger
                case HumanBodyBones.RightIndexProximal:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.RightHand].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightIndexIntermediate:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.RightIndexProximal].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightIndexDistal:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.RightIndexIntermediate].GetComponent<Rigidbody>());
                    break;

                //Right Middle Finger
                case HumanBodyBones.RightMiddleProximal:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.RightHand].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightMiddleIntermediate:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.RightMiddleProximal].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightMiddleDistal:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.RightMiddleIntermediate].GetComponent<Rigidbody>());
                    break;

                //Right Ring Finger
                case HumanBodyBones.RightRingProximal:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.RightLowerArm].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightRingIntermediate:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.RightRingProximal].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightRingDistal:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.RightRingIntermediate].GetComponent<Rigidbody>());
                    break;

                //Right Little Finger
                case HumanBodyBones.RightLittleProximal:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.RightHand].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightLittleIntermediate:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.RightLittleProximal].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightLittleDistal:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.RightLittleIntermediate].GetComponent<Rigidbody>());
                    break;
                #endregion
                #endregion

                #region Torso

                case HumanBodyBones.LeftShoulder:
                    break;
                case HumanBodyBones.RightShoulder:
                    break;
                case HumanBodyBones.Neck:
                    break;
                case HumanBodyBones.Head:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.Chest].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.UpperChest:
                    break;
                case HumanBodyBones.Chest:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.Hips].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.Spine:
                    break;
                case HumanBodyBones.Hips:
                    Rigidbody rb = GameObject.FindGameObjectWithTag("Anchor").GetComponent<Rigidbody>();
                    ConfigureJoint(joint, rb);
                    break;


                #endregion

                #region Left Leg
                case HumanBodyBones.LeftUpperLeg:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.Hips].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftLowerLeg:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.LeftUpperLeg].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftFoot:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.LeftLowerLeg].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftToes:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.LeftFoot].GetComponent<Rigidbody>());
                    break;
                #endregion

                #region Right Leg
                case HumanBodyBones.RightUpperLeg:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.Hips].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightLowerLeg:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.RightUpperLeg].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightFoot:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.RightLowerLeg].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightToes:
                    ConfigureJoint(joint, gameObjectsFromBone[HumanBodyBones.RightFoot].GetComponent<Rigidbody>());
                    break;
                #endregion

                default: break;
            }
        }
    }

    public void ConfigureJoint(ConfigurableJoint joint, Rigidbody connectedBody) {//, Vector3 anchor, Vector3 axis, Vector3 secondaryAxis, float lowAngularXLimit, float highAngularXLimit, float angularYLimit, float angularZLimit, float mass = 1)

        //TODO: DRIVE VALUES

        joint.xDrive = xDrive;
        joint.yDrive = yDrive;
        joint.zDrive = zDrive;

        joint.angularXDrive = angularXDrive;
        joint.angularYZDrive = angularYZDrive;

        //Connected Body
        joint.connectedBody = connectedBody;
    }

    public void setMassOfBone(HumanBodyBones bone, float mass = 1)
    {
    }

    public void SetTagetTransform(HumanBodyBones bone, Transform target, Vector3 targetVelocity, Vector3 targetAngularVelocity)
    {
        if (!noJoints.Contains(bone) && gameObjectsFromBone.ContainsKey(bone) && gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>() != null)
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

        // description of the joint space
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
        /* turn joint space to align with world
        * perform rotation in world
        * turn joint back into joint space
       */
        Quaternion resultRotation = Quaternion.Inverse(worldToJointSpace) *
                                    Quaternion.Inverse(targetRotation) *
                                    originalJointTransforms[bone].localRotation *
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