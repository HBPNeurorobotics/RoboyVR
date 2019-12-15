using System.Collections;
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
    string test = "test";
    bool useCollisions;
    bool noPreprocessing = true;
    bool hasColliders = false;
    bool setGlobalJointSetting = false;
    bool mirror;
    bool showJointSettings = true;
    bool useGravity = true;

    BodyGroups.BODYGROUP bodyGroup;

    JointSettings globalSettings = new JointSettings();

    static EditAvatarTemplate editor;

    Vector2 scroll;

    Object template;
    Dictionary<HumanBodyBones, GameObject> gameObjectsPerBone = new Dictionary<HumanBodyBones, GameObject>();

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
        EditorGUILayout.HelpBox("Assign the rig of AvatarTemplate", MessageType.Info);


        template = EditorGUILayout.ObjectField(template, typeof(GameObject), true);

        if (template != null)
        {
            EditorGUILayout.BeginVertical();
            bodyWeight = EditorGUILayout.FloatField("Total Body Weight", bodyWeight);
            mode = (BodyMass.MODE)EditorGUILayout.EnumPopup("Population Group of Avatar", mode);

            bodyGroup = (BodyGroups.BODYGROUP)EditorGUILayout.EnumPopup("Body Group", bodyGroup);
            GetDictionary();

            hasColliders = EditorGUILayout.Toggle("Add Colliders", hasColliders);
            /*
            if (hasColliders)
            {
                AddColliders();
            }
            else
            {
                RemoveColliders();
            }
            */
            useGravity = EditorGUILayout.Toggle("Enable Gravity", useGravity);
            useCollisions = EditorGUILayout.Toggle("Enable Collisions Between Joints", useCollisions);
            noPreprocessing = EditorGUILayout.Toggle("Disable Preprocessing", noPreprocessing);
            setGlobalJointSetting = EditorGUILayout.Toggle("Set Global Joint Settings", setGlobalJointSetting);

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
                bodyMass = new BodyMass(bodyWeight, gameObjectsPerBone, mode);
                bodyMass.RestoreOneValues();
            }

            //TODO
            if (GUILayout.Button("Restore All"))
            {
                foreach (HumanBodyBones bone in gameObjectsPerBone.Keys)
                {
                    PrefabUtility.ResetToPrefabState(gameObjectsPerBone[bone]);
                }
                //template = (GameObject)Instantiate(Resources.Load("Assets/Client Physics/Prefabs/AvatarTemplate.prefab"));
            }

            if (GUILayout.Button("Update Template"))
            {
                UpdateTemplate();
            }
        }
    }

    void GetDictionary()
    {
        Dictionary<HumanBodyBones, GameObject> chosenBones = new Dictionary<HumanBodyBones, GameObject>();
        Animator animator = ((GameObject)template).GetComponent<Animator>();
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
                    if (boneTransformAvatar != null && !gameObjectsPerBone.ContainsKey(bone))
                    {
                        gameObjectsPerBone.Add(bone, boneTransformAvatar.gameObject);
                        GetJointSettings(bone);
                    }
                }
                else
                {
                    if (gameObjectsPerBone.ContainsKey(bone))
                    {
                        gameObjectsPerBone.Remove(bone);
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
            ConfigurableJoint joint = gameObjectsPerBone[bone].GetComponent<ConfigurableJoint>();
            if (joint != null)
            {
                JointSettings setting = new JointSettings(bone, joint.angularXDrive.positionSpring, joint.angularXDrive.positionDamper, joint.angularXDrive.maximumForce, joint.angularYZDrive.positionSpring, joint.angularYZDrive.positionDamper, joint.angularYZDrive.maximumForce);
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
        boneSettings.showInEditor = EditorGUILayout.Foldout(boneSettings.showInEditor, boneSettings.bone.ToString(), true);

        if (boneSettings.showInEditor)
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
        DisplayAngularDrives(settings);
    }

    void DisplayAngularDrives(JointSettings settings)
    {
        //Angular X Drive
        settings.showAngularXDriveInEditor = EditorGUILayout.Foldout(settings.showAngularXDriveInEditor, "Angular X Drive", true);
        if (settings.showAngularXDriveInEditor)
        {
            DisplayAngularDrivesHelper(settings, 0);
        }

        //Angular YZ Drive
        settings.showAngularYZDriveInEditor = EditorGUILayout.Foldout(settings.showAngularYZDriveInEditor, "Angular YZ Drive", true);
        if (settings.showAngularYZDriveInEditor)
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
            settings.maxForceYZ= EditorGUILayout.FloatField("Maximum Force", settings.maxForceYZ);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }
    /// <summary>
    /// Shows values of the global JointSettings in the editor. These settings apply to all joints if setGlobalJointSettings is chosen.
    /// </summary>
    void DisplayGlobalJoint()
    {
        globalSettings.showInEditor = EditorGUILayout.Foldout(globalSettings.showInEditor, "Global Joint Settings", true);

        if (globalSettings.showInEditor)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(indent);
            EditorGUILayout.BeginVertical();

            DisplayJointValues(globalSettings);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
    }

    void UpdateTemplate()
    {
        //Mass
        bodyMass = new BodyMass(bodyWeight, gameObjectsPerBone, mode);
        bodyMass.SetBodyMasses();

        //Joint Settings
        UpdateJoints();

        //Refresh Values
        editor.Repaint();
    }

    void UpdateJoints()
    {
        //TODO Safe Joint Settings
        foreach (HumanBodyBones bone in gameObjectsPerBone.Keys)
        {
            ConfigurableJoint joint = gameObjectsPerBone[bone].GetComponent<ConfigurableJoint>();
            if (joint != null)
            {
                joint.enableCollision = useCollisions;
                joint.enablePreprocessing = !noPreprocessing;
                if (setGlobalJointSetting)
                {
                    AddGravity(bone);
                    ApplyJointSetting(joint, globalSettings);
                }
                else
                {
                    JointSettings setting;
                    if (jointSettings.TryGetValue(bone, out setting))
                    {
                        if (mirror)
                        {
                            if (setting.bone.ToString().StartsWith("Left"))
                            {
                                setting = jointSettings[LeftToRightMapping(bone)];
                            }
                        }
                        AddGravity(bone);
                        ApplyJointSetting(joint, setting);
                    }
                }
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
        joint.angularXDrive = tmp;

        tmp = joint.angularYZDrive;
        tmp.positionSpring = setting.angularYZDriveSpring;
        tmp.positionDamper = setting.angularYZDriveDamper;
        joint.angularYZDrive = tmp;
    }

    void AddGravity(HumanBodyBones bone)
    {
        Rigidbody rb = gameObjectsPerBone[bone].GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = useGravity;
        }
    }

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
}
