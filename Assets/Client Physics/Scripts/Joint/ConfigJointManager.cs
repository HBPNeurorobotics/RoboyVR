using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigJointManager : MonoBehaviour
{
    bool useIndividualAxes;
    bool useAnglesFromAnimationTest;

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

        this.useIndividualAxes = useIndividualAxes;
        if (useIndividualAxes)
        {
            //templateAnimator = GameObject.FindGameObjectWithTag("TemplateIndividual").GetComponent<Animator>();
        }
        else
        {
            templateAnimator = GameObject.FindGameObjectWithTag("Template").GetComponent<Animator>();
        }

        SetupJoints();

    }
    /// <summary>
    /// Assigns ConfigurableJoints configured by the editor
    /// </summary>
    /// <param name="useIndividualAxes"></param>
    public ConfigJointManager(bool useIndividualAxes)
    {
        this.useIndividualAxes = useIndividualAxes;
        if (useIndividualAxes)
        {
            //templateAnimator = GameObject.FindGameObjectWithTag("TemplateIndividual").GetComponent<Animator>();
        }
        else
        {
            templateAnimator = GameObject.FindGameObjectWithTag("Template").GetComponent<Animator>();
        }

        SetupJoints();
    }



    void SetupJoints()
    {
        GetAvatar();

        if (useIndividualAxes)
        {
            jointAngleLimits = ReadJointAngleLimitsFromJson();
        }
        else
        {
            InitTemplateDict();
        }

        //Physics.IgnoreLayerCollision(9, 9);

        foreach (HumanBodyBones bone in gameObjectsFromBone.Keys)
        {
            AddJoint(bone);
        }

    }
    /// <summary>
    /// Finds the GameObject tagged as avatar and assigns the AvatarManager reference
    /// </summary>
    void GetAvatar()
    {
        avatarManager = GameObject.FindGameObjectWithTag("Avatar").GetComponent<AvatarManager>();
        gameObjectsFromBone = avatarManager.GetGameObjectPerBoneAvatarDictionary();
        useAnglesFromAnimationTest = avatarManager.useAnglesFromAnimationTest;
    }
    /// <summary>
    /// Adds a ConfigurableJoint to the GameObject that corresponds to the specified bone.
    /// </summary>
    /// <param name="bone">The bone that a ConfigurableJoint component should be added to. If useIndividualAxis joint angles are set from previous animation test.</param>
    void AddJoint(HumanBodyBones bone)
    {
        if (!useIndividualAxes)
        {
            AddJointFromTemplate(bone);
        }
        else
        {
            AddJointFromAnimationTest(bone);
        }
    }
    /// <summary>
    /// Copys the ConfigurableJoint from a template avatar and pastes its values into the newly added ConfigurableJoint at the bone. 
    /// </summary>
    /// <param name="bone">The bone that the new ConfigurableJoint is added to in the remote avatar. This is also the bone that the values are copied from in the template.</param>
    void AddJointFromTemplate(HumanBodyBones bone)
    {
        //Assign collision layer according to template
        gameObjectsFromBone[bone].layer = templateFromBone[bone].layer;
        //Assign rigidbody 
        Rigidbody templateRb = templateFromBone[bone].gameObject.GetComponent<Rigidbody>();
        if (templateRb != null)
        {
            templateRb.useGravity = false;

            UnityEditorInternal.ComponentUtility.CopyComponent(templateRb);
            UnityEditorInternal.ComponentUtility.PasteComponentValues(gameObjectsFromBone[bone].GetComponent<Rigidbody>());
        }
        //Add colliders if needed
        if (avatarManager.ShouldAddColliders())
        {
            Component colliderComp;
            //Some bones have multiple colliders to better fit the shape of the body part
            Collider[] templateColliders = templateFromBone[bone].GetComponents<Collider>();
            foreach (Collider templateCollider in templateColliders)
            {
                Type colliderType = templateCollider.GetType();
                colliderComp = gameObjectsFromBone[bone].AddComponent(colliderType);

                //Colliders recalculate the center of mass and inertia tensor of the rigidbody. Since this leads to unintended behavior we have to restore default values.
                gameObjectsFromBone[bone].GetComponent<Rigidbody>().centerOfMass = Vector3.zero;
                gameObjectsFromBone[bone].GetComponent<Rigidbody>().inertiaTensor = Vector3.one;

                UnityEditorInternal.ComponentUtility.CopyComponent(templateCollider);
                UnityEditorInternal.ComponentUtility.PasteComponentValues(colliderComp);

                //We need to disable the template collider to avoid collisions and save costs
                templateFromBone[bone].GetComponent<Collider>().enabled = false;
            }
        }

        //Add joint(s)
        ConfigurableJoint[] jointsOfTemplateBone = templateFromBone[bone].GetComponents<ConfigurableJoint>();
        for (int i = 0; i < jointsOfTemplateBone.Length; i++)
        {
            ConfigurableJoint newJoint = gameObjectsFromBone[bone].AddComponent<ConfigurableJoint>();


            UnityEditorInternal.ComponentUtility.CopyComponent(jointsOfTemplateBone[i]);
            UnityEditorInternal.ComponentUtility.PasteComponentValues(newJoint);

            //Set Connected Rigidbody of Joints
            SetConnectedBody(bone, newJoint);
        }
    }

    void AddJointFromAnimationTest(HumanBodyBones bone)
    {
        //Add joint to handle x rotation
        ConfigurableJoint xJoint = gameObjectsFromBone[bone].AddComponent<ConfigurableJoint>();
        xJoint.axis = new Vector3(1, 0, 0);
        xJoint.angularXMotion = ConfigurableJointMotion.Limited;



        //Add joint to handle x rotation
        ConfigurableJoint yJoint = gameObjectsFromBone[bone].AddComponent<ConfigurableJoint>();
        yJoint.axis = new Vector3(0, 1, 0);
        yJoint.angularXMotion = ConfigurableJointMotion.Limited;



        //Add joint to handle x rotation
        ConfigurableJoint zJoint = gameObjectsFromBone[bone].AddComponent<ConfigurableJoint>();
        zJoint.axis = new Vector3(0, 0, 1);
        zJoint.angularXMotion = ConfigurableJointMotion.Limited;


        switch (bone)
        {
            case HumanBodyBones.Chest:
            case HumanBodyBones.Spine:
            case HumanBodyBones.RightUpperLeg:
            case HumanBodyBones.RightLowerLeg:
            case HumanBodyBones.RightFoot:
            case HumanBodyBones.RightToes:
                xJoint.secondaryAxis = new Vector3(0, 0, 1);
                yJoint.secondaryAxis = new Vector3(0, 1, 0);
                zJoint.secondaryAxis = new Vector3(1, 0, 0);

                ApplyAnglesFromAnimationTest(bone, xJoint, 'x', true);
                ApplyAnglesFromAnimationTest(bone, yJoint, 'y', true);
                ApplyAnglesFromAnimationTest(bone, zJoint, 'z', true);
                break;
            default:
                ApplyAnglesFromAnimationTest(bone, xJoint, 'x', false);
                ApplyAnglesFromAnimationTest(bone, yJoint, 'y', false);
                ApplyAnglesFromAnimationTest(bone, zJoint, 'z', false);
                break;
        }

        SetConnectedBody(bone, xJoint);
        SetConnectedBody(bone, yJoint);
        SetConnectedBody(bone, zJoint);

    }

    Dictionary<HumanBodyBones, JointAngleContainer> ReadJointAngleLimitsFromJson()
    {
        TextAsset file = avatarManager.angles;
        string[] lines = file.text.Split('\n');

        Dictionary<HumanBodyBones, JointAngleContainer> jointAngleLimits = new Dictionary<HumanBodyBones, JointAngleContainer>();

        foreach (string line in lines)
        {
            if (line.Length > 0)
            {
                JointAngleContainer container = JsonUtility.FromJson<JointAngleContainer>(line);
                jointAngleLimits.Add(container.bone, container);
            }
        }

        return jointAngleLimits;

    }

    // This needs to consider the axis definition of the joint!
    void ApplyAnglesFromAnimationTest(HumanBodyBones bone, ConfigurableJoint joint, char axis, bool invertMinMax)
    {
        JointAngleContainer angleContainer;
        if (jointAngleLimits.TryGetValue(bone, out angleContainer))
        {

            float lowerLimit = 0f, upperLimit = 0f;
            switch (axis)
            {
                case 'x':
                    lowerLimit = angleContainer.minAngleX;
                    upperLimit = angleContainer.maxAngleX;
                    break;
                case 'y':
                    lowerLimit = angleContainer.minAngleY;
                    upperLimit = angleContainer.maxAngleY;
                    break;
                case 'z':
                    lowerLimit = angleContainer.minAngleZ;
                    upperLimit = angleContainer.maxAngleZ;
                    break;
            }

            if (invertMinMax)
            {
                float tmp = lowerLimit;
                lowerLimit = upperLimit;
                upperLimit = lowerLimit;
            }

            joint.lowAngularXLimit = ApplySoftJointAngleHelper(joint.lowAngularXLimit, lowerLimit);
            joint.highAngularXLimit = ApplySoftJointAngleHelper(joint.highAngularXLimit, upperLimit);

            /*
            ApplySoftJointAngleHelper(joint.lowAngularXLimit, angleContainer.minAngleX);
            ApplySoftJointAngleHelper(joint.highAngularXLimit, angleContainer.maxAngleX);

            float midPoint = (Mathf.Abs(angleContainer.minAngleY) + Mathf.Abs(angleContainer.maxAngleY)) / 2f;
            ApplySoftJointAngleHelper(joint.angularYLimit, midPoint);

            midPoint = (Mathf.Abs(angleContainer.minAngleZ) + Mathf.Abs(angleContainer.maxAngleZ)) / 2f;
            ApplySoftJointAngleHelper(joint.angularZLimit, midPoint);
            */
        }
    }

    SoftJointLimit ApplySoftJointAngleHelper(SoftJointLimit toChange, float angle)
    {
        SoftJointLimit newLimit = new SoftJointLimit();
        newLimit = toChange;
        newLimit.limit = angle;
        return newLimit;
    }

    /// <summary>
    /// Sets the connectedBody property of the ConfigurableJoint in a human body.
    /// </summary>
    /// <param name="bone">The bone of the ConfigurableJoint.</param>
    /// <param name="joint">The joined at a bone. This needs to be specified to support cases of multiple joints per bone (e.g. one for each axis).</param>
    void SetConnectedBody(HumanBodyBones bone, ConfigurableJoint joint)
    {

        joint.xMotion = ConfigurableJointMotion.Limited;
        joint.yMotion = ConfigurableJointMotion.Limited;
        joint.zMotion = ConfigurableJointMotion.Limited;

        JointDrive drive = new JointDrive();
        drive.positionDamper = 600;
        drive.positionSpring = 3000;

        joint.xDrive = drive;
        joint.yDrive = drive;
        joint.zDrive = drive;
        
        /*
        if (!useIndividualAxes)
        {
            joint.angularXMotion = ConfigurableJointMotion.Free;
            joint.angularYMotion = ConfigurableJointMotion.Free;
            joint.angularZMotion = ConfigurableJointMotion.Free;
        }
        */


        joint.configuredInWorldSpace = false;

        joint.enableCollision = false;
        joint.enablePreprocessing = false;
        //joint.projectionMode = JointProjectionMode.PositionAndRotation;
        //joint.projectionAngle = 2f;
        //joint.projectionDistance = 0.1f;

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
            //TODO assign head to neck, neck is too light
            case HumanBodyBones.Head:
                ConfigureJoint(bone, joint, gameObjectsFromBone[HumanBodyBones.Neck].GetComponent<Rigidbody>());
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

        //joint.autoConfigureConnectedAnchor = false;
        //joint.autoConfigureConnectedAnchor = false;
    }

    /// <summary>
    /// Sets joint values (TODO remove), Sets the connected body of a ConfigurableJoint.
    /// </summary>
    /// <param name="bone"> The bone of the BodyPart that has a ConfigurableJoint Component</param>
    /// <param name="joint">A ConfigurableJoint of the bone (might be multiple in the future)</param>
    /// <param name="connectedBody">The rigidbody of the Object that the joint is connected to. For example, this would be the LeftUpperArm if the bone is the LeftLowerArm, NOT the LeftHand</param>
    public void ConfigureJoint(HumanBodyBones bone, ConfigurableJoint joint, Rigidbody connectedBody)
    {
        //TODO: CURRENTLY OVEWRITES EDITOR INPUT, REMOVE LATER
        joint.xDrive = xDrive;
        joint.yDrive = yDrive;
        joint.zDrive = zDrive;

        joint.angularXDrive = angularXDrive;
        joint.angularYZDrive = angularYZDrive;
        //END TODO

        //Connected Body
        joint.connectedBody = connectedBody;

        //This will only be used if there are no individual rotations/velocities assigned by the AvatarManager
        if (!avatarManager.usesActiveInput())
        {/*
            if (usesFixedJoint.Contains(bone))
            {
                joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = ConfigurableJointMotion.Locked;
            } 
            else
            {
                AssignTargetToImitatePassive(bone);
            }
            */
            AssignTargetToImitatePassive(bone);
        }
        else
        {
            AssignOriginalTransforms(bone);
        }
    }

    void AssignTargetToImitatePassive(HumanBodyBones bone)
    {
        if (gameObjectsFromBone[bone].GetComponent<ConfigJointMotionHandler>() == null)
        {
            ConfigJointMotionHandler rotationHelper = gameObjectsFromBone[bone].AddComponent<ConfigJointMotionHandler>();
            rotationHelper.target = avatarManager.GetGameObjectPerBoneTargetDictionary()[bone];
        }
    }

    void AssignOriginalTransforms(HumanBodyBones bone)
    {
        quaternionFromBoneAtStart = avatarManager.GetGameObjectPerBoneRemoteAvatarDictionaryAtStart();
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
}