using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AvatarManager : MonoBehaviour
{

    public bool useJoints = true;
    public bool useBodyMass = false;
    public bool useIndividualAxes = true;
    public float weight = 72f;
    public float PDKp = 1;
    public float PDKd = 1;
    public float Kp = 8;
    public float Ki = 0;
    public float Kd = .05f;

    ConfigJointManager configJointManager;

    public JointDrive xDrive = new JointDrive();
    public JointDrive yDrive = new JointDrive();
    public JointDrive zDrive = new JointDrive();

    public JointDrive angularXDrive = new JointDrive();
    public JointDrive angularYZDrive = new JointDrive();


    Animator animatorRemoteAvatar;
    Animator animatorTarget;

    Dictionary<HumanBodyBones, GameObject> gameObjectPerBoneRemoteAvatar = new Dictionary<HumanBodyBones, GameObject>();
    Dictionary<GameObject, HumanBodyBones> bonesPerGameObjectRemoteAvatar = new Dictionary<GameObject, HumanBodyBones>();
    Dictionary<HumanBodyBones, GameObject> gameObjectPerBoneTarget = new Dictionary<HumanBodyBones, GameObject>();

    // Use this for initialization
    void Start()
    {
        animatorRemoteAvatar = GetComponentInChildren<Animator>();
        animatorTarget = GameObject.FindGameObjectWithTag("Target").GetComponent<Animator>();
        InitializeBodyStructures();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!useJoints)
        {
            UpdatePDControllers();
        }
        else
        {
            //UpdatePDControllers();
            UpdateJoints();
        }
        //UpdateVacuumBreatherPIDControllers();
        //UpdateJoints();
        //UpdateMerchVRPIDControllers();
        if (Input.GetKey(KeyCode.Mouse0))
        {
            gameObjectPerBoneRemoteAvatar[HumanBodyBones.LeftLowerArm].GetComponent<Rigidbody>().AddTorque(Vector3.forward * 10, ForceMode.Force);
        }

    }

    /// <summary>
    ///     Maps all HumanBodyBones (assigned in the Avatar) to their GameObjects in the scene in order to get access to all components.
    ///     Adds Rigidbody to both bodies, adds PDController to the avatar.
    /// </summary>
    void InitializeBodyStructures()
    {
        foreach (HumanBodyBones bone in System.Enum.GetValues(typeof(HumanBodyBones)))
        {
            //LastBone is not mapped to a bodypart, we need to skip it.
            if (bone != HumanBodyBones.LastBone)
            {
                Transform boneTransformAvatar = animatorRemoteAvatar.GetBoneTransform(bone);
                Transform boneTransformTarget = animatorTarget.GetBoneTransform(bone);
                //We have to skip unassigned bodyparts.
                if (boneTransformAvatar != null && boneTransformTarget != null)
                {
                    //build Dictionaries
                    gameObjectPerBoneRemoteAvatar.Add(bone, boneTransformAvatar.gameObject);
                    gameObjectPerBoneTarget.Add(bone, boneTransformTarget.gameObject);
                    bonesPerGameObjectRemoteAvatar.Add(boneTransformAvatar.gameObject, bone);

                    AssignRigidbodys(bone);
                    AssignPDController(bone);

                    //AssignVacuumBreatherPIDController(bone);
                    //AssignMerchVRPIDController(bone);
                }
            }
        }


        if (useJoints)
        {
            xDrive.positionSpring = yDrive.positionSpring = zDrive.positionSpring = 2500;
            xDrive.positionDamper = yDrive.positionDamper = zDrive.positionDamper = 600;
            xDrive.maximumForce = yDrive.maximumForce = zDrive.maximumForce = 10000;

            angularXDrive.positionSpring = angularYZDrive.positionSpring = 2500;
            angularXDrive.positionDamper = angularYZDrive.positionDamper = 600;
            angularXDrive.maximumForce = angularYZDrive.maximumForce = 10000;


            configJointManager = new ConfigJointManager(xDrive, yDrive, zDrive, angularXDrive, angularYZDrive, useIndividualAxes);
            configJointManager.SetupJoints();
        }

        if (useBodyMass)
        {
            BodyMass bm = new BodyMass(weight, gameObjectPerBoneRemoteAvatar);
        }

    }

    /// <summary>
    ///     A method to return the Rigidbody of the GameObject that corresponds to a certain bodypart. 
    ///     Use this to gain access to the velocity of the bodypart.
    /// </summary>
    Rigidbody GetRigidbodyFromBone(bool fromAvatar, HumanBodyBones boneID)
    {
        GameObject obj;
        if ((fromAvatar ? gameObjectPerBoneRemoteAvatar : gameObjectPerBoneTarget).TryGetValue(boneID, out obj))
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

        gameObjectPerBoneTarget[bone].AddComponent<Rigidbody>();
        gameObjectPerBoneTarget[bone].GetComponent<Rigidbody>().useGravity = false;
        if (!bone.Equals(HumanBodyBones.RightUpperArm))
        {
            //gameObjectPerBoneRemoteAvatar[bone].GetComponent<Rigidbody>().isKinematic = true;
        }

    }

    void SetupRagdollForAvatar()
    {

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
        gameObjectPerBoneRemoteAvatar[bone].AddComponent<PDController>();
        gameObjectPerBoneRemoteAvatar[bone].GetComponent<PDController>().rigidbody = gameObjectPerBoneRemoteAvatar[bone].GetComponent<Rigidbody>();
        gameObjectPerBoneRemoteAvatar[bone].GetComponent<PDController>().oldVelocity = gameObjectPerBoneRemoteAvatar[bone].GetComponent<PDController>().rigidbody.velocity;
        gameObjectPerBoneRemoteAvatar[bone].GetComponent<PDController>().proportionalGain = PDKp;
        gameObjectPerBoneRemoteAvatar[bone].GetComponent<PDController>().derivativeGain = PDKd;
    }
    void AssignVacuumBreatherPIDController(HumanBodyBones bone)
    {
        gameObjectPerBoneRemoteAvatar[bone].AddComponent<VacuumBreather.ControlledObject>();
        gameObjectPerBoneRemoteAvatar[bone].GetComponent<VacuumBreather.ControlledObject>().Kp = Kp;
        gameObjectPerBoneRemoteAvatar[bone].GetComponent<VacuumBreather.ControlledObject>().Ki = Ki;
        gameObjectPerBoneRemoteAvatar[bone].GetComponent<VacuumBreather.ControlledObject>().Kd = Kd;
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
            gameObjectPerBoneRemoteAvatar[bone].GetComponent<PDController>().SetDestination(gameObjectPerBoneTarget[bone].transform, targetRb.velocity);
        }
    }

    void UpdateVacuumBreatherPIDControllers()
    {
        foreach (HumanBodyBones bone in gameObjectPerBoneRemoteAvatar.Keys)
        {
            gameObjectPerBoneRemoteAvatar[bone].GetComponent<VacuumBreather.ControlledObject>().DesiredOrientation = gameObjectPerBoneTarget[bone].transform.rotation;
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
        Transform rootBone = gameObjectPerBoneRemoteAvatar[HumanBodyBones.Hips].transform;
        //UpdateJointsRecursive(rootBone);
        foreach (HumanBodyBones bone in gameObjectPerBoneRemoteAvatar.Keys)
        {
            //gameObjectPerBoneRemoteAvatar[bone].GetComponent<Rigidbody>().freezeRotation = false;
            configJointManager.SetTagetTransform(bone, gameObjectPerBoneTarget[bone].transform, gameObjectPerBoneTarget[bone].GetComponent<Rigidbody>().velocity, gameObjectPerBoneTarget[bone].GetComponent<Rigidbody>().angularVelocity);
        }
    }

    void UpdateJointsRecursive(Transform boneTransform)
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
                        configJointManager.SetTagetTransform(tmpBone, gameObjectPerBoneTarget[tmpBone].transform, targetRb.velocity, targetRb.angularVelocity);

                        //gameObjectPerBoneRemoteAvatar[tmpBone].GetComponent<ConfigurableJoint>().connectedBody.freezeRotation = true;
                        //WaitUntilRotationComplete(tmpBone, gameObjectPerBoneTarget[tmpBone].transform.rotation);
                    }
                }
            }

        }
        foreach (Transform child in boneTransform)
        {
            UpdateJointsRecursive(child);
        }

    }

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


    public Dictionary<HumanBodyBones, GameObject> GetGameObjectPerBoneAvatarDictionary()
    {
        return gameObjectPerBoneRemoteAvatar;
    }
    public Dictionary<HumanBodyBones, GameObject> GetGameObjectPerBoneTargetDictionary()
    {
        return gameObjectPerBoneTarget;
    }
}
