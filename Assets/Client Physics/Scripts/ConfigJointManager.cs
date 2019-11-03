using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigJointManager {

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

    Animator templateAnimator = GameObject.FindGameObjectWithTag("Template").GetComponent<Animator>();

    public ConfigJointManager(JointDrive x, JointDrive y, JointDrive z, JointDrive angX, JointDrive angYZ)
    {
        noJoints.Add(HumanBodyBones.Hips);
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
    }

       

	

    void AssignJoint(HumanBodyBones bone)
    {

        if (gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>() == null)
        {       
            gameObjectsFromBone[bone].AddComponent<ConfigurableJoint>();
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
        if (gameObjectsFromBone.ContainsKey(bone) && templateFromBone.ContainsKey(bone)) {
            if (gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>() != null && templateFromBone[bone].GetComponent<ConfigurableJoint>() != null)
            {
                UnityEditorInternal.ComponentUtility.CopyComponent(templateFromBone[bone].GetComponent<ConfigurableJoint>());
                UnityEditorInternal.ComponentUtility.PasteComponentValues(gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>());
            }
        }
    }

    void SetJoint(HumanBodyBones bone)
    {
            //Save original position
            originalJointTransforms.Add(bone, gameObjectsFromBone[bone].transform);

            gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().xMotion = ConfigurableJointMotion.Locked;
            gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().yMotion = ConfigurableJointMotion.Locked;
            gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().zMotion = ConfigurableJointMotion.Locked;

            gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().angularXMotion = ConfigurableJointMotion.Limited;
            gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().angularYMotion = ConfigurableJointMotion.Limited;
            gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().angularZMotion = ConfigurableJointMotion.Limited;

            //gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().enableCollision = true;
            //gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().enablePreprocessing = false;

            switch (bone)
            {
                #region Left Arm

                case HumanBodyBones.LeftUpperArm:
                    ConfigureJoint(bone, gameObjectsFromBone[HumanBodyBones.Chest].GetComponent<Rigidbody>(), new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0), -90, 45, 85, 25);
                    break;
                case HumanBodyBones.LeftLowerArm:
                    ConfigureJoint(bone, gameObjectsFromBone[HumanBodyBones.LeftUpperArm].GetComponent<Rigidbody>(), new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1), -177, 0, 15, 15);
                    break;
                case HumanBodyBones.LeftHand:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.LeftLowerArm].GetComponent<Rigidbody>();
                    break;
                #region Left Hand
                //Left Thumb
                case HumanBodyBones.LeftThumbProximal:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.LeftHand].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.LeftThumbIntermediate:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.LeftThumbProximal].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.LeftThumbDistal:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.LeftIndexIntermediate].GetComponent<Rigidbody>();
                    break;

                //Left Index Finger
                case HumanBodyBones.LeftIndexProximal:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.LeftHand].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.LeftIndexIntermediate:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.LeftIndexProximal].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.LeftIndexDistal:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.LeftIndexIntermediate].GetComponent<Rigidbody>();
                    break;

                //Left Middle Finger
                case HumanBodyBones.LeftMiddleProximal:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.LeftHand].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.LeftMiddleIntermediate:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.LeftMiddleProximal].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.LeftMiddleDistal:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.LeftMiddleIntermediate].GetComponent<Rigidbody>();
                    break;

                //Left Ring Finger
                case HumanBodyBones.LeftRingProximal:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.LeftHand].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.LeftRingIntermediate:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.LeftRingProximal].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.LeftRingDistal:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.LeftRingIntermediate].GetComponent<Rigidbody>();
                    break;

                //Left Little Finger
                case HumanBodyBones.LeftLittleProximal:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.LeftHand].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.LeftLittleIntermediate:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.LeftLittleProximal].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.LeftLittleDistal:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.LeftLittleIntermediate].GetComponent<Rigidbody>();
                    break;
                #endregion
                #endregion

                #region Right Arm

                case HumanBodyBones.RightUpperArm:
                    ConfigureJoint(bone, gameObjectsFromBone[HumanBodyBones.Chest].GetComponent<Rigidbody>(), new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0), -45, 90, 85, 25);
                    break;
                case HumanBodyBones.RightLowerArm:
                    ConfigureJoint(bone, gameObjectsFromBone[HumanBodyBones.RightUpperArm].GetComponent<Rigidbody>(), new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1), 0, 177, 15, 15);
                    break;
                case HumanBodyBones.RightHand:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.RightLowerArm].GetComponent<Rigidbody>();
                    break;

                #region Right Hand

                //Right Thumb
                case HumanBodyBones.RightThumbProximal:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.RightHand].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.RightThumbIntermediate:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.RightThumbProximal].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.RightThumbDistal:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.RightIndexIntermediate].GetComponent<Rigidbody>();
                    break;

                //Right Index Finger
                case HumanBodyBones.RightIndexProximal:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.RightHand].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.RightIndexIntermediate:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.RightIndexProximal].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.RightIndexDistal:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.RightIndexIntermediate].GetComponent<Rigidbody>();
                    break;

                //Right Middle Finger
                case HumanBodyBones.RightMiddleProximal:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.RightHand].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.RightMiddleIntermediate:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.RightMiddleProximal].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.RightMiddleDistal:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.RightMiddleIntermediate].GetComponent<Rigidbody>();
                    break;

                //Right Ring Finger
                case HumanBodyBones.RightRingProximal:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.RightHand].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.RightRingIntermediate:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.RightRingProximal].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.RightRingDistal:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.RightRingIntermediate].GetComponent<Rigidbody>();
                    break;

                //Right Little Finger
                case HumanBodyBones.RightLittleProximal:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.RightHand].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.RightLittleIntermediate:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.RightLittleProximal].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.RightLittleDistal:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.RightLittleIntermediate].GetComponent<Rigidbody>();
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
                    ConfigureJoint(bone, gameObjectsFromBone[HumanBodyBones.Neck].GetComponent<Rigidbody>(), new Vector3(0, -0.05f, 0), new Vector3(1, 0, 0), new Vector3(0, 0, 1), -75, 75, 25, 25);
                    break;
                case HumanBodyBones.UpperChest:
                    break;
                case HumanBodyBones.Chest:
                    ConfigureJoint(HumanBodyBones.Chest, gameObjectsFromBone[HumanBodyBones.Hips].GetComponent<Rigidbody>(), new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 0, 1), -20, 20, 10, 0);
                    break;
                case HumanBodyBones.Spine:
                    break;

                #endregion

                #region Left Leg
                case HumanBodyBones.LeftUpperLeg:
                    ConfigureJoint(bone, gameObjectsFromBone[HumanBodyBones.Hips].GetComponent<Rigidbody>(), new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 0, 1), -75, 100, 25, 0);
                    break;
                case HumanBodyBones.LeftLowerLeg:
                    ConfigureJoint(bone, gameObjectsFromBone[HumanBodyBones.LeftUpperLeg].GetComponent<Rigidbody>(), new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 0, 1), -90, 0, 10, 10);
                    break;
                case HumanBodyBones.LeftFoot:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.LeftLowerLeg].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.LeftToes:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.LeftFoot].GetComponent<Rigidbody>();
                    break;
                #endregion

                #region Right Leg
                case HumanBodyBones.RightUpperLeg:
                    ConfigureJoint(bone, gameObjectsFromBone[HumanBodyBones.Hips].GetComponent<Rigidbody>(), new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 0, 1), -75, 100, 25, 0);
                    break;
                case HumanBodyBones.RightLowerLeg:
                    ConfigureJoint(bone, gameObjectsFromBone[HumanBodyBones.RightUpperLeg].GetComponent<Rigidbody>(), new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 0, 1), -90, 0, 10, 10);
                    break;
                case HumanBodyBones.RightFoot:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.RightLowerLeg].GetComponent<Rigidbody>();
                    break;
                case HumanBodyBones.RightToes:
                    gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = gameObjectsFromBone[HumanBodyBones.RightFoot].GetComponent<Rigidbody>();
                    break;
                #endregion

                default: break;
            }      
    }

    public void ConfigureJoint(HumanBodyBones bone, Rigidbody connectedBody, Vector3 anchor, Vector3 axis, Vector3 secondaryAxis, float lowAngularXLimit, float highAngularXLimit, float angularYLimit, float angularZLimit, float mass = 1)
    {
        /*
        //Anchor
        gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().anchor = anchor;

        //Axis
        gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().axis = axis;
        //Secondary Axis
        gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().secondaryAxis = secondaryAxis;

        //Limits
        jointLimit.limit = lowAngularXLimit;
        gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().lowAngularXLimit = jointLimit;
        jointLimit.limit = highAngularXLimit;
        gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().highAngularXLimit = jointLimit;
        jointLimit.limit = angularYLimit;
        gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().angularYLimit = jointLimit;
        jointLimit.limit = angularZLimit;
        gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().angularZLimit = jointLimit;
        
        //TODO: DRIVE VALUES

        gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().xDrive = xDrive;
        gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().yDrive = yDrive;
        gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().zDrive = zDrive;        
        
        gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().angularXDrive = angularXDrive;
        gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().angularYZDrive = angularYZDrive;
        
    */
        //Connected Body
        gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().connectedBody = connectedBody;
    }

    public void setMassOfBone(HumanBodyBones bone, float mass = 1)
    {
    }

    public void SetTagetTransform(HumanBodyBones bone, Transform target, Vector3 targetVelocity, Vector3 targetAngularVelocity)
    {
        if (!noJoints.Contains(bone) && gameObjectsFromBone.ContainsKey(bone) && gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>() != null)
        {
            gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().targetPosition = originalJointTransforms[bone].position - gameObjectsFromBone[bone].transform.InverseTransformPoint(target.position);
            gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().targetRotation = originalJointTransforms[bone].rotation * Quaternion.Inverse(Quaternion.Euler(target.localEulerAngles.x, target.localEulerAngles.y, target.localEulerAngles.z));
            //Debug.Log(gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().targetPosition);
            gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().targetVelocity = targetVelocity;
            gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>().targetAngularVelocity = targetAngularVelocity;
        }
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
