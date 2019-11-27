using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestManager : MonoBehaviour
{
    public JointDrive xDrive = new JointDrive();
    public JointDrive yDrive = new JointDrive();
    public JointDrive zDrive = new JointDrive();

    public JointDrive angularXDrive = new JointDrive();
    public JointDrive angularYZDrive = new JointDrive();

    public bool useBodyMass = true;
    public float weight = 72;
    Animator animatorRemoteAvatar;
    Animator animatorTarget;

    Dictionary<HumanBodyBones, GameObject> gameObjectPerBoneTestAvatar = new Dictionary<HumanBodyBones, GameObject>();
    Dictionary<HumanBodyBones, GameObject> gameObjectPerBoneTarget = new Dictionary<HumanBodyBones, GameObject>();

    List<HumanBodyBones> useFixedJoints = new List<HumanBodyBones>();
    // Use this for initialization
    void Start()
    {
        useFixedJoints.Add(HumanBodyBones.Spine);
        useFixedJoints.Add(HumanBodyBones.UpperChest);
        useFixedJoints.Add(HumanBodyBones.LeftShoulder);
        useFixedJoints.Add(HumanBodyBones.RightShoulder);
        useFixedJoints.Add(HumanBodyBones.Neck);

        animatorRemoteAvatar = GetComponentInChildren<Animator>();
        animatorTarget = GameObject.FindGameObjectWithTag("Target").GetComponent<Animator>();
        InitializeBodyStructures();
    }

    void InitializeBodyStructures()
    {
        foreach (HumanBodyBones bone in System.Enum.GetValues(typeof(HumanBodyBones)))
        {
            //LastBone is not mapped to a bodypart, we need to skip it.
            if (bone != HumanBodyBones.LastBone && !useFixedJoints.Contains(bone))
            {
                Transform boneTransformAvatar = animatorRemoteAvatar.GetBoneTransform(bone);
                Transform boneTransformTarget = animatorTarget.GetBoneTransform(bone);
                //We have to skip unassigned bodyparts.
                if (boneTransformAvatar != null && boneTransformTarget != null)
                {
                    //build Dictionaries
                    gameObjectPerBoneTestAvatar.Add(bone, boneTransformAvatar.gameObject);
                    gameObjectPerBoneTarget.Add(bone, boneTransformTarget.gameObject);
                    
                    ConfigJointMotionHandler test = gameObjectPerBoneTestAvatar[bone].AddComponent<ConfigJointMotionHandler>();
                    if (test != null)
                    {
                        test.target = boneTransformTarget.gameObject;
                    }
                }
            }
        }

        if (useBodyMass)
        {
            BodyMass bm = new BodyMass(weight, gameObjectPerBoneTestAvatar);
        }
    }

    void SetJoint(HumanBodyBones bone)
    {
        //Save original position
        foreach (ConfigurableJoint joint in gameObjectPerBoneTestAvatar[bone].GetComponents<ConfigurableJoint>())
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
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.Chest].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftLowerArm:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.LeftUpperArm].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftHand:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.LeftLowerArm].GetComponent<Rigidbody>());
                    break;
                #region Left Hand
                //Left Thumb
                case HumanBodyBones.LeftThumbProximal:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.LeftHand].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftThumbIntermediate:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.LeftThumbProximal].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftThumbDistal:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.LeftThumbIntermediate].GetComponent<Rigidbody>());
                    break;

                //Left Index Finger
                case HumanBodyBones.LeftIndexProximal:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.LeftHand].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftIndexIntermediate:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.LeftIndexProximal].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftIndexDistal:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.LeftIndexIntermediate].GetComponent<Rigidbody>());
                    break;

                //Left Middle Finger
                case HumanBodyBones.LeftMiddleProximal:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.LeftHand].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftMiddleIntermediate:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.LeftMiddleProximal].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftMiddleDistal:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.LeftMiddleIntermediate].GetComponent<Rigidbody>());
                    break;

                //Left Ring Finger
                case HumanBodyBones.LeftRingProximal:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.LeftLowerArm].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftRingIntermediate:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.LeftRingProximal].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftRingDistal:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.LeftRingIntermediate].GetComponent<Rigidbody>());
                    break;

                //Left Little Finger
                case HumanBodyBones.LeftLittleProximal:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.LeftHand].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftLittleIntermediate:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.LeftLittleProximal].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftLittleDistal:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.LeftLittleIntermediate].GetComponent<Rigidbody>());
                    break;
                #endregion
                #endregion

                #region Right Arm

                case HumanBodyBones.RightUpperArm:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.Chest].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightLowerArm:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.RightUpperArm].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightHand:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.RightLowerArm].GetComponent<Rigidbody>());
                    break;

                #region Right Hand

                //Right Thumb
                case HumanBodyBones.RightThumbProximal:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.RightHand].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightThumbIntermediate:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.RightThumbProximal].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightThumbDistal:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.RightThumbIntermediate].GetComponent<Rigidbody>());
                    break;

                //Right Index Finger
                case HumanBodyBones.RightIndexProximal:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.RightHand].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightIndexIntermediate:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.RightIndexProximal].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightIndexDistal:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.RightIndexIntermediate].GetComponent<Rigidbody>());
                    break;

                //Right Middle Finger
                case HumanBodyBones.RightMiddleProximal:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.RightHand].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightMiddleIntermediate:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.RightMiddleProximal].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightMiddleDistal:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.RightMiddleIntermediate].GetComponent<Rigidbody>());
                    break;

                //Right Ring Finger
                case HumanBodyBones.RightRingProximal:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.RightLowerArm].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightRingIntermediate:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.RightRingProximal].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightRingDistal:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.RightRingIntermediate].GetComponent<Rigidbody>());
                    break;

                //Right Little Finger
                case HumanBodyBones.RightLittleProximal:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.RightHand].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightLittleIntermediate:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.RightLittleProximal].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightLittleDistal:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.RightLittleIntermediate].GetComponent<Rigidbody>());
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
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.Chest].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.UpperChest:
                    break;
                case HumanBodyBones.Chest:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.Hips].GetComponent<Rigidbody>());
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
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.Hips].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftLowerLeg:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.LeftUpperLeg].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftFoot:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.LeftLowerLeg].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.LeftToes:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.LeftFoot].GetComponent<Rigidbody>());
                    break;
                #endregion

                #region Right Leg
                case HumanBodyBones.RightUpperLeg:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.Hips].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightLowerLeg:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.RightUpperLeg].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightFoot:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.RightLowerLeg].GetComponent<Rigidbody>());
                    break;
                case HumanBodyBones.RightToes:
                    ConfigureJoint(joint, gameObjectPerBoneTestAvatar[HumanBodyBones.RightFoot].GetComponent<Rigidbody>());
                    break;
                #endregion

                default: break;
            }
        }
    }

    public void ConfigureJoint(ConfigurableJoint joint, Rigidbody connectedBody) { 

        //TODO: DRIVE VALUES

        joint.xDrive = xDrive;
        joint.yDrive = yDrive;
        joint.zDrive = zDrive;

        joint.angularXDrive = angularXDrive;
        joint.angularYZDrive = angularYZDrive;

        //Connected Body
        joint.connectedBody = connectedBody;
    }
}
