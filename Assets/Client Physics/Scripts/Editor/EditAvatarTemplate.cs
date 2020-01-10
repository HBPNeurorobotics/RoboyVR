﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[ExecuteInEditMode]
public class EditAvatarTemplate : EditorWindow
{
    float bodyWeight = 72f;
    BodyMass.MODE mode = BodyMass.MODE.AVERAGE;
    float indent = 15f;

    bool useCollisions;
    bool noPreprocessing = true;
    bool setGlobalJointSetting = false;
    bool mirror;
    bool showJointSettings = true;
    bool showGlobalJointSettings = true;
    bool useGravity = true;

    public float angularXDriveSpringGlobal;
    public float angularXDriveDamperGlobal;
    public float maxForceXGlobal;

    public float angularYZDriveSpringGlobal;
    public float angularYZDriveDamperGlobal;
    public float maxForceYZGlobal;

    BodyGroups.BODYGROUP bodyGroup;

    JointSettings globalSettings;

    static EditAvatarTemplate editor;

    Vector2 scroll;

    Object template;
    Object templateMultiple;
    Dictionary<HumanBodyBones, GameObject> gameObjectsPerBoneTemplate = new Dictionary<HumanBodyBones, GameObject>();
    Dictionary<HumanBodyBones, GameObject> gameObjectsPerBoneTemplateMultiple = new Dictionary<HumanBodyBones, GameObject>();

    Dictionary<HumanBodyBones, JointSettings> jointSettings = new Dictionary<HumanBodyBones, JointSettings>();
    Dictionary<HumanBodyBones, JointSettings> jointSettingsNoLeft = new Dictionary<HumanBodyBones, JointSettings>();

    BodyMass bodyMass;

    [MenuItem("Window/Edit Joint Avatar Template")]
    public static void GetWindow()
    {
        editor = (EditAvatarTemplate)GetWindow(typeof(EditAvatarTemplate));
        editor.Show();
    }
    void OnGUI()
    {
        //editor = (EditAvatarTemplate)GetWindow(typeof(EditAvatarTemplate));
        EditorGUILayout.HelpBox("Assign the rig of AvatarTemplate and AvatarTemplateMultipleJoints", MessageType.Info);


        template = EditorGUILayout.ObjectField("Avatar Template Rig", template, typeof(GameObject), true);
        templateMultiple = EditorGUILayout.ObjectField("Avatar Template Multiple Joints Rig", templateMultiple, typeof(GameObject), true);

        if (template != null && templateMultiple != null)
        {
            EditorGUILayout.BeginVertical();
            bodyWeight = EditorGUILayout.FloatField("Total Body Weight", bodyWeight);
            mode = (BodyMass.MODE)EditorGUILayout.EnumPopup("Population Group of Avatar", mode);

            bodyGroup = (BodyGroups.BODYGROUP)EditorGUILayout.EnumPopup("Body Group", bodyGroup);

            GetDictionary();

            useGravity = EditorGUILayout.Toggle("Enable Gravity", useGravity);
            useCollisions = EditorGUILayout.Toggle("Enable Collisions Between Joints", useCollisions);
            noPreprocessing = EditorGUILayout.Toggle("Disable Preprocessing", noPreprocessing);
            setGlobalJointSetting = EditorGUILayout.Toggle("Set Global Joint Settings", setGlobalJointSetting);

            


            globalSettings = new JointSettings(HumanBodyBones.LastBone, angularXDriveSpringGlobal, angularXDriveDamperGlobal, maxForceXGlobal, angularYZDriveSpringGlobal, angularYZDriveDamperGlobal, maxForceYZGlobal);


            if (!setGlobalJointSetting)
            {
                mirror = EditorGUILayout.Toggle("Mirror Changes", mirror);
                showJointSettings = EditorGUILayout.Foldout(showJointSettings, "Joint Settings", true);

                if (showJointSettings)
                {
                    DisplayJointSettings();
                }
            }
            else
            {
                DisplayGlobalJoint();
            }

            EditorGUILayout.EndVertical();

            if (GUILayout.Button("Restore Default Mass"))
            {
                bodyMass = new BodyMass(bodyWeight, gameObjectsPerBoneTemplate, mode);
                bodyMass.RestoreOneValues();
            }

            //TODO
            if (GUILayout.Button("Restore All"))
            {
                foreach (HumanBodyBones bone in gameObjectsPerBoneTemplate.Keys)
                {
                    PrefabUtility.ResetToPrefabState(gameObjectsPerBoneTemplate[bone]);
                }
                //template = (GameObject)Instantiate(Resources.Load("Assets/Client Physics/Prefabs/AvatarTemplate.prefab"));
            }

            if (GUILayout.Button("Update Templates"))
            {
                UpdateTemplate();
            }
        }
    }

    void GetDictionary()
    {
        Dictionary<HumanBodyBones, GameObject> chosenBones = new Dictionary<HumanBodyBones, GameObject>();
        Animator animator = ((GameObject)template).GetComponent<Animator>();
        Animator animatorMultiple = ((GameObject)templateMultiple).GetComponent<Animator>();
        BodyGroups bodyGroups = new BodyGroups(animator);
        switch (bodyGroup)
        {
            case BodyGroups.BODYGROUP.ALL_COMBINED: chosenBones = bodyGroups.AllCombined(); break;
            case BodyGroups.BODYGROUP.HEAD: chosenBones = bodyGroups.Head(); break;
            case BodyGroups.BODYGROUP.LEFT_ARM: chosenBones = bodyGroups.LeftArm(); break;
            case BodyGroups.BODYGROUP.LEFT_FOOT: chosenBones = bodyGroups.LeftFoot(); break;
            case BodyGroups.BODYGROUP.LEFT_HAND: chosenBones = bodyGroups.LeftHand(); break;
            case BodyGroups.BODYGROUP.LEFT_LEG: chosenBones = bodyGroups.LeftLeg(); break;
            case BodyGroups.BODYGROUP.RIGHT_ARM: chosenBones = bodyGroups.RightArm(); break;
            case BodyGroups.BODYGROUP.RIGHT_FOOT: chosenBones = bodyGroups.RightFoot(); break;
            case BodyGroups.BODYGROUP.RIGHT_HAND: chosenBones = bodyGroups.RightHand(); break;
            case BodyGroups.BODYGROUP.RIGHT_LEG: chosenBones = bodyGroups.RightLeg(); break;
            case BodyGroups.BODYGROUP.TRUNK: chosenBones = bodyGroups.Trunk(); break;
            case BodyGroups.BODYGROUP.TRUNK_HEAD: chosenBones = bodyGroups.TrunkHead(); break;
            default: break;
        }
        //TODO: Clear bones of not selected groups
        //gameObjectsPerBone.Keys.ToList().Clear();
        //gameObjectsPerBone.Values.ToList().Clear();
        //gameObjectsPerBone.Keys.ToList().AddRange(chosenBones.Keys);
        /*
        foreach (HumanBodyBones bone in System.Enum.GetValues(typeof(HumanBodyBones)))
        {
            //LastBone is not mapped to a bodypart, we need to skip it.
            if (bone != HumanBodyBones.LastBone && !gameObjectsPerBone.ContainsKey(bone))
            {
                if (chosenBones.ContainsKey(bone))
                {
                    Transform boneTransformAvatar = animator.GetBoneTransform(bone);
                    if (boneTransformAvatar != null)
                    {
                        gameObjectsPerBone.Add(bone, boneTransformAvatar.gameObject);
                        GetJointSettings(bone);
                    }
                }
                else
                {
                    gameObjectsPerBone.Remove(bone);
                }
            }
        }
        */
        foreach(HumanBodyBones bone in System.Enum.GetValues(typeof(HumanBodyBones)))
        {
            if (!bone.Equals(HumanBodyBones.LastBone))
            {
                if (chosenBones.ContainsKey(bone))
                {
                    Transform boneTransformAvatar = animator.GetBoneTransform(bone);
                    Transform boneTransformAvatarMultiple = animatorMultiple.GetBoneTransform(bone);

                    if (boneTransformAvatar != null && boneTransformAvatarMultiple != null && !gameObjectsPerBoneTemplate.ContainsKey(bone))
                    {
                        gameObjectsPerBoneTemplate.Add(bone, boneTransformAvatar.gameObject);
                        gameObjectsPerBoneTemplateMultiple.Add(bone, boneTransformAvatarMultiple.gameObject);
                        GetJointSettings(bone);
                    }
                }
                else
                {
                    if (gameObjectsPerBoneTemplate.ContainsKey(bone))
                    {
                        gameObjectsPerBoneTemplate.Remove(bone);
                        gameObjectsPerBoneTemplateMultiple.Remove(bone);
                        jointSettings.Remove(bone);
                        jointSettingsNoLeft.Remove(bone);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Returns the JointSettings of a specified bone. Use only if a single ConfigurableJoint is attached to the bone.
    /// </summary>
    /// <param name="bone"></param>
    void GetJointSettings(HumanBodyBones bone)
    {
        if (!jointSettings.ContainsKey(bone))
        {
            ConfigurableJoint joint = gameObjectsPerBoneTemplate[bone].GetComponent<ConfigurableJoint>();
            if (joint != null)
            {
                JointSettings setting = new JointSettings(bone, gameObjectsPerBoneTemplate[bone].GetComponent<ConfigurableJoint>().angularXDrive, gameObjectsPerBoneTemplate[bone].GetComponent<ConfigurableJoint>().angularYZDrive);
                jointSettings.Add(bone, setting);
                if (!setting.bone.ToString().StartsWith("Left"))
                {
                    jointSettingsNoLeft.Add(bone, setting);
                }
            }
        }
    }
    /// <summary>
    /// Shows all JointSettings in Editor.
    /// </summary>
    void DisplayJointSettings()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);
        Dictionary<HumanBodyBones, JointSettings> dict;

        dict = mirror ? jointSettingsNoLeft : jointSettings;

        //Display Joint Settings
        foreach (HumanBodyBones bone in dict.Keys)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(indent);
            EditorGUILayout.BeginVertical();

            DisplayJointSettingsOfBone(jointSettings[bone]);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

        }
        EditorGUILayout.EndScrollView();
    }
    /// <summary>
    /// Shows single JointSettings in Editor.
    /// </summary>
    /// <param name="boneSettings">The JointSettings to show.</param>
    void DisplayJointSettingsOfBone(JointSettings boneSettings)
    {
        boneSettings.showInEditor = EditorGUILayout.Foldout(boneSettings.showInEditor, setGlobalJointSetting ? "Global Joint" : boneSettings.bone.ToString(), true);

        if (boneSettings.showInEditor || setGlobalJointSetting)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(indent);
            EditorGUILayout.BeginVertical();

            DisplayJointValues(boneSettings);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
    }
    /// <summary>
    /// Shows the values of a JointSettings.
    /// </summary>
    /// <param name="settings"></param>
    void DisplayJointValues(JointSettings settings)
    {
        //More functions could be added here, if other joint parameters are to be modified in the future
        DisplayAngularDrives(settings);
    }

    void DisplayAngularDrives(JointSettings settings)
    {
        string globalPrefix = "";
        if (setGlobalJointSetting) globalPrefix = "Global ";

        //Angular X Drive
        settings.showAngularXDriveInEditor = EditorGUILayout.Foldout(settings.showAngularXDriveInEditor, globalPrefix + "Angular X Drive", true);
        if (settings.showAngularXDriveInEditor || setGlobalJointSetting)
        {
            DisplayAngularDrivesHelper(settings, 0);
        }

        //Angular YZ Drive
        settings.showAngularYZDriveInEditor = EditorGUILayout.Foldout(settings.showAngularYZDriveInEditor, globalPrefix + "Angular YZ Drive", true);
        if (settings.showAngularYZDriveInEditor || setGlobalJointSetting)
        {
            DisplayAngularDrivesHelper(settings, 1);
        }
    }

    void DisplayAngularDrivesHelper(JointSettings settings, int index)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(indent);
        EditorGUILayout.BeginVertical();



        //Angular X Drive
        if (index == 0)
        {
            settings.angularXDriveSpring = EditorGUILayout.FloatField("Spring", settings.angularXDriveSpring);
            settings.angularXDriveDamper = EditorGUILayout.FloatField("Damper", settings.angularXDriveDamper);
            settings.maxForceX = EditorGUILayout.FloatField("Maximum Force", settings.maxForceX);
        }
        else
        //Angular YZ Drive
        {
            settings.angularYZDriveSpring = EditorGUILayout.FloatField("Spring", settings.angularYZDriveSpring);
            settings.angularYZDriveDamper = EditorGUILayout.FloatField("Damper", settings.angularYZDriveDamper);
            settings.maxForceYZ = EditorGUILayout.FloatField("Maximum Force", settings.maxForceYZ);
        }

        if (setGlobalJointSetting)
        {
            angularXDriveSpringGlobal = settings.angularXDriveSpring;
            angularXDriveDamperGlobal = settings.angularXDriveDamper;
            maxForceXGlobal = settings.maxForceX;

            angularYZDriveSpringGlobal = settings.angularYZDriveSpring;
            angularYZDriveDamperGlobal = settings.angularYZDriveDamper;
            maxForceYZGlobal = settings.maxForceYZ;
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }
    /// <summary>
    /// Shows values of the global JointSettings in the editor. These settings apply to all joints if setGlobalJointSettings is chosen.
    /// </summary>
    void DisplayGlobalJoint()
    {
 
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();

            DisplayJointSettingsOfBone(globalSettings);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

    }

    void UpdateTemplate()
    {
        //Mass
        bodyMass = new BodyMass(bodyWeight, gameObjectsPerBoneTemplate, mode);
        bodyMass.SetBodyMasses();

        //Joint Settings
        UpdateJoints();

        //Apply template changes to multiple joint template 
        CopyToMultipleJointsTemplate();

        //Refresh Values
        RefreshJointSettings();
        
        //editor.Repaint();
    }

    void RefreshJointSettings()
    {
        foreach (HumanBodyBones bone in gameObjectsPerBoneTemplate.Keys)
        {
            jointSettings[bone].SetAngularXDrive(gameObjectsPerBoneTemplate[bone].GetComponent<ConfigurableJoint>().angularXDrive);
            jointSettings[bone].SetAngularYZDrive(gameObjectsPerBoneTemplate[bone].GetComponent<ConfigurableJoint>().angularYZDrive);
        }
    }

    void CopyToMultipleJointsTemplate()
    {
        //we have to tell the JointSetup that we are using it in the context of the editor (we have no configjointmanager)
        JointSetup setup = new JointSetup(gameObjectsPerBoneTemplateMultiple, gameObjectsPerBoneTemplate, null, true);

        foreach (HumanBodyBones bone in gameObjectsPerBoneTemplate.Keys)
        {
            //we destroy previous components of the multiple joints template to assure a clean copy of the component
            foreach (ConfigurableJoint joint in gameObjectsPerBoneTemplateMultiple[bone].GetComponents<ConfigurableJoint>())
            {
                DestroyImmediate(joint);
            }
            foreach (Collider collider in gameObjectsPerBoneTemplateMultiple[bone].GetComponents<Collider>())
            {
                DestroyImmediate(collider);
            }
            //setup.CopyPasteTemplateJoint(bone);
            setup.AddJointFromTemplate(bone);
        }
    }

    void UpdateJoints()
    {
        //TODO Safe Joint Settings
        foreach (HumanBodyBones bone in gameObjectsPerBoneTemplate.Keys)
        {
            ConfigurableJoint joint = gameObjectsPerBoneTemplate[bone].GetComponent<ConfigurableJoint>();
            if (joint != null)
            {
                joint.enableCollision = useCollisions;
                joint.enablePreprocessing = !noPreprocessing;
                JointSettings settings = globalSettings;
                if (!setGlobalJointSetting)
                {
                    JointSettings specificSettings;
                    if (jointSettings.TryGetValue(bone, out specificSettings))
                    {
                        if (mirror)
                        {
                            if (specificSettings.bone.ToString().StartsWith("Left"))
                            {
                                specificSettings = jointSettings[LeftToRightMapping(bone)];
                            }
                        }
                        settings = specificSettings;
                    }
                }
                AddGravity(bone);
                ApplyJointSetting(joint, settings);
            }
        }
    }
    /// <summary>
    /// Returns the body part on the right side for a body part on the left side.
    /// </summary>
    /// <param name="bone">The body part on the left side.</param>
    /// <returns></returns>
    HumanBodyBones LeftToRightMapping(HumanBodyBones bone)
    {
        switch (bone)
        {
            case HumanBodyBones.LeftFoot: return HumanBodyBones.RightFoot;
            case HumanBodyBones.LeftHand: return HumanBodyBones.RightHand;
            case HumanBodyBones.LeftIndexDistal: return HumanBodyBones.RightIndexDistal;
            case HumanBodyBones.LeftIndexIntermediate: return HumanBodyBones.RightIndexIntermediate;
            case HumanBodyBones.LeftIndexProximal: return HumanBodyBones.RightIndexProximal;
            case HumanBodyBones.LeftLittleDistal: return HumanBodyBones.RightLittleDistal;
            case HumanBodyBones.LeftLittleIntermediate: return HumanBodyBones.RightLittleIntermediate;
            case HumanBodyBones.LeftLittleProximal: return HumanBodyBones.RightLittleProximal;
            case HumanBodyBones.LeftLowerArm: return HumanBodyBones.RightLowerArm;
            case HumanBodyBones.LeftLowerLeg: return HumanBodyBones.RightLowerLeg;
            case HumanBodyBones.LeftMiddleDistal: return HumanBodyBones.RightMiddleDistal;
            case HumanBodyBones.LeftMiddleIntermediate: return HumanBodyBones.RightMiddleIntermediate;
            case HumanBodyBones.LeftMiddleProximal: return HumanBodyBones.RightMiddleProximal;
            case HumanBodyBones.LeftRingDistal: return HumanBodyBones.RightRingDistal;
            case HumanBodyBones.LeftRingIntermediate: return HumanBodyBones.RightRingIntermediate;
            case HumanBodyBones.LeftRingProximal: return HumanBodyBones.RightRingProximal;
            case HumanBodyBones.LeftShoulder: return HumanBodyBones.RightShoulder;
            case HumanBodyBones.LeftThumbDistal: return HumanBodyBones.RightThumbDistal;
            case HumanBodyBones.LeftThumbIntermediate: return HumanBodyBones.RightThumbIntermediate;
            case HumanBodyBones.LeftThumbProximal: return HumanBodyBones.RightThumbProximal;
            case HumanBodyBones.LeftToes: return HumanBodyBones.RightToes;
            case HumanBodyBones.LeftUpperArm: return HumanBodyBones.RightUpperArm;
            case HumanBodyBones.LeftUpperLeg: return HumanBodyBones.RightUpperLeg;
            default: return bone;
        }
    }

    void ApplyJointSetting(ConfigurableJoint joint, JointSettings setting)
    {
        JointDrive tmp;

        tmp = joint.angularXDrive;
        tmp.positionSpring = setting.angularXDriveSpring;
        tmp.positionDamper = setting.angularXDriveDamper;
        tmp.maximumForce = setting.maxForceX;
        joint.angularXDrive = tmp;

        tmp = joint.angularYZDrive;
        tmp.positionSpring = setting.angularYZDriveSpring;
        tmp.positionDamper = setting.angularYZDriveDamper;
        tmp.maximumForce = setting.maxForceYZ;
        joint.angularYZDrive = tmp;

        tmp.positionSpring = 0;
        tmp.positionDamper = 0;
        tmp.maximumForce = 0;

        joint.xDrive = joint.yDrive = joint.zDrive = tmp;
    }

    void AddGravity(HumanBodyBones bone)
    {
        Rigidbody rb = gameObjectsPerBoneTemplate[bone].GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = useGravity;
        }
    }
    /* Legacy
    void AddColliders()
    {
        foreach (HumanBodyBones bone in gameObjectsPerBone.Keys)
        {
            if (gameObjectsPerBone[bone].GetComponent<Collider>() == null)
            {
                switch (bone)
                {
                    //SphereColliders
                    case HumanBodyBones.Head:
                    case HumanBodyBones.LeftShoulder:
                    case HumanBodyBones.RightShoulder: gameObjectsPerBone[bone].AddComponent<SphereCollider>(); return;
                    //Box Colliders
                    case HumanBodyBones.Hips:
                    case HumanBodyBones.Spine:
                    case HumanBodyBones.Chest:
                    case HumanBodyBones.UpperChest: gameObjectsPerBone[bone].AddComponent<BoxCollider>(); return;
                    //Capsule Colliders
                    default: gameObjectsPerBone[bone].AddComponent<CapsuleCollider>(); return;
                }
            }
        }
    }

    void RemoveColliders()
    {
        foreach (HumanBodyBones bone in gameObjectsPerBone.Keys)
        {
            Collider collider = gameObjectsPerBone[bone].GetComponent<Collider>();
            if (collider != null)
            {
                DestroyImmediate(collider);
            }
        }
    }
    */
}
