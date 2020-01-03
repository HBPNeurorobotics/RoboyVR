using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointSetup
{

    Dictionary<HumanBodyBones, GameObject> gameObjectsFromBone;
    Dictionary<HumanBodyBones, GameObject> templateFromBone;
    ConfigJointManager configJointManager;

    bool meshCollidersDone = false;
    bool simpleCollidersDone = false;

    JointDrive angularXDrive;
    JointDrive angularYZDrive;

    public JointSetup(Dictionary<HumanBodyBones, GameObject> gameObjectsFromBone, Dictionary<HumanBodyBones, GameObject> templateFromBone, ConfigJointManager configJointManager)
    {
        this.gameObjectsFromBone = gameObjectsFromBone;
        this.templateFromBone = templateFromBone;
        this.configJointManager = configJointManager;

        angularXDrive = configJointManager.GetAngularXDrive();
        angularYZDrive = configJointManager.GetAngularYZDrive();

    }

    public void ToggleMeshColliders(bool enabled)
    {
        foreach (HumanBodyBones bone in gameObjectsFromBone.Keys)
        {
            MeshCollider collider = gameObjectsFromBone[bone].GetComponent<MeshCollider>();
            if (collider == null)
            {
                AddMeshColliders(bone);
                collider = gameObjectsFromBone[bone].GetComponent<MeshCollider>();
            }

            if (collider != null)
            {
                MeshCollider[] colliders = gameObjectsFromBone[bone].GetComponents<MeshCollider>();
                foreach (MeshCollider col in colliders)
                {
                    col.enabled = enabled;
                }
            }
        }
    }

    public void ToggleSimpleColliders(bool enabled)
    {
        foreach (HumanBodyBones bone in gameObjectsFromBone.Keys)
        {
            Collider[] colliders = gameObjectsFromBone[bone].GetComponents<Collider>();
            bool hasOnlyMeshColliders = false;
            foreach (Collider col in colliders)
            {
                if (col is MeshCollider)
                {
                    hasOnlyMeshColliders = true;
                }
                else
                {
                    hasOnlyMeshColliders = false;
                    break;
                }
            }

            if (colliders.Length == 0 || hasOnlyMeshColliders)
            {
                CopyPasteColliders(bone);
                colliders = gameObjectsFromBone[bone].GetComponents<Collider>();
            }

            foreach (Collider col in colliders)
            {
                if (col is MeshCollider)
                {

                }
                else
                {
                    col.enabled = enabled;
                }
            }
        }
    }

    public void InitializeStructures()
    {
        foreach (HumanBodyBones bone in gameObjectsFromBone.Keys)
        {
            if (gameObjectsFromBone[bone].GetComponent<ConfigurableJoint>() == null)
            {
                AddJoint(bone);
            }
        }
    }


    /// <summary>
    /// Adds a ConfigurableJoint to the GameObject that corresponds to the specified bone.
    /// </summary>
    /// <param name="bone">The bone that a ConfigurableJoint component should be added to. If useIndividualAxis joint angles are set from previous animation test.</param>
    void AddJoint(HumanBodyBones bone)
    {
        if (!configJointManager.useJointsMultipleTemplate)
        {
            AddJointFromTemplate(bone);
        }
        /*
        else
        {
            AddJointFromAnimationTest(bone);
        }
        */
    }
    /// <summary>
    /// Copys the ConfigurableJoint from a template avatar and pastes its values into the newly added ConfigurableJoint at the bone. 
    /// </summary>
    /// <param name="bone">The bone that the new ConfigurableJoint is added to in the remote avatar. This is also the bone that the values are copied from in the template.</param>
    void AddJointFromTemplate(HumanBodyBones bone)
    {
        //Assign rigidbody 
        CopyPasteRigidbody(bone);

        //Add colliders if needed
        if (configJointManager.addSimpleColliders)
        {
            CopyPasteColliders(bone);
        }
        else
        {
            if (configJointManager.addMeshColliders)
            {
                AddMeshColliders(bone);
            }
        }

        //We need to disable the template collider to avoid collisions and save costs
        DisableTemplateColliders();

        //Add joint(s)
        CopyPasteJoint(bone);
    }

    void DisableTemplateColliders()
    {
        foreach (HumanBodyBones bone in templateFromBone.Keys)
        {
            Collider[] colliders = templateFromBone[bone].GetComponents<Collider>();
            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }
        }
    }

    void AddMeshColliders(HumanBodyBones bone)
    {
        List<Mesh> meshes = configJointManager.gameObject.GetComponent<BoneMeshContainer>().GetMeshesFromBone(bone);
        if (meshes != null)
        {
            foreach (Mesh mesh in meshes)
            {
                //We have to make sure that the origin of the mesh matches the bone position! Convert from Unity(left handed) to Blender(right handed) coordinates: x = -x, y = -z, z = y
                MeshCollider meshCollider = gameObjectsFromBone[bone].AddComponent<MeshCollider>();
                meshCollider.enabled = true;


                //We need to make sure that Unity treats the assigned mesh as convex. Blender's convex hull is sometimes not valid and has to be converted into a form accepted by Unity
                meshCollider.cookingOptions = MeshColliderCookingOptions.InflateConvexMesh;
                meshCollider.convex = true;

                meshCollider.sharedMesh = mesh;
                //Set tolerance to almost zero to preserve orignal "convex-in-blender" form
                meshCollider.skinWidth = 1e-20f;

            }

            //Disable self-collision for composite colliders
            gameObjectsFromBone[bone].layer = templateFromBone[bone].layer;

            //Colliders recalculate the center of mass and inertia tensor of the rigidbody. Since this leads to unintended behavior we have to restore default values.
            gameObjectsFromBone[bone].GetComponent<Rigidbody>().centerOfMass = Vector3.zero;
            gameObjectsFromBone[bone].GetComponent<Rigidbody>().inertiaTensor = Vector3.one;
        }
    }

    void CopyPasteRigidbody(HumanBodyBones bone)
    {
        Rigidbody templateRb = templateFromBone[bone].gameObject.GetComponent<Rigidbody>();
        if (templateRb != null)
        {
            templateRb.useGravity = false;

            UnityEditorInternal.ComponentUtility.CopyComponent(templateRb);
            UnityEditorInternal.ComponentUtility.PasteComponentValues(gameObjectsFromBone[bone].GetComponent<Rigidbody>());
        }
    }

    void CopyPasteJoint(HumanBodyBones bone)
    {
        if (configJointManager.useJointsMultipleTemplate)
        {
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
        else
        {

            ConfigurableJoint joint = templateFromBone[bone].GetComponent<ConfigurableJoint>();
            ConfigurableJoint newJoint = gameObjectsFromBone[bone].AddComponent<ConfigurableJoint>();

            UnityEditorInternal.ComponentUtility.CopyComponent(joint);
            UnityEditorInternal.ComponentUtility.PasteComponentValues(newJoint);

            SetConnectedBody(bone, newJoint);

            if (configJointManager.splitJointTemplate)
            {
                AddSplitJoints(newJoint, bone);
            }

        }
    }
    /// <summary>
    /// Creates 2 additional joints, that have the y/z axis of the previous joint as x axis respectively
    /// </summary>
    /// <param name="joint">The joint to split.</param>
    /// <param name="bone">The bone of the body part that the joint is attached to.</param>
    void AddSplitJoints(ConfigurableJoint joint, HumanBodyBones bone)
    {
        Vector3 primaryAxisOne = Vector3.right;
        Vector3 secondaryAxisOne = Vector3.up;
        Vector3 primaryAxisTwo = Vector3.right;
        Vector3 secondaryAxisTwo = Vector3.up;

        ConfigurableJoint jointA = joint.gameObject.AddComponent<ConfigurableJoint>();
        ConfigurableJoint jointB = joint.gameObject.AddComponent<ConfigurableJoint>();


        UnityEditorInternal.ComponentUtility.CopyComponent(joint);
        UnityEditorInternal.ComponentUtility.PasteComponentValues(jointA);
        UnityEditorInternal.ComponentUtility.PasteComponentValues(jointB);

        SoftJointLimit lowLimit = new SoftJointLimit();

        //Torso, Legs
        if (((joint.axis == Vector3.right) && (joint.secondaryAxis == Vector3.forward || joint.secondaryAxis == Vector3.back || joint.secondaryAxis == Vector3.up))
           || joint.axis == Vector3.left && joint.secondaryAxis == Vector3.forward)
        {
            primaryAxisOne = Vector3.up;
            secondaryAxisOne = Vector3.zero;

            primaryAxisTwo = Vector3.forward;
            secondaryAxisTwo = Vector3.right;
        }
        else
        {
            //Arms
            if (joint.axis == Vector3.up)
            {
                primaryAxisOne = Vector3.right;
                secondaryAxisOne = Vector3.back;

                primaryAxisTwo = Vector3.forward;

                //right
                if (joint.secondaryAxis == Vector3.forward)
                {
                    secondaryAxisTwo = Vector3.up;
                }
                else
                {
                    //left
                    if (joint.secondaryAxis == Vector3.back)
                    {
                        secondaryAxisTwo = Vector3.zero;
                    }
                }
            }
            else
            {
                //left hand
                if (joint.axis == Vector3.forward && joint.secondaryAxis == Vector3.up)
                {
                    primaryAxisOne = Vector3.up;
                    secondaryAxisOne = Vector3.back;

                    primaryAxisTwo = Vector3.right;
                    secondaryAxisTwo = Vector3.back;
                }
                else
                {
                    //right hand
                    if ((joint.axis == Vector3.back && joint.secondaryAxis == Vector3.up) || (joint.axis == Vector3.forward && joint.secondaryAxis == Vector3.down))
                    {
                        primaryAxisOne = Vector3.up;
                        secondaryAxisOne = Vector3.forward;

                        primaryAxisTwo = Vector3.right;
                        secondaryAxisTwo = Vector3.back;
                    }
                }
            }
        }

        jointA.axis = primaryAxisOne;
        jointA.secondaryAxis = secondaryAxisOne;

        jointA.highAngularXLimit = joint.angularZLimit;
        lowLimit = joint.angularZLimit;
        lowLimit.limit *= -1;
        jointA.lowAngularXLimit = lowLimit;

        jointB.axis = primaryAxisTwo;
        jointB.secondaryAxis = secondaryAxisTwo;

        jointB.highAngularXLimit = joint.angularYLimit;
        lowLimit = joint.angularYLimit;
        lowLimit.limit *= -1;
        jointB.lowAngularXLimit = lowLimit;


        //only primary axis restricted
        lowLimit.limit = 0;
        jointA.angularYLimit = jointB.angularYLimit = joint.angularYLimit = lowLimit;
        jointA.angularZLimit = jointB.angularZLimit = joint.angularZLimit = lowLimit;

        joint.angularXMotion = jointA.angularXMotion = jointB.angularXMotion = ConfigurableJointMotion.Limited;
        joint.angularYMotion = jointA.angularYMotion = jointB.angularYMotion = ConfigurableJointMotion.Free;
        joint.angularZMotion = jointA.angularZMotion = jointB.angularZMotion = ConfigurableJointMotion.Free;
    }

    void CopyPasteColliders(HumanBodyBones bone)
    {
        //Assign collision layer according to template
        gameObjectsFromBone[bone].layer = templateFromBone[bone].layer;

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
        }
    }

    IEnumerator EnableCollider(HumanBodyBones bone)
    {
        yield return new WaitForSeconds(0.5f);
        foreach (MeshCollider collider in gameObjectsFromBone[bone].GetComponents<MeshCollider>())
        {
            collider.enabled = true;
        }
    }


    /// <summary>
    /// Sets the connectedBody property of the ConfigurableJoint in a human body.
    /// </summary>
    /// <param name="bone">The bone of the ConfigurableJoint.</param>
    /// <param name="joint">The joined at a bone. This needs to be specified to support cases of multiple joints per bone (e.g. one for each axis).</param>
    void SetConnectedBody(HumanBodyBones bone, ConfigurableJoint joint)
    {

        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;

        JointDrive drive = new JointDrive();
        drive.positionDamper = 0;
        drive.positionSpring = 0;

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
        joint.enablePreprocessing = true;

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
        joint.angularXDrive = angularXDrive;
        joint.angularYZDrive = angularYZDrive;
        //END TODO


        //Connected Body
        joint.connectedBody = connectedBody;

        //This will only be used if there are no individual rotations/velocities assigned by the AvatarManager
        if (!configJointManager.inputByManager)
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
            rotationHelper.target = configJointManager.gameObject.GetComponent<AvatarManager>().GetGameObjectPerBoneLocalAvatarDictionary()[bone];
        }
    }

    void AssignOriginalTransforms(HumanBodyBones bone)
    {
        configJointManager.SetStartOrientation();
    }

    /*
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
*/
}
